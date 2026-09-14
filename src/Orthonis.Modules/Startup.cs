using System.Collections.Immutable;
using Orthonis.Core;

namespace Orthonis.Modules;

public enum Presence { NotChecked, Present, Missing, Unknown }
public sealed record StartupEntry(string Id, string Name, bool Enabled, Presence Executable);
public interface IStartupSource
{
    Task<ImmutableArray<StartupEntry>> InventoryAsync(CancellationToken cancellationToken);
    Task<StartupEntry> InspectAsync(string targetId, CancellationToken cancellationToken);
}

public sealed class StartupModule(IStartupSource source) : IDiagnosticModule
{
    public string Id => "startup";
    public ImmutableArray<Capability> Capabilities => [
        new("startup.inventory", 1, Id, "List startup items; does not measure performance or change settings.", null),
        new("startup.inspect", 1, Id, "Check one previously observed startup target; missing is not permission to delete.", "startup.item")];

    public async Task<CollectionResult> CollectAsync(DiscoveryRequest request, CancellationToken cancellationToken)
    {
        var entries = request.CapabilityId switch
        {
            "startup.inventory" => await source.InventoryAsync(cancellationToken).ConfigureAwait(false),
            "startup.inspect" when request.TargetId is not null => [await source.InspectAsync(request.TargetId, cancellationToken).ConfigureAwait(false)],
            _ => throw new RefusalException("Unsupported Startup request.")
        };
        Contract.Require(!entries.IsDefault && entries.Length <= 100, "Startup source limit exceeded.");
        var evidence = ImmutableArray.CreateBuilder<Observation>();
        foreach (var entry in entries)
        {
            Contract.Require(entry is not null && Contract.Id(entry.Id) && entry.Name is { Length: > 0 and <= 200 } && Enum.IsDefined(entry.Executable),
                "Invalid Startup source entry.");
            evidence.Add(new($"ev-{Guid.NewGuid():N}", request.CapabilityId, Id, entry!.Id, "startup.item",
                DateTimeOffset.UtcNow, CollectionStatus.Observed, "startup-entry.v1", JsonCodec.Element(entry)));
        }
        // A completed empty enumeration is still an observed result, not an unavailable scan.
        if (evidence.Count == 0)
            evidence.Add(new($"ev-{Guid.NewGuid():N}", request.CapabilityId, Id, Id, "collection", DateTimeOffset.UtcNow,
                CollectionStatus.Observed, "startup-empty.v1", JsonCodec.Element(new { count = 0 })));
        var observed = evidence.ToImmutable();
        return new(observed, StartupAnalysis.Analyze(observed));
    }
}

public static class StartupAnalysis
{
    public static ImmutableArray<Finding> Analyze(ImmutableArray<Observation> observations)
    {
        var findings = ImmutableArray.CreateBuilder<Finding>();
        foreach (var e in observations.Where(e => e.Status == CollectionStatus.Observed && e.DetailSchema == "startup-entry.v1"))
        {
            var entry = JsonCodec.Decode<StartupEntry>(JsonCodec.Encode(e.Detail));
            if (entry.Enabled && entry.Executable == Presence.Missing)
                findings.Add(new($"finding-{Guid.NewGuid():N}", Severity.Attention,
                    $"{entry.Name}: the fixture reports a missing target. Verify installation before considering changes; no performance impact is measured.", [e.Id]));
        }
        return findings.ToImmutable();
    }
}

// Synthetic sources only. No registry, filesystem target checks, or startup changes.
public sealed class FixtureStartupSource(string scenario) : IStartupSource
{
    public Task<ImmutableArray<StartupEntry>> InventoryAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (scenario == "denied") throw new UnauthorizedAccessException();
        return Task.FromResult<ImmutableArray<StartupEntry>>([
            new("startup-001", "Example Sync", true, Presence.NotChecked),
            new("startup-002", "Example Updater", true, Presence.NotChecked)]);
    }

    public Task<StartupEntry> InspectAsync(string targetId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Contract.Require(targetId is "startup-001" or "startup-002", "Unknown fixture target.");
        var presence = scenario == "inconclusive" ? Presence.Unknown :
            scenario == "missing" && targetId == "startup-002" ? Presence.Missing : Presence.Present;
        return Task.FromResult(new StartupEntry(targetId, targetId == "startup-001" ? "Example Sync" : "Example Updater", true, presence));
    }
}
