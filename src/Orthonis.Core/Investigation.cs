using System.Collections.Immutable;

namespace Orthonis.Core;

public sealed class Investigation
{
    private readonly Dictionary<string, (Capability Capability, IDiagnosticModule Module)> routes = new(StringComparer.Ordinal);
    public ImmutableArray<Capability> Capabilities => routes.Values.Select(v => v.Capability).OrderBy(v => v.Id, StringComparer.Ordinal).ToImmutableArray();

    public Investigation(IEnumerable<IDiagnosticModule> modules)
    {
        var moduleIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var module in modules)
        {
            Contract.Require(Contract.Id(module.Id) && moduleIds.Add(module.Id) && !module.Capabilities.IsDefaultOrEmpty,
                "Invalid or duplicate module.");
            foreach (var cap in module.Capabilities)
            {
                Contract.Require(cap is not null && Contract.Id(cap.Id) && cap.ModuleId == module.Id && cap.Version == 1 &&
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
            budget.CancelAfter(TimeSpan.FromSeconds(5));
            CollectionResult result;
            try
            {
                result = await route.Module.CollectAsync(request, budget.Token).WaitAsync(budget.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            { result = Failure(route.Capability, request, CollectionStatus.TimedOut); }
            catch (UnauthorizedAccessException)
            { result = Failure(route.Capability, request, CollectionStatus.PermissionDenied); }
            catch (Exception ex) when (ex is not OperationCanceledException and not RefusalException)
            { result = Failure(route.Capability, request, CollectionStatus.Failed); }
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
        Contract.Require(JsonCodec.Encode(next).Length <= JsonCodec.MaxCaseBytes, "Case storage limit reached.");
        return next;
    }

    private static CollectionResult Failure(Capability capability, DiscoveryRequest request, CollectionStatus status)
    {
        var e = new Observation($"ev-{Guid.NewGuid():N}", capability.Id, capability.ModuleId,
            request.TargetId ?? capability.ModuleId, capability.TargetKind ?? "collection", DateTimeOffset.UtcNow,
            status, "collection-outcome.v1", JsonCodec.Element(new { reason = "Collector did not complete; not a healthy result." }));
        return new([e], []);
    }
}
