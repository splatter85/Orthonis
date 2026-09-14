using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Orthonis.Core;

namespace Orthonis.Windows;

public enum RunHive { CurrentUser, LocalMachine }
public enum Enablement { Unknown }
public enum TargetPresence { NotChecked, Present, Missing, Inaccessible, Unknown, Unsupported, Stale }
public sealed record RunValue(string Name, int Kind, ImmutableArray<byte> Data);
public sealed record RunListing(CollectionStatus Status, ImmutableArray<RunValue> Values, int Examined);
public sealed record RunRead(CollectionStatus Status, RunValue? Value);
public sealed record RunLocator(RunHive Hive, SourceScope View, string ValueName, string RegistrationHash);
public sealed record RunEntry(RunLocator Locator, Enablement Enabled, TargetPresence Presence);
public sealed record RunCoverageResult(int Count);

// No arbitrary subkey, path or command can be supplied through a discovery request.
public interface IRunRegistry
{
    SourceScope Scope { get; }
    SourceContext Context(string caseId);
    RunListing Enumerate(RunHive hive, CancellationToken cancellationToken);
    RunRead Read(RunHive hive, string valueName, CancellationToken cancellationToken);
}
public interface IExecutableProbe
{
    TargetPresence Inspect(string executablePath, CancellationToken cancellationToken);
}

public static class RunIdentity
{
    public const int PerKeyLimit = 32;
    public const int MaxValueBytes = 2048;
    public const string Key = @"Software\Microsoft\Windows\CurrentVersion\Run";
    public const string TargetKind = "startup.run-registration";
    public static string Hash<T>(T value) => Convert.ToHexString(SHA256.HashData(JsonCodec.Encode(value))).ToLowerInvariant();
    public static SourceContext Context(string caseId, SourceScope scope, string machine, string user) =>
        new(SourceMode.WindowsStartup, scope, Hash(new[] { caseId, "machine", machine }), Hash(new[] { caseId, "user", user }));
    public static bool Name(string? value)
    {
        if (value is null || value.Length > 256 || value.Any(char.IsControl)) return false;
        try { _ = new UnicodeEncoding(false, false, true).GetByteCount(value); return true; }
        catch (EncoderFallbackException) { return false; }
    }
    public static void ValidateValue(RunValue value) => Contract.Require(value is not null && Name(value.Name) &&
        !value.Data.IsDefault && value.Data.Length <= MaxValueBytes && value.Kind >= 0, "Invalid or excessive registry value.");
    public static string Registration(RunValue value)
    {
        ValidateValue(value);
        return Hash(new { value.Kind, Data = Convert.ToHexString(value.Data.AsSpan()) });
    }
    public static string Target(CaseSnapshot snapshot, RunLocator locator) => "run-" + Hash(new
    {
        snapshot.CaseId, snapshot.Source, locator.Hive, locator.View,
        Name = locator.ValueName.ToUpperInvariant(), locator.RegistrationHash
    });
}

public static class RunCase
{
    public static RunEntry Entry(Observation observation) => JsonCodec.Decode<RunEntry>(JsonCodec.Encode(observation.Detail));

    public static void Validate(CaseSnapshot snapshot)
    {
        Contract.Validate(snapshot);
        Contract.Require(snapshot.SchemaVersion == 2 && snapshot.Source is not null, "A version 2 Windows Startup case is required.");
        var targets = new HashSet<string>(StringComparer.Ordinal);
        foreach (var e in snapshot.Evidence)
        {
            Contract.Require(SourceData.Opaque(e.Id, "ev-") && e.ModuleId == "startup" &&
                e.CapabilityId is "startup.inventory" or "startup.inspect", "Unsupported live evidence source.");
            if (e.DetailSchema == "startup-run.v2")
            {
                var entry = Entry(e);
                var l = entry.Locator;
                Contract.Require(l is not null && Enum.IsDefined(l.Hive) && l.View == snapshot.Source.Scope &&
                    RunIdentity.Name(l.ValueName) && SourceData.Digest(l.RegistrationHash) &&
                    e.TargetId == RunIdentity.Target(snapshot, l) && e.TargetKind == RunIdentity.TargetKind &&
                    entry.Enabled == Enablement.Unknown && Enum.IsDefined(entry.Presence), "Invalid persisted local target mapping.");
                Contract.Require(e.CapabilityId == "startup.inventory"
                    ? e.Status == CollectionStatus.Observed && entry.Presence == TargetPresence.NotChecked &&
                        e.Coverage!.Portion == (l.Hive == RunHive.CurrentUser ? QueryPortion.CurrentUserRun : QueryPortion.LocalMachineRun)
                    : targets.Contains(e.TargetId) && entry.Presence != TargetPresence.NotChecked &&
                        e.Status == WindowsStartupModule.Status(entry.Presence) && e.Coverage!.Portion == QueryPortion.Target,
                    "Contradictory Startup evidence outcome.");
                targets.Add(e.TargetId);
            }
            else if (e.DetailSchema == "startup-coverage.v2")
            {
                var detail = JsonCodec.Decode<RunCoverageResult>(JsonCodec.Encode(e.Detail));
                Contract.Require(e.CapabilityId == "startup.inventory" && e.TargetKind == "collection" && e.TargetId == "startup" &&
                    detail.Count == e.Coverage!.Returned && e.Coverage.Portion is QueryPortion.CurrentUserRun or QueryPortion.LocalMachineRun &&
                    e.Status is CollectionStatus.Observed or CollectionStatus.Empty or CollectionStatus.Limited or CollectionStatus.PermissionDenied or
                        CollectionStatus.Failed or CollectionStatus.Unavailable or CollectionStatus.Unsupported,
                    "Invalid collection coverage record.");
                Contract.Require(e.Status is CollectionStatus.Observed or CollectionStatus.Empty
                    ? e.Coverage.State == CoverageState.Complete : e.Coverage.State == CoverageState.Partial,
                    "Contradictory collection coverage.");
            }
            else
            {
                Contract.Require(e.DetailSchema == "collection-outcome.v1" && e.Status is CollectionStatus.Unavailable or
                    CollectionStatus.PermissionDenied or CollectionStatus.Failed or CollectionStatus.TimedOut or CollectionStatus.Unsupported,
                    "Unsupported live detail format.");
                Contract.Require(e.Coverage!.State == CoverageState.NotQueried && e.Coverage.Examined == 0 && e.Coverage.Returned == 0 &&
                    e.Coverage.Portion == QueryPortion.Collection && (e.CapabilityId == "startup.inspect"
                        ? e.TargetKind == RunIdentity.TargetKind && targets.Contains(e.TargetId)
                        : e.TargetKind == "collection" && e.TargetId == "startup"), "Unmapped target or invalid failure coverage.");
            }
        }
        Contract.Require(snapshot.Findings.All(f => SourceData.Opaque(f.Id, "finding-")), "Invalid live finding identity.");
    }

