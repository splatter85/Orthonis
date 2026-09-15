using System.Collections.Immutable;

namespace Orthonis.Core;

public sealed class Investigation
{
    private readonly Dictionary<string, (Capability Capability, IDiagnosticModule Module)> routes = new(StringComparer.Ordinal);
    private readonly SourceMode mode;
    private readonly TimeSpan timeout;
    public ImmutableArray<Capability> Capabilities => routes.Values.Select(v => v.Capability).OrderBy(v => v.Id, StringComparer.Ordinal).ToImmutableArray();

    public Investigation(IEnumerable<IDiagnosticModule> modules, SourceMode mode = SourceMode.Synthetic, TimeSpan? timeout = null)
    {
        Contract.Require(Enum.IsDefined(mode), "Unsupported source mode.");
        this.mode = mode;
        this.timeout = timeout ?? TimeSpan.FromSeconds(5);
        Contract.Require(this.timeout > TimeSpan.Zero && this.timeout <= TimeSpan.FromSeconds(5), "Invalid collection budget.");
        var moduleIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var module in modules)
        {
            Contract.Require(Contract.Id(module.Id) && moduleIds.Add(module.Id) && !module.Capabilities.IsDefaultOrEmpty,
                "Invalid or duplicate module.");
            foreach (var cap in module.Capabilities)
            {
                Contract.Require(cap is not null && Contract.Id(cap.Id) && cap.ModuleId == module.Id && cap.Version is 1 or 2 &&
                    cap.Description is { Length: > 0 and <= 500 } && (cap.TargetKind is null || Contract.Id(cap.TargetKind)),
                    "Invalid capability contract.");
                Contract.Require(routes.TryAdd(cap!.Id, (cap, module)), "Duplicate capability.");
            }
        }
        Contract.Require(routes.Count is > 0 and <= 32, "Invalid capability count.");
    }

    public void ValidateRequests(CaseSnapshot snapshot, ImmutableArray<DiscoveryRequest> requests)
    {
        Contract.Validate(snapshot);
        Contract.Require(SourceData.Mode(snapshot) == mode, "Case and collector source modes differ.");
        foreach (var module in routes.Values.Select(r => r.Module).OfType<ICaseDiagnosticModule>().Distinct())
            module.ValidateCase(snapshot);
        Contract.Require(!requests.IsDefaultOrEmpty && requests.Length <= 8, "A plan needs 1 to 8 requests.");
        var unique = new HashSet<DiscoveryRequest>();
        foreach (var request in requests)
        {
            Contract.Require(request is not null && Contract.Id(request.CapabilityId) && unique.Add(request), "Null or duplicate request.");
            Contract.Require(routes.TryGetValue(request!.CapabilityId, out var route), "Unsupported capability.");
            Contract.Require(request.CapabilityVersion == route.Capability.Version, "Unsupported capability version.");
            if (route.Capability.TargetKind is null)
                Contract.Require(request.TargetId is null, "This capability does not accept a target.");
            else
                Contract.Require(Contract.Id(request.TargetId) && snapshot.Evidence.Any(e =>
                    e.ModuleId == route.Module.Id && e.TargetId == request.TargetId &&
                    e.TargetKind == route.Capability.TargetKind && e.Status == CollectionStatus.Observed),
                    "Unknown, unavailable, or wrong-module target.");
        }
    }

    public async Task<CaseSnapshot> CollectAsync(CaseSnapshot snapshot, ImmutableArray<DiscoveryRequest> requests,
        CancellationToken cancellationToken = default)
    {
        ValidateRequests(snapshot, requests); // Whole batch before any collector call.
        var observations = snapshot.Evidence.ToBuilder();
        var findings = snapshot.Findings.ToBuilder();
        foreach (var request in requests)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var route = routes[request.CapabilityId];
            using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            budget.CancelAfter(timeout);
            CollectionResult result;
            try
            {
                var collection = route.Module is ICaseDiagnosticModule scoped
                    ? scoped.CollectAsync(snapshot, request, budget.Token)
                    : route.Module.CollectAsync(request, budget.Token);
                result = await collection.WaitAsync(budget.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            { result = Failure(snapshot, route.Capability, request, CollectionStatus.TimedOut); }
            catch (UnauthorizedAccessException)
            { result = Failure(snapshot, route.Capability, request, CollectionStatus.PermissionDenied); }
            catch (System.Security.SecurityException)
            { result = Failure(snapshot, route.Capability, request, CollectionStatus.PermissionDenied); }
            catch (Exception ex) when (ex is not OperationCanceledException and not RefusalException)
            { result = Failure(snapshot, route.Capability, request, CollectionStatus.Failed); }
            Contract.Require(!result.Evidence.IsDefaultOrEmpty && !result.Findings.IsDefault &&
                result.Evidence.All(e => e is not null && e.CapabilityId == request.CapabilityId && e.ModuleId == route.Module.Id &&
                    (request.TargetId is null || request.TargetId == e.TargetId)), "Collector returned mismatched evidence.");
            var fresh = result.Evidence.Select(e => e.Id).ToHashSet(StringComparer.Ordinal);
            Contract.Require(result.Findings.All(f => f is not null && !f.EvidenceIds.IsDefaultOrEmpty &&
                f.EvidenceIds.All(fresh.Contains)), "Collector returned ungrounded findings.");
            observations.AddRange(result.Evidence);
            findings.AddRange(result.Findings);
        }
        cancellationToken.ThrowIfCancellationRequested();
        var next = snapshot with { Revision = snapshot.Revision + 1, Evidence = observations.ToImmutable(), Findings = findings.ToImmutable() };
        Contract.Validate(next);
        foreach (var module in routes.Values.Select(r => r.Module).OfType<ICaseDiagnosticModule>().Distinct())
            module.ValidateCase(next);
        Contract.Require(JsonCodec.Encode(next).Length <= JsonCodec.MaxCaseBytes, "Case storage limit reached.");
        return next;
    }

    private static CollectionResult Failure(CaseSnapshot snapshot, Capability capability, DiscoveryRequest request, CollectionStatus status)
    {
        QueryCoverage? coverage = snapshot.Source is { } source ? new($"query-{Guid.NewGuid():N}", source.Scope,
            QueryPortion.Collection, CoverageState.NotQueried, 0, 0, 1) : null;
        var e = new Observation($"ev-{Guid.NewGuid():N}", capability.Id, capability.ModuleId,
            request.TargetId ?? capability.ModuleId, capability.TargetKind ?? "collection", DateTimeOffset.UtcNow,
            status, "collection-outcome.v1", JsonCodec.Element(new { reason = "Collector did not complete; not a healthy result." }), coverage);
        return new([e], []);
    }
}
