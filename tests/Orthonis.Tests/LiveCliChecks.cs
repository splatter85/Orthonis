using System.Text;
using Orthonis.Core;
using Orthonis.Cli;
using Orthonis.Windows;
using App = Orthonis.Cli.Program;

namespace Orthonis.Tests;

public static partial class Program
{
    private static async Task<(int Code, string Output, string Error)> Command(CaseOperations operations, params string[] args)
    {
        using var output = new StringWriter(); using var error = new StringWriter();
        var code = await App.RunAsync(args, output, error, operations);
        Private(output.ToString()); Private(error.ToString());
        return (code, output.ToString(), error.ToString());
    }

    private static async Task LiveCliChecks()
    {
        await Test("OFC3 CLI all live output routes preserve private-data boundary", async () =>
        {
            var root = Path.Combine(Path.GetTempPath(), "orthonis-live-cli-" + Guid.NewGuid().ToString("N"));
            try
            {
                var r = new FakeRegistry(); var p = new FakeProbe { Presence = TargetPresence.Missing };
                var ops = new CaseOperations(() => new(r, p));
                var created = await Command(ops, "start-windows", root); Equal(0, created.Code);
                True(created.Output.Contains("LOCAL / PRIVATE", StringComparison.Ordinal));
                var before = File.ReadAllBytes(Path.Combine(root, "case.json"));
                Equal(0, (await Command(ops, "summary", root)).Code);
                Equal(2, (await Command(ops, "report", root)).Code);
                Equal(2, (await Command(ops, "example-plan", root)).Code);
                var caps = await Command(ops, "capabilities", "--windows-startup"); Equal(0, caps.Code);
                True(!caps.Output.Contains("reliability", StringComparison.Ordinal));
                var example = await Command(ops, "local-plan", root); Equal(0, example.Code);
                var plan = JsonCodec.Decode<DiscoveryPlan>(Encoding.UTF8.GetBytes(example.Output));
                plan = plan with { Hypothesis = Sentinel }; // even valid imported free text must not be echoed
                var path = Path.Combine(root, "plan.json"); File.WriteAllBytes(path, JsonCodec.Encode(plan));
                var calls = r.Contexts + r.Enumerations + r.Reads;
                Equal(0, (await Command(ops, "apply", root, path)).Code);
                Equal(calls, r.Contexts + r.Enumerations + r.Reads); Equal(0, p.Calls);
                True(before.SequenceEqual(File.ReadAllBytes(Path.Combine(root, "case.json"))));
                var applied = await Command(ops, "apply", root, path, "--approve"); Equal(0, applied.Code);
                True(applied.Output.Contains("Current attention findings: 1", StringComparison.Ordinal)); Equal(1, p.Calls);
                var after = File.ReadAllBytes(Path.Combine(root, "case.json"));
                Equal(2, (await Command(ops, "apply", root, path, "--approve")).Code);
                True(after.SequenceEqual(File.ReadAllBytes(Path.Combine(root, "case.json"))));
                Equal(2, (await Command(ops, "local-plan", root, "unknown-target")).Code);
            }
            finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
        });
        await Test("OFC3 CLI exception and argument paths do not echo private data", async () =>
        {
            var root = Path.Combine(Path.GetTempPath(), "orthonis-live-error-" + Guid.NewGuid().ToString("N"));
            try
            {
                var r = new FakeRegistry { ContextError = new RefusalException(Sentinel) }; var ops = new CaseOperations(() => new(r, new FakeProbe()));
                Equal(2, (await Command(ops, "start-windows", root)).Code); True(!File.Exists(Path.Combine(root, "case.json")));
                r.ContextError = new InvalidOperationException(Sentinel);
                Equal(1, (await Command(ops, "start-windows", root)).Code);
                Equal(2, (await Command(ops, "unknown-" + Sentinel)).Code);
                Equal(2, (await Command(ops, "start", root, "windows")).Code);
            }
            finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
        });
        await Test("OFC3 CLI invalid live batch and unsupported saved version leave bytes unchanged", async () =>
        {
            var root = Path.Combine(Path.GetTempPath(), "orthonis-live-refusal-" + Guid.NewGuid().ToString("N"));
            try
            {
                var r = new FakeRegistry(); var p = new FakeProbe(); var ops = new CaseOperations(() => new(r, p));
                Equal(0, (await Command(ops, "start-windows", root)).Code);
                var file = Path.Combine(root, "case.json"); var before = File.ReadAllBytes(file);
                var plan = JsonCodec.Decode<DiscoveryPlan>(Encoding.UTF8.GetBytes((await Command(ops, "local-plan", root)).Output));
                var invalid = plan with { Requests = [new("startup.inventory", 2, null), new("reliability.summary", 1, null)] };
                var path = Path.Combine(root, "plan.json"); File.WriteAllBytes(path, JsonCodec.Encode(invalid)); var calls = r.Contexts + r.Enumerations;
                Equal(2, (await Command(ops, "apply", root, path, "--approve")).Code); Equal(calls, r.Contexts + r.Enumerations); Equal(0, p.Calls);
                True(before.SequenceEqual(File.ReadAllBytes(file)));
                var unsupported = JsonCodec.Decode<CaseSnapshot>(before) with { SchemaVersion = 99 };
                File.WriteAllBytes(file, JsonCodec.Encode(unsupported)); var unsupportedBytes = File.ReadAllBytes(file);
                Equal(2, (await Command(ops, "summary", root)).Code); True(unsupportedBytes.SequenceEqual(File.ReadAllBytes(file)));
            }
            finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
        });
        await Test("OFC3 unsupported host never falls back to fixture collectors", async () =>
        {
            if (WindowsHost.Available) return; // Windows adapter execution is a separate explicit smoke.
            var root = Path.Combine(Path.GetTempPath(), "orthonis-unsupported-" + Guid.NewGuid().ToString("N"));
            Equal(2, (await Command(new CaseOperations(), "capabilities", "--windows-startup")).Code);
            Equal(2, (await Command(new CaseOperations(), "start-windows", root)).Code);
            True(!Directory.Exists(root));
        });
    }