    public static ImmutableArray<Observation> CurrentTargets(CaseSnapshot snapshot)
    {
        Validate(snapshot);
        var lastInventory = -1;
        for (var i = 0; i < snapshot.Evidence.Length; i++)
            if (snapshot.Evidence[i].CapabilityId == "startup.inventory") lastInventory = i;
        var query = lastInventory < 0 ? null : snapshot.Evidence[lastInventory].Coverage!.QueryId;
        // A refresh makes prior results historical, including when the refresh is incomplete.
        // A later inspection can explicitly revisit a historical target without remapping it.
        return snapshot.Evidence.Where((e, i) => e.TargetKind == RunIdentity.TargetKind &&
                (e.Coverage!.QueryId == query || i > lastInventory))
            .GroupBy(e => e.TargetId, StringComparer.Ordinal).Select(g => g.Last()).ToImmutableArray();
    }

    public static string Summary(CaseSnapshot snapshot)
    {
        Validate(snapshot);
        var text = new StringBuilder("LOCAL / PRIVATE Windows Startup summary. Not an AI export or a sanitized case file.\n");
        text.AppendLine($"Case: {snapshot.CaseId}; revision: {snapshot.Revision}; scope: {snapshot.Source!.Scope}.");
        text.AppendLine("Coverage: HKCU and HKLM Run, one native registry view only; at most 32 values per key.");
        text.AppendLine("Excluded: alternate view, RunOnce, Startup folders, scheduled tasks, services, drivers and shell extensions.");
        text.AppendLine("Enablement is unknown. Registration is not proof of execution, boot delay, malware, health or a required repair.");
        var lastInventory = snapshot.Evidence.LastOrDefault(e => e.CapabilityId == "startup.inventory");
        if (lastInventory is not null)
            foreach (var e in snapshot.Evidence.Where(e => e.Coverage!.QueryId == lastInventory.Coverage!.QueryId && e.TargetKind == "collection"))
                text.AppendLine($"Query: {e.Coverage!.Portion}; {e.Status}; {e.Coverage.State}; examined {e.Coverage.Examined}; retained {e.Coverage.Returned}; observed {e.ObservedAt:O}.");
        var current = CurrentTargets(snapshot);
        foreach (var e in current.Take(64))
        {
            var presence = e.DetailSchema == "startup-run.v2" ? Entry(e).Presence : TargetPresence.Unknown;
            text.AppendLine($"Target {e.TargetId}: {e.Status}; primary executable {presence}; enabled Unknown; observed {e.ObservedAt:O}.");
        }
        var missingIds = current.Where(e => e.Status == CollectionStatus.Observed && e.DetailSchema == "startup-run.v2" &&
            Entry(e).Presence == TargetPresence.Missing).Select(e => e.Id).ToHashSet(StringComparer.Ordinal);
        var active = snapshot.Findings.Count(f => f.Severity == Severity.Attention && f.EvidenceIds.All(missingIds.Contains));
        text.AppendLine($"Current attention findings: {active}; historical findings: {snapshot.Findings.Length - active}; retained observations: {snapshot.Evidence.Length}.");
        text.AppendLine($"Current target rows: {Math.Min(64, current.Length)} of {current.Length}. Missing means an observed absence, not permission to repair.");
        text.AppendLine("Old findings remain history, not current diagnoses. Incomplete or empty queries do not establish a healthy PC. Live AI/export is blocked.");
        // Never render source bindings, locators, value names, raw details or untrusted finding summaries here.
        return text.ToString();
    }
}
