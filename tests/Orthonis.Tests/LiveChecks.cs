using System.Collections.Immutable;
using System.Text;
using Orthonis.Core;
using Orthonis.Cli;
using Orthonis.Windows;
using App = Orthonis.Cli.Program;

namespace Orthonis.Tests;

public static partial class Program
{
    private const string Sentinel = "PRIVATE_SENTINEL_7348";
    private static RunValue Value(string command = "\"C:\\PRIVATE_SENTINEL_7348\\app.exe\" --token=PRIVATE_SENTINEL_7348", string name = Sentinel, int kind = 1) =>
        new(name, kind, Encoding.Unicode.GetBytes(command + "\0").ToImmutableArray());
    private static CaseSnapshot LiveCase(FakeRegistry registry)
    {
        var id = $"case-{Guid.NewGuid():N}";
        return new(2, id, 0, "windows:startup", DateTimeOffset.UtcNow, [], [], [], registry.Context(id));
    }
    private static Investigation LiveEngine(FakeRegistry registry, FakeProbe probe, TimeSpan? timeout = null) =>
        new([new WindowsStartupModule(registry, probe)], SourceMode.WindowsStartup, timeout);
    private static async Task<CaseSnapshot> LiveInitial(FakeRegistry registry, FakeProbe probe) =>
        await LiveEngine(registry, probe).CollectAsync(LiveCase(registry), [new("startup.inventory", 2, null)]);
    private static string Target(CaseSnapshot snapshot) => snapshot.Evidence.First(e => e.DetailSchema == "startup-run.v2").TargetId;
    private static async Task<CaseSnapshot> Inspect(CaseSnapshot snapshot, FakeRegistry registry, FakeProbe probe) =>
        await LiveEngine(registry, probe).CollectAsync(snapshot, [new("startup.inspect", 2, Target(snapshot))]);
    private static void Private(string text) => True(!text.Contains(Sentinel, StringComparison.Ordinal));

