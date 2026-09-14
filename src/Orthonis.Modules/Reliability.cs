using System.Collections.Immutable;
using Orthonis.Core;

namespace Orthonis.Modules;

public sealed record ReliabilityEntry(string Id, string Application, int HangCount, int WindowDays, string? ReportedModule);
public interface IReliabilitySource
{
    Task<ImmutableArray<ReliabilityEntry>> SummaryAsync(CancellationToken cancellationToken);
    Task<ReliabilityEntry> InspectAsync(string targetId, CancellationToken cancellationToken);
}

public sealed class ReliabilityModule(IReliabilitySource source) : IDiagnosticModule
{
    public string Id => "reliability";
    public ImmutableArray<Capability> Capabilities => [
        new("reliability.summary", 1, Id, "Summarize available application hang records; no causal diagnosis.", null),
        new("reliability.inspect", 1, Id, "Inspect details of one previously observed application group.", "reliability.application")];

    public async Task<CollectionResult> CollectAsync(DiscoveryRequest request, CancellationToken cancellationToken)
    {
        var entries = request.CapabilityId switch
        {
            "reliability.summary" => await source.SummaryAsync(cancellationToken).ConfigureAwait(false),
            "reliability.inspect" when request.TargetId is not null => [await source.InspectAsync(request.TargetId, cancellationToken).ConfigureAwait(false)],
            _ => throw new RefusalException("Unsupported Reliability request.")
        };
        Contract.Require(!entries.IsDefault && entries.Length <= 100, "Reliability source limit exceeded.");
        var evidence = ImmutableArray.CreateBuilder<Observation>();
        foreach (var entry in entries)
        {
            Contract.Require(entry is not null && Contract.Id(entry.Id) && entry.Application is { Length: > 0 and <= 200 } &&
                entry.HangCount is >= 0 and <= 100000 && entry.WindowDays is >= 1 and <= 365 &&
                (entry.ReportedModule is null || entry.ReportedModule.Length <= 200), "Invalid Reliability source entry.");
            evidence.Add(new($"ev-{Guid.NewGuid():N}", request.CapabilityId, Id, entry.Id, "reliability.application",
                DateTimeOffset.UtcNow, CollectionStatus.Observed, "reliability-entry.v1", JsonCodec.Element(entry)));
        }
        if (evidence.Count == 0)
            evidence.Add(new($"ev-{Guid.NewGuid():N}", request.CapabilityId, Id, Id, "collection", DateTimeOffset.UtcNow,
                CollectionStatus.Observed, "reliability-empty.v1", JsonCodec.Element(new { count = 0 })));
        var observed = evidence.ToImmutable();
        return new(observed, ReliabilityAnalysis.Analyze(observed));
    }
}

public static class ReliabilityAnalysis
{
    public static ImmutableArray<Finding> Analyze(ImmutableArray<Observation> observations)
    {
        var findings = ImmutableArray.CreateBuilder<Finding>();
        foreach (var e in observations.Where(e => e.Status == CollectionStatus.Observed && e.DetailSchema == "reliability-entry.v1"))
        {
            var entry = JsonCodec.Decode<ReliabilityEntry>(JsonCodec.Encode(e.Detail));
            if (entry.HangCount >= 2)
                findings.Add(new($"finding-{Guid.NewGuid():N}", Severity.Attention,
                    $"{entry.Application}: {entry.HangCount} synthetic hang records in {entry.WindowDays} days. Repeated observations are not a proven cause or separate new crashes.", [e.Id]));
        }
        return findings.ToImmutable();
    }
}

public sealed class FixtureReliabilitySource(string scenario) : IReliabilitySource
{
    public Task<ImmutableArray<ReliabilityEntry>> SummaryAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (scenario == "denied") throw new UnauthorizedAccessException();
        return Task.FromResult<ImmutableArray<ReliabilityEntry>>([Entry(null)]);
    }
    public Task<ReliabilityEntry> InspectAsync(string targetId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Contract.Require(targetId == "application-001", "Unknown reliability fixture target.");
        return Task.FromResult(Entry(scenario == "missing" ? "example-extension.dll" : null));
    }
    private ReliabilityEntry Entry(string? detail) => new("application-001", "Example Explorer", scenario == "missing" ? 3 : 0, 7, detail);
}
