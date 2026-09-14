namespace Orthonis.Tests;

public static class Ofc3Runner
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length > 0) return await Program.LivePhase(args).ConfigureAwait(false);
        var baseline = await Program.Main().ConfigureAwait(false); // All 53 existing assertions stay intact.
        await Program.PermissionChecks().ConfigureAwait(false);
        var extended = await Program.LiveChecks().ConfigureAwait(false);
        return baseline == 0 && extended == 0 ? 0 : 1;
    }
}