    public static async Task<int> LiveChecks()
    {
        await Test("OFC3 fixed version 1 bytes/hash and plan remain compatible", async () =>
        {
            var bytes = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", "case-v1.json"));
            var s = JsonCodec.Decode<CaseSnapshot>(bytes); Contract.Validate(s);
            Equal("05e68fd65a42d69b5b5f3e31f6ca21ed1c763f9c8b648f69f857f9cadc99d5b7", JsonCodec.Hash(s));
            True(bytes.SequenceEqual(JsonCodec.Encode(s))); True(s.Source is null);
            var p = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", "plan-v1.json"));
            var next = await new DiscoveryPlans(App.CreateEngine(s.SourceLabel)).ExecuteAsync(s, p, true);
            Equal(2, next.Revision); Equal(1, next.SchemaVersion); Equal(1, next.Findings.Length);
            True(bytes.SequenceEqual(JsonCodec.Encode(s)));
        });
        await Test("OFC3 version 2 source and coverage survive strict round trip", async () =>
        {
            var r = new FakeRegistry(); var p = new FakeProbe(); var s = await LiveInitial(r, p);
            var loaded = JsonCodec.Decode<CaseSnapshot>(JsonCodec.Encode(s)); RunCase.Validate(loaded);
            Equal(JsonCodec.Hash(s), JsonCodec.Hash(loaded)); Equal(2, loaded.SchemaVersion);
            True(loaded.Evidence.All(e => e.Coverage is not null));
            True(loaded.Evidence.Where(e => e.DetailSchema == "startup-run.v2").All(e => RunCase.Entry(e).Enabled == Enablement.Unknown));
            Equal(0, p.Calls);
        });
        await Test("OFC3 native views are explicit and case identities differ", () => Sync(() =>
        {
            var r = new FakeRegistry(); var a = LiveCase(r); var b = LiveCase(r);
            var l = new RunLocator(RunHive.CurrentUser, SourceScope.RunNative64, Sentinel, RunIdentity.Registration(Value()));
            True(RunIdentity.Target(a, l) != RunIdentity.Target(b, l));
            True(RunIdentity.Target(a, l) != RunIdentity.Target(a, l with { Hive = RunHive.LocalMachine }));
            r.Scope = SourceScope.RunNative32; var c = LiveCase(r);
            True(RunIdentity.Target(a, l) != RunIdentity.Target(c, l with { View = SourceScope.RunNative32 }));
        }));
        foreach (var status in new[] { CollectionStatus.Empty, CollectionStatus.PermissionDenied, CollectionStatus.Failed, CollectionStatus.Limited, CollectionStatus.Unsupported })
        {
            await Test("OFC3 explicit registry outcome " + status, async () =>
            {
                var r = new FakeRegistry { Listing = _ => new(status, [], 0) }; var p = new FakeProbe(); var s = await LiveInitial(r, p);
                Equal(2, s.Evidence.Length); True(s.Evidence.All(e => e.Status == status)); Equal(0, s.Findings.Length);
                True(s.Evidence.All(e => e.Coverage!.State == (status == CollectionStatus.Empty ? CoverageState.Complete : CoverageState.Partial)));
                Equal(0, p.Calls);
            });
        }
        await Test("OFC3 partial query retains bounded registrations and limitations", async () =>
        {
            var r = new FakeRegistry { Listing = _ => new(CollectionStatus.Limited, [Value()], 33) };
            var s = await LiveInitial(r, new()); Equal(2, s.Evidence.Count(e => e.DetailSchema == "startup-run.v2"));
            True(s.Evidence.All(e => e.Coverage!.State == CoverageState.Partial)); True(RunCase.Summary(s).Contains("Limited", StringComparison.Ordinal));
        });
        await Test("OFC3 case-insensitive aliases are not counted as separate targets", async () =>
        {
            var r = new FakeRegistry { Listing = _ => new(CollectionStatus.Observed, [Value(name: "A"), Value(name: "a")], 2) };
            var s = await LiveInitial(r, new()); Equal(0, s.Evidence.Count(e => e.DetailSchema == "startup-run.v2"));
            True(s.Evidence.All(e => e.Status == CollectionStatus.Limited));
        });
        await Test("OFC3 excessive source result fails closed", async () =>
        {
            var r = new FakeRegistry { Listing = _ => new(CollectionStatus.Observed, Enumerable.Range(0, 33).Select(i => Value(name: "v" + i)).ToImmutableArray(), 33) };
            await ThrowsAsync<RefusalException>(() => LiveInitial(r, new()));
        });
        await Test("OFC3 direct quoted and unquoted local executable subset", () => Sync(() =>
        {
            Equal(@"C:\Tools\app.exe", RunCommand.Resolve(Value(@"C:\Tools\app.exe --simple=argument")));
            Equal(@"C:\Program Files\app.exe", RunCommand.Resolve(Value("\"C:\\Program Files\\app.exe\" --simple")));
        }));
        foreach (var command in new[]
        {
            @"C:\Program Files\app.exe", "\"C:\\Tools\\app.exe", "\"C:\\Tools\\app.exe\"tail",
            "\"C:\\Tools\\app.exe\" \"quoted argument\"", @"%APPDATA%\app.exe", @"C:\Tools\cmd.exe /c app.exe",
            @"cmd.exe", @"\\server\share\app.exe", @"\\?\C:\Tools\app.exe", @"C:Tools\app.exe",
            @"C:\Tools\..\app.exe", @"C:\Tools\file.exe:stream.exe", @"C:\NUL\app.exe", @"C:\Tools.\app.exe",
            @"C:\Tools\app.cmd", "https://example.invalid/app.exe", @"C:\Tools\app.exe & ignored.exe"
        })
        {
            // Do not put potentially private test inputs into test names/logs.
            await Test("OFC3 unsupported command subset is never resolved", () => Sync(() => True(RunCommand.Resolve(Value(command)) is null)));
        }
        await Test("OFC3 malformed registry strings and types remain unsupported", () => Sync(() =>
        {
            True(RunCommand.Resolve(Value(kind: 3)) is null);
            True(RunCommand.Resolve(Value() with { Data = [65, 0, 66] }) is null);
            True(RunCommand.Resolve(Value() with { Data = Encoding.Unicode.GetBytes(@"C:\Tools\app.exe").ToImmutableArray() }) is null);
            True(RunCommand.Resolve(Value() with { Data = [0, 216, 0, 0] }) is null);
            True(RunCommand.Resolve(Value("C:\\x\0y\\app.exe")) is null);
            True(RunCommand.Resolve(Value(new string('x', 261))) is null);
        }));
        await Test("OFC3 unsupported command causes no filesystem probe", async () =>
        {
            var r = new FakeRegistry { Values = [Value(@"\\server\share\app.exe")] }; var p = new FakeProbe();
            var s = await Inspect(await LiveInitial(r, p), r, p);
            Equal(CollectionStatus.Unsupported, s.Evidence[^1].Status); Equal(0, p.Calls);
        });
        foreach (var presence in new[] { TargetPresence.Present, TargetPresence.Missing, TargetPresence.Inaccessible, TargetPresence.Unknown, TargetPresence.Unsupported })
            await Test("OFC3 distinct file presence " + presence, async () =>
            {
                var r = new FakeRegistry(); var p = new FakeProbe { Presence = presence };
                var s = await Inspect(await LiveInitial(r, p), r, p);
                Equal(presence, RunCase.Entry(s.Evidence[^1]).Presence); Equal(WindowsStartupModule.Status(presence), s.Evidence[^1].Status);
                Equal(presence == TargetPresence.Missing ? 1 : 0, s.Findings.Length);
            });
        await Test("OFC3 changed value is stale and never silently retargeted", async () =>
        {
            var r = new FakeRegistry(); var p = new FakeProbe(); var s = await LiveInitial(r, p);
            r.Values = [Value(@"C:\Other\substitute.exe")];
            var next = await Inspect(s, r, p); Equal(CollectionStatus.Stale, next.Evidence[^1].Status); Equal(0, p.Calls);
            Equal(Target(s), next.Evidence[^1].TargetId);
        });
        await Test("OFC3 removed, substituted-name and ambiguous registrations do not probe", async () =>
        {
            foreach (var read in new[] { new RunRead(CollectionStatus.Unavailable, null), new RunRead(CollectionStatus.Observed, Value(name: "substitute")), new RunRead(CollectionStatus.Limited, null) })
            {
                var r = new FakeRegistry(); var p = new FakeProbe(); var s = await LiveInitial(r, p); r.ReadResult = _ => read;
                var next = await Inspect(s, r, p); True(next.Evidence[^1].Status is CollectionStatus.Stale or CollectionStatus.Unavailable); Equal(0, p.Calls);
            }
        });
        await Test("OFC3 registration drift during file check supersedes presence", async () =>
        {
            var r = new FakeRegistry(); var p = new FakeProbe(); var s = await LiveInitial(r, p);
            r.ReadResult = n => new(CollectionStatus.Observed, n == 1 ? Value() : Value(@"C:\Changed\app.exe"));
            var next = await Inspect(s, r, p); Equal(CollectionStatus.Stale, next.Evidence[^1].Status); Equal(1, p.Calls); Equal(0, next.Findings.Length);
        });
        foreach (var change in new Action<FakeRegistry>[] { r => r.Machine = "other-machine", r => r.User = "other-user", r => r.Scope = SourceScope.RunNative32 })
            await Test("OFC3 wrong machine user or view is not remapped", async () =>
            {
                var r = new FakeRegistry(); var p = new FakeProbe(); var s = await LiveInitial(r, p); change(r);
                var next = await Inspect(s, r, p); Equal(CollectionStatus.Unsupported, next.Evidence[^1].Status); Equal(0, r.Reads); Equal(0, p.Calls);
            });
        await Test("OFC3 source-mode mismatch refuses before collectors", async () =>
        {
            var r = new FakeRegistry(); var p = new FakeProbe(); var s = await LiveInitial(r, p);
            await ThrowsAsync<RefusalException>(() => App.CreateEngine("synthetic:missing").CollectAsync(s, Inventory));
            await ThrowsAsync<RefusalException>(() => LiveEngine(r, p).CollectAsync(CaseSnapshot.Create("synthetic:missing"), [new("startup.inventory", 2, null)]));
            Equal(0, r.Reads); Equal(0, p.Calls);
        });
        await Test("OFC3 malformed persisted locator refuses before source calls", async () =>
        {
            var r = new FakeRegistry(); var p = new FakeProbe(); var s = await LiveInitial(r, p); var e = s.Evidence[0];
            var entry = RunCase.Entry(e); var bad = s with { Evidence = s.Evidence.SetItem(0, e with { Detail = JsonCodec.Element(entry with { Locator = entry.Locator with { ValueName = "substituted" } }) }) };
            var calls = r.Contexts + r.Enumerations + r.Reads;
            await ThrowsAsync<RefusalException>(() => LiveEngine(r, p).CollectAsync(bad, [new("startup.inspect", 2, Target(s))]));
            Equal(calls, r.Contexts + r.Enumerations + r.Reads); Equal(0, p.Calls);
        });
        await Test("OFC3 preview unapproved invalid batch and replay preserve no-call boundary", async () =>
        {
            var r = new FakeRegistry(); var p = new FakeProbe(); var s = await LiveInitial(r, p);
            var plans = new DiscoveryPlans(LiveEngine(r, p)); var plan = plans.Example(s); var before = r.Contexts + r.Enumerations + r.Reads;
            plans.Preview(s, JsonCodec.Encode(plan)); await ThrowsAsync<RefusalException>(() => plans.ExecuteAsync(s, JsonCodec.Encode(plan), false));
            var invalid = plan with { Requests = [new("startup.inventory", 2, null), new("reliability.summary", 1, null)] };
            await ThrowsAsync<RefusalException>(() => plans.ExecuteAsync(s, JsonCodec.Encode(invalid), true));
            Equal(before, r.Contexts + r.Enumerations + r.Reads); Equal(0, p.Calls);
            var next = await plans.ExecuteAsync(s, JsonCodec.Encode(plan), true); var calls = r.Reads;
            await ThrowsAsync<RefusalException>(() => plans.ExecuteAsync(next, JsonCodec.Encode(plan), true)); Equal(calls, r.Reads);
        });
        await Test("OFC3 fresh decoded mapping works and contradictory evidence makes finding historical", async () =>
        {
            var r = new FakeRegistry(); var p = new FakeProbe { Presence = TargetPresence.Missing }; var s = await LiveInitial(r, p);
            var missing = await Inspect(JsonCodec.Decode<CaseSnapshot>(JsonCodec.Encode(s)), new FakeRegistry(), p);
            True(RunCase.Summary(missing).Contains("Current attention findings: 1", StringComparison.Ordinal));
            var present = await Inspect(JsonCodec.Decode<CaseSnapshot>(JsonCodec.Encode(missing)), new FakeRegistry(), new FakeProbe());
            True(RunCase.Summary(present).Contains("Current attention findings: 0; historical findings: 1", StringComparison.Ordinal)); Equal(1, present.Findings.Length);
            True(present.Evidence.Length > missing.Evidence.Length); Equal(0, s.Findings.Length);
        });
        await Test("OFC3 cancellation preserves immutable input and skips work", async () =>
        {
            var r = new FakeRegistry(); var p = new FakeProbe(); var s = await LiveInitial(r, p); var bytes = JsonCodec.Encode(s);
            using var c = new CancellationTokenSource(); c.Cancel(); var calls = r.Contexts;
            await ThrowsAsync<OperationCanceledException>(() => LiveEngine(r, p).CollectAsync(s, [new("startup.inventory", 2, null)], c.Token));
            Equal(calls, r.Contexts); True(bytes.SequenceEqual(JsonCodec.Encode(s)));
        });
        await Test("OFC3 cooperative timeout retains an incomplete outcome not health", async () =>
        {
            var r = new FakeRegistry { Listing = _ => { Thread.Sleep(120); return new(CollectionStatus.Empty, [], 0); } }; var p = new FakeProbe();
            var s = await LiveEngine(r, p, TimeSpan.FromMilliseconds(10)).CollectAsync(LiveCase(r), [new("startup.inventory", 2, null)]);
            Equal(CollectionStatus.TimedOut, s.Evidence[0].Status); Equal(CoverageState.NotQueried, s.Evidence[0].Coverage!.State);
            await Task.Delay(150); Equal(1, s.Evidence.Length); Equal(0, p.Calls);
        });
        await Test("OFC3 unsupported formats and downgrade attempts fail closed", async () =>
        {
            var s = await LiveInitial(new(), new());
            Throws<RefusalException>(() => Contract.Validate(s with { SchemaVersion = 3 }));
            Throws<RefusalException>(() => Contract.Validate(s with { SchemaVersion = 1 }));
            Throws<RefusalException>(() => Contract.Validate(s with { Source = null }));
            Throws<RefusalException>(() => Contract.Validate(s with { Source = s.Source! with { Mode = SourceMode.Synthetic } }));
            Throws<RefusalException>(() => Contract.Validate(s with { Evidence = s.Evidence.SetItem(0, s.Evidence[0] with { Coverage = null }) }));
        });
        await Test("OFC3 core exporter blocks live data and summary omits private fields", async () =>
        {
            var r = new FakeRegistry(); var p = new FakeProbe { Presence = TargetPresence.Missing }; var s = await Inspect(await LiveInitial(r, p), r, p);
            var dirty = s with { Findings = [s.Findings[0] with { Summary = Sentinel }] };
            Throws<RefusalException>(() => Report.Render(dirty, LiveEngine(r, p).Capabilities)); Private(RunCase.Summary(dirty));
            True(Encoding.UTF8.GetString(JsonCodec.Encode(s)).Contains(Sentinel, StringComparison.Ordinal)); // sentinel is really in private locator data
            True(!Encoding.UTF8.GetString(JsonCodec.Encode(s)).Contains("--token", StringComparison.Ordinal)); // commands are not persisted
        });
        await Test("OFC3 live storage failure preserves prior bytes", async () =>
        {
            var root = Path.Combine(Path.GetTempPath(), "orthonis-live-store-" + Guid.NewGuid().ToString("N"));
            try
            {
                var r = new FakeRegistry(); var p = new FakeProbe(); var s = await LiveInitial(r, p);
                using var store = new CaseStore(root, true); store.Save(s, null); var before = File.ReadAllBytes(Path.Combine(root, "case.json"));
                var next = await Inspect(s, r, p);
                Throws<IOException>(() => store.Save(next, s.Revision, () => throw new IOException(Sentinel)));
                True(before.SequenceEqual(File.ReadAllBytes(Path.Combine(root, "case.json")))); Equal(1, store.Load().Revision);
            }
            finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
        });
        await LiveCliChecks();
        Console.WriteLine($"RESULT {passed} passed; {failed} failed. Foundation plus controlled OFC3 sources; not owner-PC acceptance.");
        return failed == 0 ? 0 : 1;
    }

