using System.Collections.Immutable;
using System.Text;
using Orthonis.Core;
using Orthonis.Modules;
using Orthonis.Cli;
using App = Orthonis.Cli.Program;

namespace Orthonis.Tests;

public static class Program
{
    private static int passed;
    private static int failed;
    private static readonly ImmutableArray<DiscoveryRequest> Inventory = [new("startup.inventory", 1, null)];
    private static void Equal<T>(T expected, T actual)
    { if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception($"Expected {expected}, got {actual}."); }
    private static void True(bool value) { if (!value) throw new Exception("Assertion failed."); }
    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new Exception($"Expected {typeof(T).Name}.");
    }
    private static async Task ThrowsAsync<T>(Func<Task> action) where T : Exception
    {
        try { await action(); } catch (T) { return; }
        throw new Exception($"Expected {typeof(T).Name}.");
    }
    private static async Task Test(string name, Func<Task> action)
    {
        try { await action(); passed++; Console.WriteLine($"PASS {name}"); }
        catch (Exception e) { failed++; Console.WriteLine($"FAIL {name}: {e.GetType().Name}: {e.Message}"); }
    }
    private static Task Sync(Action action) { action(); return Task.CompletedTask; }
    private static async Task<CaseSnapshot> Initial(string scenario = "missing") =>
        await App.CreateEngine("synthetic:" + scenario).CollectAsync(CaseSnapshot.Create("synthetic:" + scenario), Inventory);

    public static async Task<int> Main()
    {
        await Test("inventory is not a performance measurement", async () =>
        {
            var s = await Initial(); Equal(1, s.Revision); Equal(2, s.Evidence.Length); Equal(0, s.Findings.Length);
            True(s.Evidence.All(e => JsonCodec.Decode<StartupEntry>(JsonCodec.Encode(e.Detail)).Executable == Presence.NotChecked));
        });
        await Test("missing target gets grounded finding, no automatic fix", async () =>
        {
            var s = await Initial(); var next = await App.CreateEngine(s.SourceLabel).CollectAsync(s, [new("startup.inspect", 1, "startup-002")]);
            Equal(2, next.Revision); Equal(1, next.Findings.Length); True(next.Evidence.Any(e => e.Id == next.Findings[0].EvidenceIds[0]));
            Equal(1, s.Revision); Equal(2, s.Evidence.Length);
        });
        await Test("present target has no missing finding", async () =>
        {
            var s = await Initial("healthy"); var next = await App.CreateEngine(s.SourceLabel).CollectAsync(s, [new("startup.inspect", 1, "startup-002")]);
            Equal(0, next.Findings.Length);
        });
        await Test("unknown target status is not missing", async () =>
        {
            var s = await Initial("inconclusive"); var next = await App.CreateEngine(s.SourceLabel).CollectAsync(s, [new("startup.inspect", 1, "startup-002")]);
            Equal(0, next.Findings.Length); Equal(Presence.Unknown, JsonCodec.Decode<StartupEntry>(JsonCodec.Encode(next.Evidence[^1].Detail)).Executable);
        });
        await Test("permission denied is not healthy", async () =>
        {
            var s = await Initial("denied"); Equal(CollectionStatus.PermissionDenied, s.Evidence[0].Status); Equal(0, s.Findings.Length);
        });
        await Test("unavailable target cannot be selected", async () =>
        {
            var s = await Initial("denied"); await ThrowsAsync<RefusalException>(() => App.CreateEngine(s.SourceLabel).CollectAsync(s, [new("startup.inspect", 1, "startup")]));
        });
        await Test("whole batch validation before any calls", async () =>
        {
            var module = new CountingModule(); var engine = new Investigation([module]);
            await ThrowsAsync<RefusalException>(() => engine.CollectAsync(CaseSnapshot.Create("synthetic:healthy"),
                [new("count.inventory", 1, null), new("shell.execute", 1, null)])); Equal(0, module.Calls);
        });
        await Test("unknown target refused", async () =>
        {
            var s = await Initial(); await ThrowsAsync<RefusalException>(() => App.CreateEngine(s.SourceLabel).CollectAsync(s, [new("startup.inspect", 1, "arbitrary-path")]));
        });
        await Test("duplicate requests refused", async () =>
        {
            var s = await Initial(); await ThrowsAsync<RefusalException>(() => App.CreateEngine(s.SourceLabel).CollectAsync(s, [Inventory[0], Inventory[0]]));
        });
        await Test("capability version refused", async () =>
        {
            var s = await Initial(); await ThrowsAsync<RefusalException>(() => App.CreateEngine(s.SourceLabel).CollectAsync(s, [new("startup.inventory", 2, null)]));
        });
        await Test("inventory does not accept target", async () =>
        {
            var s = await Initial(); await ThrowsAsync<RefusalException>(() => App.CreateEngine(s.SourceLabel).CollectAsync(s, [new("startup.inventory", 1, "startup-001")]));
        });
        await Test("cancellation leaves old immutable case intact", async () =>
        {
            var s = await Initial(); using var c = new CancellationTokenSource(); c.Cancel();
            await ThrowsAsync<OperationCanceledException>(() => App.CreateEngine(s.SourceLabel).CollectAsync(s, Inventory, c.Token)); Equal(1, s.Revision);
        });
        await Test("duplicate module refused", () => Sync(() =>
            Throws<RefusalException>(() => new Investigation([new CountingModule(), new CountingModule()]))));
        await Test("strict JSON case round trip", async () =>
        {
            var s = await Initial(); var r = JsonCodec.Decode<CaseSnapshot>(JsonCodec.Encode(s));
            Contract.Validate(r); Equal(JsonCodec.Hash(s), JsonCodec.Hash(r));
        });
        await Test("JSON duplicate escaped key refused", () => Sync(() =>
            Throws<RefusalException>(() => JsonCodec.Decode<DiscoveryRequest>(Encoding.UTF8.GetBytes("{\"capabilityId\":\"x\",\"capability\\u0049d\":\"y\",\"capabilityVersion\":1,\"targetId\":null}")))));
        await Test("JSON unknown field refused", () => Sync(() =>
            Throws<RefusalException>(() => JsonCodec.Decode<DiscoveryRequest>(Encoding.UTF8.GetBytes("{\"capabilityId\":\"x\",\"capabilityVersion\":1,\"targetId\":null,\"command\":\"anything\"}")))));
        await Test("JSON missing required field refused", () => Sync(() =>
            Throws<RefusalException>(() => JsonCodec.Decode<DiscoveryRequest>(Encoding.UTF8.GetBytes("{\"capabilityId\":\"x\"}")))));
        await Test("JSON null nonnullable refused", () => Sync(() =>
            Throws<RefusalException>(() => JsonCodec.Decode<DiscoveryRequest>(Encoding.UTF8.GetBytes("{\"capabilityId\":null,\"capabilityVersion\":1,\"targetId\":null}")))));
        await Test("JSON excessive number refused", () => Sync(() =>
            Throws<RefusalException>(() => JsonCodec.Decode<DiscoveryRequest>(Encoding.UTF8.GetBytes("{\"capabilityId\":\"x\",\"capabilityVersion\":1e999,\"targetId\":null}")))));
        await Test("oversized input refused", () => Sync(() =>
            Throws<RefusalException>(() => JsonCodec.Decode<DiscoveryRequest>(new byte[JsonCodec.MaxPlanBytes + 1], JsonCodec.MaxPlanBytes))));
        await Test("case finding must cite actual evidence", async () =>
        {
            var s = await Initial(); Throws<RefusalException>(() => Contract.Validate(s with
                { Findings = [new("finding-bad", Severity.Attention, "Bad reference", ["missing-id"])] }));
        });
        await Test("disk persistence refuses stale revision and preserves bytes", async () =>
        {
            var root = Path.Combine(Path.GetTempPath(), "orthonis-test-" + Guid.NewGuid().ToString("N"));
            try
            {
                var s = await Initial(); using var store = new CaseStore(root, create: true); store.Save(s, null);
                var before = File.ReadAllBytes(Path.Combine(root, "case.json"));
                Throws<RefusalException>(() => store.Save(s with { Revision = 2 }, 0));
                True(before.SequenceEqual(File.ReadAllBytes(Path.Combine(root, "case.json")))); Equal(1, store.Load().Revision);
                Throws<RefusalException>(() => store.Save(s, null));
            }
            finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
        });
        await Test("case store excludes simultaneous cooperative writer", () => Sync(() =>
        {
            var root = Path.Combine(Path.GetTempPath(), "orthonis-test-" + Guid.NewGuid().ToString("N"));
            try { using var store = new CaseStore(root, create: true); Throws<IOException>(() => { using var other = new CaseStore(root); }); }
            finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
        }));
        await Test("report labels fixtures and unknown coverage", async () =>
        {
            var s = await Initial(); var report = Report.Render(s, App.CreateEngine(s.SourceLabel).Capabilities);
            True(report.Contains("SYNTHETIC DEMONSTRATION", StringComparison.Ordinal));
            True(report.Contains("does not establish a healthy PC", StringComparison.Ordinal));
            True(report.Contains(JsonCodec.Hash(s), StringComparison.Ordinal));
        });
        Console.WriteLine($"RESULT {passed} passed; {failed} failed. Synthetic tests only.");
        return failed == 0 ? 0 : 1;
    }

    private sealed class CountingModule : IDiagnosticModule
    {
        public int Calls { get; private set; }
        public string Id => "count";
        public ImmutableArray<Capability> Capabilities => [new("count.inventory", 1, Id, "Test only", null)];
        public Task<CollectionResult> CollectAsync(DiscoveryRequest request, CancellationToken cancellationToken)
        { Calls++; throw new InvalidOperationException("Test collector failure"); }
    }
}
