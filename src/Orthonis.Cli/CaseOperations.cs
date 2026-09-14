using System.Collections.Immutable;
using System.Text;
using Orthonis.Core;
using Orthonis.Windows;

namespace Orthonis.Cli;

public sealed record StartupAdapters(IRunRegistry Registry, IExecutableProbe Probe);

// Application operations are reusable independently of console parsing. The injection seam is not a CLI option.
public sealed class CaseOperations(Func<StartupAdapters>? startupFactory = null)
{
    private StartupAdapters Adapters() => (startupFactory ?? NativeAdapters)();
    public static StartupAdapters NativeAdapters()
    {
        if (!OperatingSystem.IsWindows() || !WindowsHost.ArchitectureSupported)
            throw new RefusalException("Windows Startup requires a supported native Windows x86/x64 process; no synthetic fallback.");
        return new(new WindowsRunRegistry(), new LocalExecutableProbe());
    }

    public Investigation Engine(CaseSnapshot snapshot)
    {
        Contract.Validate(snapshot);
        if (snapshot.SchemaVersion == 1) return Program.CreateEngine(snapshot.SourceLabel);
        var adapters = Adapters();
        return new([new WindowsStartupModule(adapters.Registry, adapters.Probe)], SourceMode.WindowsStartup);
    }
    public ImmutableArray<Capability> WindowsCapabilities()
    {
        var adapters = Adapters();
        return new WindowsStartupModule(adapters.Registry, adapters.Probe).Capabilities;
    }

    public async Task<string> Start(string directory, string scenario, CancellationToken token)
    {
        var snapshot = CaseSnapshot.Create("synthetic:" + scenario);
        var engine = Engine(snapshot);
        using var store = new CaseStore(directory, create: true);
        var next = await engine.CollectAsync(snapshot, [new("startup.inventory", 1, null), new("reliability.summary", 1, null)], token).ConfigureAwait(false);
        store.Save(next, expectedRevision: null);
        return Report.Render(next, engine.Capabilities, new DiscoveryPlans(engine).Example(next));
    }

    public async Task<string> StartWindows(string directory, CancellationToken token)
    {
        var adapters = Adapters();
        var id = $"case-{Guid.NewGuid():N}";
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(token);
        budget.CancelAfter(TimeSpan.FromSeconds(5));
        SourceContext context;
        try
        {
            context = await Task.Run(() => adapters.Registry.Context(id), budget.Token).WaitAsync(budget.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        { throw new RefusalException("Live source context timed out; no case was created."); }
        var snapshot = new CaseSnapshot(2, id, 0, "windows:startup", DateTimeOffset.UtcNow, [], [], [], context);
        var engine = new Investigation([new WindowsStartupModule(adapters.Registry, adapters.Probe)], SourceMode.WindowsStartup);
        using var store = new CaseStore(directory, create: true);
        var next = await engine.CollectAsync(snapshot, [new("startup.inventory", 2, null)], token).ConfigureAwait(false);
        store.Save(next, expectedRevision: null);
        return RunCase.Summary(next);
    }

    public string Summary(string directory)
    {
        using var store = new CaseStore(directory);
        var snapshot = store.Load();
        return snapshot.SchemaVersion == 2 ? RunCase.Summary(snapshot) : FixtureReport(snapshot);
    }
    public string Export(string directory)
    {
        using var store = new CaseStore(directory);
        return FixtureReport(store.Load());
    }
    private static string FixtureReport(CaseSnapshot snapshot)
    {
        SourceData.RequireExportableFixture(snapshot);
        var engine = Program.CreateEngine(snapshot.SourceLabel);
        return Report.Render(snapshot, engine.Capabilities, new DiscoveryPlans(engine).Example(snapshot));
    }
    public string Example(string directory)
    {
        using var store = new CaseStore(directory);
        var snapshot = store.Load();
        SourceData.RequireExportableFixture(snapshot);
        return Encoding.UTF8.GetString(JsonCodec.Encode(new DiscoveryPlans(Engine(snapshot)).Example(snapshot)));
    }
    public string LocalPlan(string directory, string? target)
    {
        using var store = new CaseStore(directory);
        var snapshot = store.Load();
        RunCase.Validate(snapshot);
        var plans = new DiscoveryPlans(Engine(snapshot));
        var plan = plans.Example(snapshot);
        if (target is not null)
        {
            var evidence = snapshot.Evidence.LastOrDefault(e => e.TargetId == target && e.DetailSchema == "startup-run.v2");
            Contract.Require(evidence is not null, "Unknown local target.");
            plan = plan with { Requests = [new("startup.inspect", 2, target)], EvidenceRefs = [evidence.Id] };
            plans.Preview(snapshot, JsonCodec.Encode(plan));
        }
        return Encoding.UTF8.GetString(JsonCodec.Encode(plan));
    }
    public async Task<string> Apply(string directory, string planPath, bool approved, CancellationToken token)
    {
        var bytes = CaseStore.ReadBounded(planPath, JsonCodec.MaxPlanBytes);
        using var store = new CaseStore(directory); // Lock spans revalidation, collection and save.
        var snapshot = store.Load();
        var engine = Engine(snapshot);
        var plans = new DiscoveryPlans(engine);
        var plan = plans.Preview(snapshot, bytes);
        if (!approved)
            return "PREVIEW ONLY: no collectors run, no case revision changed. Requests:\n" +
                Encoding.UTF8.GetString(JsonCodec.Encode(plan.Requests)) +
                "\nReview the exact requests, then repeat with --approve for local read-only collection.\n";
        var next = await plans.ExecuteAsync(snapshot, bytes, approved: true, token).ConfigureAwait(false);
        store.Save(next, snapshot.Revision);
        return next.SchemaVersion == 2 ? RunCase.Summary(next) : Report.Render(next, engine.Capabilities, plans.Example(next));
    }
}
