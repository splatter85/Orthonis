using System.Text.RegularExpressions;

namespace Orthonis.Core;

public enum SourceMode { Synthetic, WindowsStartup, WindowsReliability }
public enum SourceScope { RunNative32, RunNative64, ApplicationEventLog }
public enum QueryPortion { CurrentUserRun, LocalMachineRun, Target, Collection, ApplicationEvents }
public enum CoverageState { Complete, Partial, NotQueried }

// Case-salted bindings are provenance/preconditions, not authentication or an export guarantee.
public sealed record SourceContext(SourceMode Mode, SourceScope Scope, string MachineBinding, string UserBinding);
public sealed record QueryCoverage(string QueryId, SourceScope Scope, QueryPortion Portion,
    CoverageState State, int Examined, int Returned, int Limit);

public static class SourceData
{
    public static bool Digest(string? value) => value is not null &&
        Regex.IsMatch(value, @"\A[0-9a-f]{64}\z", RegexOptions.CultureInvariant);
    public static bool Opaque(string? value, string prefix) => value is not null && value.StartsWith(prefix, StringComparison.Ordinal) &&
        Regex.IsMatch(value[prefix.Length..], @"\A[0-9a-f]{32}\z", RegexOptions.CultureInvariant);
    public static bool FixtureLabel(string value) => value is "synthetic:healthy" or "synthetic:missing" or "synthetic:denied" or "synthetic:inconclusive";
    public static SourceMode Mode(CaseSnapshot value) => value.SchemaVersion == 1 ? SourceMode.Synthetic :
        value.Source?.Mode ?? throw new RefusalException("Missing live source context.");

    public static void Validate(CaseSnapshot value)
    {
        if (value.SchemaVersion == 1)
        {
            Contract.Require(value.Source is null && value.Evidence.All(e => e.Coverage is null &&
                e.Status is CollectionStatus.Observed or CollectionStatus.Unavailable or CollectionStatus.PermissionDenied or CollectionStatus.Failed or CollectionStatus.TimedOut),
                "Version 1 cannot contain version 2 source or coverage semantics. Original files are not migrated implicitly.");
            return;
        }
        var source = value.Source;
        Contract.Require(source is not null && Digest(source.MachineBinding) && Digest(source.UserBinding) &&
            Opaque(value.CaseId, "case-"), "Unsupported live source identity.");
        Contract.Require(source.Mode switch
        {
            SourceMode.WindowsStartup => value.SourceLabel == "windows:startup" && source.Scope is SourceScope.RunNative32 or SourceScope.RunNative64,
            SourceMode.WindowsReliability => value.SourceLabel == "windows:reliability" && source.Scope == SourceScope.ApplicationEventLog,
            _ => false
        }, "Unsupported live source identity or scope.");
        foreach (var e in value.Evidence)
        {
            var c = e.Coverage;
            Contract.Require(c is not null && Opaque(c.QueryId, "query-") && c.Scope == source.Scope &&
                Enum.IsDefined(c.Portion) && Enum.IsDefined(c.State) && c.Limit is >= 1 and <= 64 &&
                c.Examined >= 0 && c.Examined <= c.Limit + 1 && c.Returned >= 0 && c.Returned <= c.Limit &&
                c.Returned <= c.Examined, "Missing, incompatible or unbounded live query coverage.");
            Contract.Require(source.Mode == SourceMode.WindowsReliability
                ? c.Portion is QueryPortion.ApplicationEvents or QueryPortion.Collection
                : c.Portion != QueryPortion.ApplicationEvents, "Query portion and source differ.");
        }
    }

    public static void RequireExportableFixture(CaseSnapshot value)
    {
        Contract.Validate(value);
        Contract.Require(value.SchemaVersion == 1 && value.Source is null && FixtureLabel(value.SourceLabel) &&
            value.Evidence.All(e => e.DetailSchema != "startup-run.v2" && e.DetailSchema != "startup-coverage.v2" &&
                e.DetailSchema != "reliability-application.v2"),
            "Live AI/report export is blocked. Use the bounded local summary; case files are private working data.");
    }
}
