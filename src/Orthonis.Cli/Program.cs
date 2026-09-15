using System.Text;
using Orthonis.Core;
using Orthonis.Modules;

namespace Orthonis.Cli;

public static class Program
{
    public static Investigation CreateEngine(string sourceLabel)
    {
        Contract.Require(SourceData.FixtureLabel(sourceLabel), "Only the four built-in synthetic scenarios are supported by the fixture engine.");
        return new Investigation([
            new StartupModule(new FixtureStartupSource(sourceLabel[10..])),
            new ReliabilityModule(new FixtureReliabilitySource(sourceLabel[10..]))]);
    }
    public static async Task<int> Main(string[] args)
    {
        using var cancellation = new CancellationTokenSource();
        Console.OutputEncoding = new UTF8Encoding(false);
        ConsoleCancelEventHandler onCancel = (_, e) => { e.Cancel = true; cancellation.Cancel(); };
        Console.CancelKeyPress += onCancel;
        try { return await RunAsync(args, Console.Out, Console.Error, new CaseOperations(), cancellation.Token).ConfigureAwait(false); }
        finally { Console.CancelKeyPress -= onCancel; }
    }
    // Real command dispatcher, also exercised with controlled sources by the separate test executable.
    public static async Task<int> RunAsync(string[] args, TextWriter output, TextWriter error, CaseOperations operations,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (args.Length == 0 || args is ["help"])
            {
                output.WriteLine("Orthonis CLI: synthetic demo by default; live Windows sources are explicit opt-in. No repairs or target execution.\n" +
                    "start <case-directory> [healthy|missing|denied|inconclusive]\n" +
                    "start-windows <case-directory>\nstart-reliability <case-directory>\nsummary <case-directory>\n" +
                    "report <case-directory>                 (synthetic export only)\n" +
                    "example-plan <case-directory>           (synthetic only)\n" +
                    "local-plan <case-directory> [target-id]  (local/private Windows plan; Reliability takes no target)\n" +
                    "apply <case-directory> <plan.json> [--approve]\ncapabilities [--windows-startup|--windows-reliability]\n" +
                    "Startup: HKCU/HKLM Run in the native registry view, at most 32 entries per key. Other startup surfaces are excluded.\n" +
                    "Reliability: Application levels 1/2/3 plus Windows Error Reporting, preceding seven days, newest 64 records; metadata only, not incident counts.\n" +
                    "Live case files are private. Live report/example-plan export is blocked. No fallback on unsupported hosts.\n" +
                    "Exit: 0 completed/preview (not health); 1 internal failure; 2 refused/unsupported/export blocked; 3 cancelled; 4 storage unavailable.");
                return 0;
            }
            if (args[0] == "start" && args.Length is 2 or 3)
            {
                output.Write(await operations.Start(args[1], args.Length == 3 ? args[2] : "missing", cancellationToken).ConfigureAwait(false));
                return 0;
            }
            if (args is ["start-windows", var liveDirectory])
            {
                error.WriteLine("Reading the bounded Windows Run subset. Evidence stays in the local case; Ctrl+C cancels before saving.");
                output.Write(await operations.StartWindows(liveDirectory, cancellationToken).ConfigureAwait(false));
                return 0;
            }
            if (args is ["start-reliability", var reliabilityDirectory])
            {
                error.WriteLine("Reading bounded local Application event metadata. No messages/dumps are exported; Ctrl+C cancels before saving.");
                output.Write(await operations.StartReliability(reliabilityDirectory, cancellationToken).ConfigureAwait(false));
                return 0;
            }
            if (args is ["summary", var summaryDirectory]) { output.Write(operations.Summary(summaryDirectory)); return 0; }
            if (args is ["report", var directory]) { output.Write(operations.Export(directory)); return 0; }
            if (args is ["example-plan", var caseDirectory]) { output.WriteLine(operations.Example(caseDirectory)); return 0; }
            if (args[0] == "local-plan" && args.Length is 2 or 3)
            {
                var plan = operations.LocalPlan(args[1], args.Length == 3 ? args[2] : null);
                error.WriteLine("LOCAL / PRIVATE plan. Not an AI export or a sanitized case. Save UTF-8 JSON locally, preview, then explicitly approve.");
                output.WriteLine(plan);
                return 0;
            }
            if ((args.Length == 3 || (args.Length == 4 && args[3] == "--approve")) && args[0] == "apply")
            {
                output.Write(await operations.Apply(args[1], args[2], args.Length == 4, cancellationToken).ConfigureAwait(false));
                return 0;
            }
            if (args is ["capabilities"])
            {
                output.WriteLine(Encoding.UTF8.GetString(JsonCodec.Encode(CreateEngine("synthetic:healthy").Capabilities)));
                return 0;
            }
            if (args is ["capabilities", "--windows-startup"])
            {
                output.WriteLine(Encoding.UTF8.GetString(JsonCodec.Encode(operations.WindowsCapabilities())));
                return 0;
            }
            if (args is ["capabilities", "--windows-reliability"])
            {
                output.WriteLine(Encoding.UTF8.GetString(JsonCodec.Encode(operations.ReliabilityCapabilities())));
                return 0;
            }
            throw new RefusalException("Unknown command or arguments.");
        }
        // Never echo exception text: source adapters, paths and imported data may contain private values.
        catch (RefusalException)
        {
            error.WriteLine("REFUSED: invalid input, unsupported host/source/version, stale plan, or blocked live export. Use help. Live cases use summary/local-plan, not report/example-plan. No implicit format migration.");
            return 2;
        }
        catch (OperationCanceledException) { error.WriteLine("Cancelled. No new case revision committed."); return 3; }
        catch (IOException) { error.WriteLine("Storage operation failed or case is busy. Reload the local case before retrying."); return 4; }
        catch (UnauthorizedAccessException) { error.WriteLine("Local storage or source permission denied. No elevation was attempted."); return 4; }
        catch (System.Security.SecurityException) { error.WriteLine("Local access denied. No elevation was attempted."); return 4; }
        catch (Exception e) when (e is ArgumentException or NotSupportedException)
        { error.WriteLine("REFUSED: unsupported input, path or platform."); return 2; }
        catch (Exception) { error.WriteLine("Internal operation failed. Reload the local case before retrying; no diagnostic details were exported."); return 1; }
    }
}
