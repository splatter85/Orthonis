using System.Collections.Immutable;
using Orthonis.Core;

namespace Orthonis.Windows;

public sealed class WindowsReliabilityModule(IApplicationEventLog source) : ICaseDiagnosticModule
{
    public string Id => "reliability";
    public ImmutableArray<Capability> Capabilities => [new("reliability.summary", 2, Id,
        "Local/private bounded seven-day Application event metadata refresh. No payload diagnosis, export or repairs.", null)];
    public void ValidateCase(CaseSnapshot snapshot) => ReliabilityData.ValidateCase(snapshot);
    public Task<CollectionResult> CollectAsync(DiscoveryRequest request, CancellationToken cancellationToken) =>
        throw new RefusalException("Persisted live case context is required.");
    public Task<CollectionResult> CollectAsync(CaseSnapshot snapshot, DiscoveryRequest request, CancellationToken cancellationToken)
    {
        ValidateCase(snapshot);
        Contract.Require(request == new DiscoveryRequest("reliability.summary", 2, null), "Unsupported Reliability request.");
        // Entire native query/read/close sequence stays on one worker thread; no native handles cross awaits.
        return Task.Run(() => Collect(snapshot, cancellationToken), cancellationToken);
    }
    private CollectionResult Collect(CaseSnapshot snapshot, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var to = DateTimeOffset.UtcNow;
        var from = to.AddDays(-7);
        ApplicationRead read;
        var sourceChanged = source.Context(snapshot.CaseId) != snapshot.Source;
        if (sourceChanged) read = new(CollectionStatus.Stale, 0, 0, [], null, null);
        else
        {
            read = source.Read(from, to, token);
            token.ThrowIfCancellationRequested();
            sourceChanged = source.Context(snapshot.CaseId) != snapshot.Source;
            if (sourceChanged) read = new(CollectionStatus.Stale, 0, 0, [], null, null);
        }
        ReliabilityData.ValidateRead(read, from, to);
        read = read with { Records = read.Records.GroupBy(e => ReliabilityData.Identity(snapshot, e), StringComparer.Ordinal)
            .Select(g => g.First()).ToImmutableArray() };
        var last = snapshot.Evidence.LastOrDefault(e => e.DetailSchema == ReliabilityData.DetailSchema);
        var prior = last is null ? null : ReliabilityData.Query(last);
        var gaps = ReliabilityData.Gaps(read, from, prior);
        if (sourceChanged) gaps |= ReliabilityGap.SourceChanged;
        var history = snapshot.Evidence.Where(e => e.DetailSchema == ReliabilityData.DetailSchema)
            .SelectMany(e => ReliabilityData.Query(e).Read.Records).Concat(read.Records);
        if (history.GroupBy(e => e.RecordId).Any(g => g.Select(e => ReliabilityData.Identity(snapshot, e)).Distinct(StringComparer.Ordinal).Skip(1).Any()))
            gaps |= ReliabilityGap.RecordIdReused;
        var complete = gaps == ReliabilityGap.None && read.Status is CollectionStatus.Observed or CollectionStatus.Empty;
        var coverage = new QueryCoverage($"query-{Guid.NewGuid():N}", SourceScope.ApplicationEventLog, QueryPortion.ApplicationEvents,
            complete ? CoverageState.Complete : CoverageState.Partial, read.Examined, read.Records.Length, ReliabilityData.Limit);
        token.ThrowIfCancellationRequested();
        var evidence = new Observation($"ev-{Guid.NewGuid():N}", "reliability.summary", Id, Id, "collection", DateTimeOffset.UtcNow,
            read.Status, ReliabilityData.DetailSchema, JsonCodec.Element(new ReliabilityQuery(from, to, read, gaps)), coverage);
        return new([evidence], []);
    }
}