    // Test-assembly-only driver for fresh-process checks of the actual CLI dispatcher.
    // There is intentionally no corresponding production flag, environment variable or arbitrary-source loader.
    public static async Task<int> LivePhase(string[] args)
    {
        try
        {
            if (args is ["--ofc3-file-probe"]) return NativeProbeSmoke();
            if (args is not ["--ofc3-phase", var phase, var root, var planPath]) return 2;
            var r = new FakeRegistry(); var p = new FakeProbe { Presence = phase == "missing" ? TargetPresence.Missing : TargetPresence.Present };
            var ops = new CaseOperations(() => new(r, p));
            var command = phase switch
            {
                "create" => new[] { "start-windows", root },
                "plan" => ["local-plan", root],
                "preview" => ["apply", root, planPath],
                "missing" or "present" or "refuse" => ["apply", root, planPath, "--approve"],
                "summary-missing" or "summary-present" => ["summary", root],
                _ => throw new RefusalException("Unknown fixture phase.")
            };
            var result = await Command(ops, command);
            Equal(phase == "refuse" ? 2 : 0, result.Code);
            if (phase == "plan") File.WriteAllText(planPath, result.Output, new UTF8Encoding(false));
            if (phase is "plan" or "preview" or "refuse" or "summary-missing" or "summary-present")
            { Equal(0, r.Contexts + r.Enumerations + r.Reads); Equal(0, p.Calls); }
            if (phase is "missing" or "present") { Equal(2, r.Reads); Equal(1, p.Calls); }
            if (phase == "summary-missing") True(result.Output.Contains("Current attention findings: 1", StringComparison.Ordinal));
            if (phase == "summary-present") True(result.Output.Contains("Current attention findings: 0; historical findings: 1", StringComparison.Ordinal));
            Console.WriteLine("PASS controlled OFC3 fresh-process phase.");
            return 0;
        }
        catch (Exception) { Console.Error.WriteLine("FAIL OFC3 test phase. Details withheld to preserve the no-dump boundary."); return 1; }
    }

    private static int NativeProbeSmoke()
    {
        if (!OperatingSystem.IsWindows() || !WindowsHost.ArchitectureSupported)
        { Console.WriteLine("SKIP native Windows file-attribute probe: unsupported host."); return 0; }
        var probe = new LocalExecutableProbe();
        var path = Environment.ProcessPath;
        if (path is null || !RunCommand.LocalExe(path)) throw new RefusalException("No supported native test executable.");
        Equal(TargetPresence.Present, probe.Inspect(path, CancellationToken.None));
        var missing = Path.Combine(Path.GetDirectoryName(path)!, Guid.NewGuid().ToString("N") + ".exe");
        Equal(TargetPresence.Missing, probe.Inspect(missing, CancellationToken.None));
        Equal(TargetPresence.Unsupported, probe.Inspect(@"\\not-contacted.invalid\share\app.exe", CancellationToken.None));
        Equal(TargetPresence.Unsupported, probe.Inspect(@"\\?\C:\app.exe", CancellationToken.None));
        Console.WriteLine("PASS native Windows existing/missing local executable attribute probes; no execution and no network target opened.");
        return 0;
    }
}
