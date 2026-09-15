namespace Orthonis.Tests;

public static class Ofc3Runner
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length > 0 && args[0].StartsWith("--ofc4", StringComparison.Ordinal))
            return await ReliabilityChecks.Phase(args).ConfigureAwait(false);
        if (args.Length > 0) return await Program.LivePhase(args).ConfigureAwait(false);
        var baseline = await Program.Main().ConfigureAwait(false); // All 53 existing assertions stay intact.
        await Program.PermissionChecks().ConfigureAwait(false);
        var extended = await Program.LiveChecks().ConfigureAwait(false);
        await ReliabilityChecks.Run().ConfigureAwait(false);
        return baseline == 0 && extended == 0 ? 0 : 1;
    }
}
