using System.Collections.Immutable;
using System.Text;
using Orthonis.Core;
using Orthonis.Modules;

namespace Orthonis.Cli;

public static class Program
{
    public static Investigation CreateEngine(string sourceLabel)
    {
        Contract.Require(sourceLabel is "synthetic:healthy" or "synthetic:missing" or "synthetic:denied" or "synthetic:inconclusive",
            "Only the four built-in synthetic scenarios are supported.");
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
        try
        {
            if (args.Length == 0 || args is ["help"])
            {
                Console.WriteLine("Orthonis foundation: SYNTHETIC evidence only. No PC scan or repairs.\n" +
                    "start <case-directory> [healthy|missing|denied|inconclusive]\nreport <case-directory>\nexample-plan <case-directory>\napply <case-directory> <plan.json> [--approve]\ncapabilities");
                return 0;
            }
            if (args[0] == "start" && args.Length is 2 or 3)
            {
                var snapshot = CaseSnapshot.Create("synthetic:" + (args.Length == 3 ? args[2] : "missing"));
                var engine = CreateEngine(snapshot.SourceLabel);
                using var store = new CaseStore(args[1], create: true);
                var next = await engine.CollectAsync(snapshot, [new("startup.inventory", 1, null), new("reliability.summary", 1, null)], cancellation.Token);
                store.Save(next, expectedRevision: null);
                Console.Write(Report.Render(next, engine.Capabilities, new DiscoveryPlans(engine).Example(next)));
                return 0;
            }
            if (args is ["report", var directory])
            {
                using var store = new CaseStore(directory);
                var snapshot = store.Load();
                var engine = CreateEngine(snapshot.SourceLabel);
                Console.Write(Report.Render(snapshot, engine.Capabilities, new DiscoveryPlans(engine).Example(snapshot)));
                return 0;
            }
            if (args is ["example-plan", var caseDirectory])
            {
                using var store = new CaseStore(caseDirectory);
                var snapshot = store.Load();
                var plans = new DiscoveryPlans(CreateEngine(snapshot.SourceLabel));
                Console.WriteLine(Encoding.UTF8.GetString(JsonCodec.Encode(plans.Example(snapshot))));
                return 0;
            }
            if ((args.Length == 3 || (args.Length == 4 && args[3] == "--approve")) && args[0] == "apply")
            {
                var bytes = CaseStore.ReadBounded(args[2], JsonCodec.MaxPlanBytes);
                using var store = new CaseStore(args[1]); // Lock spans read, revalidation, collection and save.
                var snapshot = store.Load();
                var engine = CreateEngine(snapshot.SourceLabel);
                var plans = new DiscoveryPlans(engine);
                var plan = plans.Preview(snapshot, bytes);
                if (args.Length == 3)
                {
                    Console.WriteLine("PREVIEW ONLY: no collectors run, no case revision changed. Requests:");
                    Console.WriteLine(Encoding.UTF8.GetString(JsonCodec.Encode(plan.Requests)));
                    Console.WriteLine("Review the exact requests, then repeat with --approve to collect fixture evidence.");
                    return 0;
                }
                var next = await plans.ExecuteAsync(snapshot, bytes, approved: true, cancellation.Token);
                store.Save(next, snapshot.Revision);
                Console.Write(Report.Render(next, engine.Capabilities, plans.Example(next)));
                return 0;
            }
            if (args is ["capabilities"])
            {
                Console.WriteLine(System.Text.Encoding.UTF8.GetString(JsonCodec.Encode(CreateEngine("synthetic:healthy").Capabilities)));
                return 0;
            }
            throw new RefusalException("Unknown command or arguments. Run help.");
        }
        catch (RefusalException e) { Console.Error.WriteLine($"REFUSED: {e.Message}"); return 2; }
        catch (OperationCanceledException) { Console.Error.WriteLine("Cancelled. No new case revision committed."); return 3; }
        catch (IOException) { Console.Error.WriteLine("Storage operation failed or case is busy. Inspect the current case before retrying."); return 4; }
        catch (UnauthorizedAccessException) { Console.Error.WriteLine("Case storage permission denied."); return 4; }
        finally { Console.CancelKeyPress -= onCancel; }
    }
}