    private sealed class FakeRegistry : IRunRegistry
    {
        public SourceScope Scope { get; set; } = SourceScope.RunNative64;
        public string Machine { get; set; } = Sentinel + "-machine";
        public string User { get; set; } = Sentinel + "-user";
        public ImmutableArray<RunValue> Values { get; set; } = [Value()];
        public Func<RunHive, RunListing>? Listing { get; set; }
        public Func<int, RunRead>? ReadResult { get; set; }
        public Exception? ContextError { get; set; }
        public int Contexts { get; private set; }
        public int Enumerations { get; private set; }
        public int Reads { get; private set; }
        public SourceContext Context(string caseId)
        { Contexts++; if (ContextError is not null) throw ContextError; return RunIdentity.Context(caseId, Scope, Machine, User); }
        public RunListing Enumerate(RunHive hive, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested(); Enumerations++;
            if (Listing is not null) return Listing(hive);
            return hive == RunHive.CurrentUser ? new(Values.Length == 0 ? CollectionStatus.Empty : CollectionStatus.Observed, Values, Values.Length) : new(CollectionStatus.Empty, [], 0);
        }
        public RunRead Read(RunHive hive, string valueName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested(); Reads++;
            if (ReadResult is not null) return ReadResult(Reads);
            var values = Values.Where(v => string.Equals(v.Name, valueName, StringComparison.OrdinalIgnoreCase)).ToArray();
            return values.Length == 1 ? new(CollectionStatus.Observed, values[0]) : new(values.Length == 0 ? CollectionStatus.Unavailable : CollectionStatus.Limited, null);
        }
    }
    private sealed class FakeProbe : IExecutableProbe
    {
        public TargetPresence Presence { get; set; } = TargetPresence.Present;
        public int Calls { get; private set; }
        public TargetPresence Inspect(string executablePath, CancellationToken cancellationToken)
        { cancellationToken.ThrowIfCancellationRequested(); Calls++; return Presence; }
    }
}
