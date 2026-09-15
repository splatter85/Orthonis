using System.Collections.Immutable;
using System.Text;
using System.Xml;
using Orthonis.Core;
using Orthonis.Cli;
using Orthonis.Windows;
using CliProgram = Orthonis.Cli.Program;

namespace Orthonis.Tests;

public static class ReliabilityChecks
{
    private const string Sentinel = "PRIVATE-OFC4-PROVIDER";
    private static readonly DateTimeOffset Occurred = new(2026, 9, 15, 0, 0, 0, TimeSpan.Zero);
    private static int checks;
    private static void Check(bool value, string name)
    {
        if (!value) throw new InvalidOperationException("OFC4 regression: " + name);
        checks++;
    }
    private static void Refused(Action action, string name)
    {
        try { action(); } catch (RefusalException) { Check(true, name); return; }
        throw new InvalidOperationException("OFC4 expected refusal: " + name);
    }
    private static string Xml(ulong id = 42, DateTimeOffset? occurred = null, string version = "<Version>0</Version>") =>
        $"<Event xmlns='http://schemas.microsoft.com/win/2004/08/events/event'><System><Provider Name='{Sentinel}'/>" +
        $"<EventID>1000</EventID>{version}<Level>2</Level><TimeCreated SystemTime='{(occurred ?? Occurred):yyyy-MM-ddTHH:mm:ss.fffffffZ}'/>" +
        $"<EventRecordID>{id}</EventRecordID><Channel>Application</Channel><Computer>PRIVATE-COMPUTER</Computer>" +
        "</System><EventData><Data Name='AppName'>PRIVATE-PAYLOAD.exe</Data></EventData></Event>";
    private sealed class Source : IApplicationEventLog
    {
        public CollectionStatus Status = CollectionStatus.Observed;
        public bool Changed;
        public bool ChangeAfterRead;
        public bool Duplicates;
        public bool Unknown;
        public bool MissingBoundary;
        public bool Gap;
        public bool ThrowPermission;
        public bool ThrowSecurity;
        public int Calls;
        public int ContextCalls;
        public int DelayMilliseconds;
        public ulong Record = 42;
        public DateTimeOffset Time = DateTimeOffset.UtcNow.AddHours(-1);
        public SourceContext Context(string id)
        {
            ContextCalls++;
            return new(SourceMode.WindowsReliability, SourceScope.ApplicationEventLog,
                RunIdentity.Hash(new[] { id, Changed ? "changed" : "machine" }), RunIdentity.Hash(new[] { id, "user" }));
        }
        public ApplicationRead Read(DateTimeOffset from, DateTimeOffset to, CancellationToken token)
        {
            Calls++;
            if (ThrowPermission) throw new UnauthorizedAccessException(Sentinel);
            if (ThrowSecurity) throw new System.Security.SecurityException(Sentinel);
            if (DelayMilliseconds > 0) { if (token.WaitHandle.WaitOne(DelayMilliseconds)) token.ThrowIfCancellationRequested(); }
            token.ThrowIfCancellationRequested();
            var item = ReliabilityData.Parse(Xml(Record, Time, Unknown ? "" : "<Version>0</Version>"));
            ImmutableArray<ApplicationEvent> records = Status is CollectionStatus.Observed or CollectionStatus.Limited ?
                (Duplicates ? [item, item] : [item]) : [];
            var boundary = MissingBoundary ? null : new LogBoundary(from.AddDays(-10), 1, 100,
                Gap ? from.AddDays(1) : from.AddDays(-9), RunIdentity.Hash("oldest"));
            if (ChangeAfterRead) Changed = true;
            return new(Status, Status == CollectionStatus.Limited ? 65 : records.Length, 0, records, boundary, boundary);
        }
    }
    private static CaseSnapshot New(Source source)
    {
        var id = $"case-{Guid.NewGuid():N}";
        return new(2, id, 0, "windows:reliability", DateTimeOffset.UtcNow, [], [], [], source.Context(id));
    }
    private static Investigation Engine(Source source, TimeSpan? timeout = null) =>
        new([new WindowsReliabilityModule(source)], SourceMode.WindowsReliability, timeout);
    private static Task<CaseSnapshot> Collect(Source source, CaseSnapshot snapshot, CancellationToken token = default) =>
        Engine(source).CollectAsync(snapshot, [new("reliability.summary", 2, null)], token);

