using Orthonis.Core;

namespace Orthonis.Tests;

public static partial class Program
{
    public static async Task PermissionChecks()
    {
        await Test("OFC3 registry security exception remains permission denied", async () =>
        {
            var r = new FakeRegistry { Listing = _ => throw new System.Security.SecurityException(Sentinel) };
            var p = new FakeProbe();
            var s = await LiveInitial(r, p);
            Equal(2, s.Evidence.Length);
            True(s.Evidence.All(e => e.Status == CollectionStatus.PermissionDenied));
            True(s.Evidence.All(e => e.Coverage!.State == CoverageState.Partial));
            Equal(0, p.Calls);
        });
        await Test("OFC3 source-context security exception remains permission denied", async () =>
        {
            var r = new FakeRegistry(); var p = new FakeProbe(); var s = await LiveInitial(r, p);
            var reads = r.Reads; r.ContextError = new System.Security.SecurityException(Sentinel);
            var next = await LiveEngine(r, p).CollectAsync(s, [new("startup.inspect", 2, Target(s))]);
            Equal(CollectionStatus.PermissionDenied, next.Evidence[^1].Status);
            Equal(reads, r.Reads); Equal(0, p.Calls);
        });
    }
}
