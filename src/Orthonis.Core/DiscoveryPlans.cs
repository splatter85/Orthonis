using System.Collections.Immutable;

namespace Orthonis.Core;

public sealed record DiscoveryPlan(int SchemaVersion, string Kind, string PlanId, string CaseId,
    int BasedOnRevision, string BasedOnSnapshotHash, string Hypothesis, ImmutableArray<string> EvidenceRefs,
    ImmutableArray<DiscoveryRequest> Requests);

public sealed class DiscoveryPlans(Investigation investigation)
{
    public DiscoveryPlan Preview(CaseSnapshot snapshot, byte[] bytes)
    {
        var plan = JsonCodec.Decode<DiscoveryPlan>(bytes, JsonCodec.MaxPlanBytes);
        Contract.Validate(snapshot);
        Contract.Require(plan.SchemaVersion == 1 && plan.Kind == "discovery" && Contract.Id(plan.PlanId),
            "Only version 1 discovery plans are supported; no repair or shell operations.");
        Contract.Require(plan.CaseId == snapshot.CaseId && plan.BasedOnRevision == snapshot.Revision &&
            plan.BasedOnSnapshotHash == JsonCodec.Hash(snapshot), "Plan belongs to another case or stale snapshot.");
        Contract.Require(!snapshot.AppliedPlanIds.Contains(plan.PlanId, StringComparer.Ordinal), "Plan was already applied.");
        Contract.Require(plan.Hypothesis is { Length: > 0 and <= 1000 }, "A bounded hypothesis is required.");
        Contract.Require(!plan.EvidenceRefs.IsDefaultOrEmpty && plan.EvidenceRefs.Length <= 32 &&
            plan.EvidenceRefs.Distinct(StringComparer.Ordinal).Count() == plan.EvidenceRefs.Length &&
            plan.EvidenceRefs.All(id => Contract.Id(id) && snapshot.Evidence.Any(e => e.Id == id)),
            "Plan must cite existing, unique evidence IDs.");
        investigation.ValidateRequests(snapshot, plan.Requests);
        return plan;
    }

    public async Task<CaseSnapshot> ExecuteAsync(CaseSnapshot snapshot, byte[] bytes, bool approved,
        CancellationToken cancellationToken = default)
    {
        var plan = Preview(snapshot, bytes); // Revalidate, even after an earlier preview.
        Contract.Require(approved, "Explicit local approval is required before further collection.");
        Contract.Require(snapshot.Revision < 128 && snapshot.AppliedPlanIds.Length < 128, "Case revision limit reached.");
        var next = await investigation.CollectAsync(snapshot, plan.Requests, cancellationToken).ConfigureAwait(false);
        next = next with { AppliedPlanIds = next.AppliedPlanIds.Add(plan.PlanId) };
        Contract.Validate(next);
        Contract.Require(JsonCodec.Encode(next).Length <= JsonCodec.MaxCaseBytes, "Case storage limit reached.");
        return next;
    }

    // Only supplies an editable example using capabilities/targets actually available in this snapshot.
    // The human/AI may choose other supported requests. This does not diagnose the case or grant approval.
    public DiscoveryPlan Example(CaseSnapshot snapshot)
    {
        Contract.Validate(snapshot);
        var requests = ImmutableArray.CreateBuilder<DiscoveryRequest>();
        foreach (var cap in investigation.Capabilities.Where(c => c.TargetKind is not null))
        {
            var target = snapshot.Evidence.LastOrDefault(e => e.ModuleId == cap.ModuleId &&
                e.TargetKind == cap.TargetKind && e.Status == CollectionStatus.Observed);
            if (target is not null) requests.Add(new(cap.Id, cap.Version, target.TargetId));
        }
        if (requests.Count == 0)
            foreach (var cap in investigation.Capabilities.Where(c => c.TargetKind is null))
                requests.Add(new(cap.Id, cap.Version, null));
        var plan = new DiscoveryPlan(1, "discovery", $"plan-{Guid.NewGuid():N}", snapshot.CaseId, snapshot.Revision,
            JsonCodec.Hash(snapshot), "Collect more evidence before attributing a cause. This is an editable example, not a diagnosis.",
            snapshot.Evidence.Take(8).Select(e => e.Id).ToImmutableArray(), requests.Take(8).ToImmutableArray());
        Preview(snapshot, JsonCodec.Encode(plan));
        return plan;
    }
}