    public static async Task Run()
    {
        var parsed = ReliabilityData.Parse(Xml());
        Check(parsed.RecordId == 42 && parsed.OccurredAt == Occurred && parsed.Provider == Sentinel && parsed.KnownEnvelope, "identity and occurrence time");
        Check(!ReliabilityData.Parse(Xml(version: "")).KnownEnvelope, "missing schema is unknown");
        Check(!ReliabilityData.Parse(Xml(version: "<Version>99</Version>")).KnownEnvelope, "future schema is unknown");
        Refused(() => ReliabilityData.Parse(Xml().Replace("<EventRecordID>42</EventRecordID>", "", StringComparison.Ordinal)), "missing identity");
        Refused(() => ReliabilityData.Parse(Xml().Replace("<EventID>1000</EventID>", "<EventID>1000</EventID><EventID>1</EventID>", StringComparison.Ordinal)), "duplicate identity");
        Refused(() => ReliabilityData.Parse(new string('x', ReliabilityData.MaxXmlBytes)), "bounded XML");
        try { ReliabilityData.Parse("<!DOCTYPE Event [<!ENTITY x SYSTEM 'file:///private'>]>" + Xml()); throw new InvalidOperationException("DTD accepted"); }
        catch (XmlException) { Check(true, "DTD prohibited"); }
        var source = new Source();
        var empty = New(source);
        var first = await Collect(source, empty).ConfigureAwait(false);
        Check(first.Evidence[0].Status == CollectionStatus.Observed && first.Evidence[0].Coverage!.State == CoverageState.Complete, "complete selected metadata query");
        Check(!Encoding.UTF8.GetString(JsonCodec.Encode(first)).Contains("PRIVATE-PAYLOAD", StringComparison.Ordinal), "payload not persisted");
        Check(!ReliabilityData.Summary(first).Contains(Sentinel, StringComparison.Ordinal), "provider not printed");
        Check(first.Findings.IsEmpty, "events are not diagnoses");
        var second = await Collect(source, first).ConfigureAwait(false);
        Check(ReliabilityData.DistinctRecords(second).Length == 1 && second.Evidence.Length == 2, "overlapping history deduplicated without deleting queries");
        var decoded = JsonCodec.Decode<CaseSnapshot>(JsonCodec.Encode(second));
        Check(ReliabilityData.DistinctRecords(decoded).Length == 1, "dedup survives persistence");
        source.Time = source.Time.AddMinutes(1);
        var reused = await Collect(source, second).ConfigureAwait(false);
        Check(ReliabilityData.DistinctRecords(reused).Length == 2 && ReliabilityData.Query(reused.Evidence[^1]).Gaps.HasFlag(ReliabilityGap.RecordIdReused), "reused record ID is not merged");
        source = new Source { Duplicates = true };
        var duplicate = await Collect(source, New(source)).ConfigureAwait(false);
        Check(ReliabilityData.Query(duplicate.Evidence[0]).Read.Records.Length == 1 && duplicate.Evidence[0].Coverage!.Examined == 2, "in-query duplicate bounded");
        foreach (var status in new[] { CollectionStatus.Empty, CollectionStatus.PermissionDenied, CollectionStatus.Failed,
            CollectionStatus.TimedOut, CollectionStatus.Unavailable, CollectionStatus.Unsupported, CollectionStatus.Stale, CollectionStatus.Limited })
        {
            source = new Source { Status = status };
            var value = await Collect(source, New(source)).ConfigureAwait(false);
            Check(value.Evidence[0].Status == status, "explicit outcome " + status);
            Check(status == CollectionStatus.Empty || value.Evidence[0].Coverage!.State == CoverageState.Partial, "incomplete outcome " + status);
        }
        foreach (var flag in new[] { "unknown", "metadata", "history" })
        {
            source = new Source { Unknown = flag == "unknown", MissingBoundary = flag == "metadata", Gap = flag == "history" };
            var value = await Collect(source, New(source)).ConfigureAwait(false);
            Check(value.Evidence[0].Coverage!.State == CoverageState.Partial, "gap " + flag);
        }
        source = new Source();
        empty = New(source);
        source.Changed = true;
        var stale = await Collect(source, empty).ConfigureAwait(false);
        Check(stale.Evidence[0].Status == CollectionStatus.Stale && source.Calls == 0, "wrong machine refuses read");
        source = new Source { ChangeAfterRead = true };
        stale = await Collect(source, New(source)).ConfigureAwait(false);
        Check(stale.Evidence[0].Status == CollectionStatus.Stale && ReliabilityData.DistinctRecords(stale).IsEmpty, "post-read source drift discards evidence");
        foreach (var security in new[] { false, true })
        {
            source = new Source { ThrowPermission = !security, ThrowSecurity = security };
            var denied = await Collect(source, New(source)).ConfigureAwait(false);
            Check(denied.Evidence[0].Status == CollectionStatus.PermissionDenied && !ReliabilityData.Summary(denied).Contains(Sentinel, StringComparison.Ordinal), "permission outcome privacy");
        }
        source = new Source { DelayMilliseconds = 200 };
        var timed = await Engine(source, TimeSpan.FromMilliseconds(10)).CollectAsync(New(source), [new("reliability.summary", 2, null)]).ConfigureAwait(false);
        Check(timed.Evidence[0].Status == CollectionStatus.TimedOut, "coordinator timeout");
        source = new Source();
        using (var cancel = new CancellationTokenSource())
        {
            cancel.Cancel();
            try { await Collect(source, New(source), cancel.Token).ConfigureAwait(false); throw new InvalidOperationException("Cancellation lost"); }
            catch (OperationCanceledException) { Check(source.Calls == 0, "caller cancellation"); }
        }
        Refused(() => SourceData.RequireExportableFixture(first), "Core live export blocked");
        Refused(() => Contract.Validate(first with { Source = first.Source! with { Scope = SourceScope.RunNative64 } }), "mode/scope mismatch");
        Refused(() => Contract.Validate(first with { Source = first.Source! with { Mode = SourceMode.WindowsStartup } }), "source mode mismatch");
        Refused(() => Engine(source).ValidateRequests(first, [new("reliability.summary", 1, null)]), "no version downgrade");
        Refused(() => Engine(source).ValidateRequests(first, [new("reliability.summary", 2, "arbitrary")]), "no arbitrary targets");
        await CliChecks().ConfigureAwait(false);
        Console.WriteLine($"PASS: {checks} OFC4 controlled checks; no native event writes or live data output.");
    }
    private static async Task CliChecks()
    {
        var directory = Path.Combine(Path.GetTempPath(), "orthonis-ofc4-" + Guid.NewGuid().ToString("N"));
        var source = new Source();
        var operations = new CaseOperations(reliabilityFactory: () => source);
        try
        {
            using var output = new StringWriter();
            using var error = new StringWriter();
            Check(await CliProgram.RunAsync(["start-reliability", directory], output, error, operations).ConfigureAwait(false) == 0, "CLI start");
            Check(!output.ToString().Contains(Sentinel, StringComparison.Ordinal), "CLI privacy");
            var path = Path.Combine(directory, "case.json");
            var before = File.ReadAllBytes(path);
            Check(await CliProgram.RunAsync(["report", directory], output, error, operations).ConfigureAwait(false) == 2, "CLI export blocked");
            Check(await CliProgram.RunAsync(["example-plan", directory], output, error, operations).ConfigureAwait(false) == 2, "CLI example blocked");
            var planPath = Path.Combine(directory, "plan.json");
            File.WriteAllText(planPath, operations.LocalPlan(directory, null), new UTF8Encoding(false));
            var calls = source.Calls;
            await operations.Apply(directory, planPath, false, CancellationToken.None).ConfigureAwait(false);
            Check(calls == source.Calls && before.SequenceEqual(File.ReadAllBytes(path)), "preview has no effects");
            Refused(() => operations.LocalPlan(directory, "arbitrary"), "target plan rejected");
            await operations.Apply(directory, planPath, true, CancellationToken.None).ConfigureAwait(false);
            var after = File.ReadAllBytes(path);
            Check(await CliProgram.RunAsync(["apply", directory, planPath, "--approve"], output, error, operations).ConfigureAwait(false) == 2 &&
                after.SequenceEqual(File.ReadAllBytes(path)), "replay leaves saved case unchanged");
            using var store = new CaseStore(directory);
            var value = store.Load();
            Check(value.Revision == 2 && value.AppliedPlanIds.Length == 1 && ReliabilityData.DistinctRecords(value).Length == 1, "saved approved history");
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }
    public static async Task<int> Phase(string[] args)
    {
        if (args is ["--ofc4-native"])
        {
            if (!OperatingSystem.IsWindows()) return 2;
            var directory = Path.Combine(Path.GetTempPath(), "orthonis-ofc4-native-" + Guid.NewGuid().ToString("N"));
            try
            {
                await new CaseOperations().StartReliability(directory, CancellationToken.None).ConfigureAwait(false);
                using var store = new CaseStore(directory);
                var snapshot = store.Load();
                ReliabilityData.ValidateCase(snapshot);
                var e = snapshot.Evidence.Single();
                Check(e.DetailSchema == ReliabilityData.DetailSchema && e.Status is CollectionStatus.Observed or CollectionStatus.Empty or CollectionStatus.Limited or CollectionStatus.Unavailable,
                    "native Application query executed");
                Console.WriteLine("PASS: bounded native Application query; local artifacts removed; no event details logged.");
                return 0;
            }
            catch { Console.Error.WriteLine("FAIL: native Application check; private details suppressed."); return 1; }
            finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        }
        if (args.Length >= 2 && args[0] == "--ofc4-cli")
        {
            var source = new Source { Time = Occurred };
            // Fixed public synthetic event time across processes; query window remains the real seven-day window.
            return await CliProgram.RunAsync(args[1..], Console.Out, Console.Error,
                new CaseOperations(reliabilityFactory: () => source)).ConfigureAwait(false);
        }
        return 2;
    }
}
