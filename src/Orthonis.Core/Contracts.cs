using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Orthonis.Core;

public enum CollectionStatus { Observed, Unavailable, PermissionDenied, Failed, TimedOut }
public enum Severity { Information, Attention }

public sealed class RefusalException(string message) : Exception(message);

public sealed record Capability(string Id, int Version, string ModuleId, string Description, string? TargetKind);
public sealed record DiscoveryRequest(string CapabilityId, int CapabilityVersion, string? TargetId);
public sealed record Observation(string Id, string CapabilityId, string ModuleId, string TargetId,
    string TargetKind, DateTimeOffset ObservedAt, CollectionStatus Status, string DetailSchema, JsonElement Detail);
public sealed record Finding(string Id, Severity Severity, string Summary, ImmutableArray<string> EvidenceIds);
public sealed record CollectionResult(ImmutableArray<Observation> Evidence, ImmutableArray<Finding> Findings);
public sealed record CaseSnapshot(int SchemaVersion, string CaseId, int Revision, string SourceLabel,
    DateTimeOffset CreatedAt, ImmutableArray<Observation> Evidence, ImmutableArray<Finding> Findings,
    ImmutableArray<string> AppliedPlanIds)
{
    public static CaseSnapshot Create(string sourceLabel) => new(1, $"case-{Guid.NewGuid():N}", 0,
        sourceLabel, DateTimeOffset.UtcNow, [], [], []);
}

// Modules are reviewed, built-in code, not a security sandbox or dynamic plugins.
public interface IDiagnosticModule
{
    string Id { get; }
    ImmutableArray<Capability> Capabilities { get; }
    Task<CollectionResult> CollectAsync(DiscoveryRequest request, CancellationToken cancellationToken);
}

public static class Contract
{
    public static void Require([DoesNotReturnIf(false)] bool condition, string message)
    {
        if (!condition) throw new RefusalException(message);
    }

    public static bool Id(string? value) => value is not null &&
        Regex.IsMatch(value, @"\A[a-zA-Z0-9][a-zA-Z0-9._-]{0,79}\z", RegexOptions.CultureInvariant);

    public static void Validate(CaseSnapshot value)
    {
        Require(value.SchemaVersion == 1 && Id(value.CaseId) && value.Revision is >= 0 and <= 128,
            "Unsupported case identity, revision, or schema.");
        Require(value.SourceLabel is { Length: > 0 and <= 100 }, "Missing source label.");
        Require(value.CreatedAt != default, "Missing creation time.");
        Require(!value.Evidence.IsDefault && value.Evidence.Length <= 512 &&
            !value.Findings.IsDefault && value.Findings.Length <= 512 &&
            !value.AppliedPlanIds.IsDefault && value.AppliedPlanIds.Length <= 128, "Case limits exceeded.");
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in value.Evidence)
        {
            Require(item is not null, "Null evidence entry.");
            Require(Id(item!.Id) && ids.Add(item.Id) && Id(item.CapabilityId) && Id(item.ModuleId) &&
                Id(item.TargetId) && Id(item.TargetKind), "Invalid or duplicate evidence identity.");
            Require(item.ObservedAt != default && Enum.IsDefined(item.Status) && Id(item.DetailSchema) &&
                item.Detail.ValueKind == JsonValueKind.Object, "Invalid observation.");
        }
        var findings = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in value.Findings)
        {
            Require(item is not null, "Null finding entry.");
            Require(Id(item!.Id) && findings.Add(item.Id) && Enum.IsDefined(item.Severity) &&
                item.Summary is { Length: > 0 and <= 1000 } && !item.EvidenceIds.IsDefaultOrEmpty &&
                item.EvidenceIds.All(id => id is not null && ids.Contains(id)), "Invalid or ungrounded finding.");
        }
        Require(value.AppliedPlanIds.All(Id) && value.AppliedPlanIds.Distinct(StringComparer.Ordinal).Count() ==
            value.AppliedPlanIds.Length, "Invalid or duplicate applied-plan identity.");
    }
}
