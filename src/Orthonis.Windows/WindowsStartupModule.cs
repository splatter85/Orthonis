using System.Collections.Immutable;
using Orthonis.Core;

namespace Orthonis.Windows;

public sealed class WindowsStartupModule(IRunRegistry registry, IExecutableProbe probe) : ICaseDiagnosticModule
{
    public string Id => "startup";
    public ImmutableArray<Capability> Capabilities => [
        new("startup.inventory", 2, Id, "Read bounded HKCU/HKLM Run registrations in the declared native view; local evidence only.", null),
        new("startup.inspect", 2, Id, "Revalidate one case-scoped Run registration and inspect a supported local executable without execution.", RunIdentity.TargetKind)];
    public void ValidateCase(CaseSnapshot snapshot) => RunCase.Validate(snapshot);
    public Task<CollectionResult> CollectAsync(DiscoveryRequest request, CancellationToken cancellationToken) =>
        throw new RefusalException("A persisted case context is required for live collection.");

    public Task<CollectionResult> CollectAsync(CaseSnapshot snapshot, DiscoveryRequest request, CancellationToken cancellationToken) =>
        Task.Run(() => Collect(snapshot, request, cancellationToken), cancellationToken);

    private CollectionResult Collect(CaseSnapshot snapshot, DiscoveryRequest request, CancellationToken token)
    {
        ValidateCase(snapshot);
        token.ThrowIfCancellationRequested();
        var query = $"query-{Guid.NewGuid():N}";
        if (registry.Scope != snapshot.Source!.Scope || registry.Context(snapshot.CaseId) != snapshot.Source)
        {
            var coverage = new QueryCoverage(query, snapshot.Source.Scope, QueryPortion.Collection, CoverageState.NotQueried, 0, 0, 1);
            return new([new($"ev-{Guid.NewGuid():N}", request.CapabilityId, Id, request.TargetId ?? Id,
                request.TargetId is null ? "collection" : RunIdentity.TargetKind, DateTimeOffset.UtcNow, CollectionStatus.Unsupported,
                "collection-outcome.v1", JsonCodec.Element(new { reason = "Source context differs; no target was inspected." }), coverage)], []);
        }
        if (request.CapabilityId == "startup.inventory") return Inventory(snapshot, query, token);
        Contract.Require(request.CapabilityId == "startup.inspect" && request.TargetId is not null, "Unsupported Startup operation.");
        var previous = snapshot.Evidence.LastOrDefault(e => e.TargetId == request.TargetId && e.DetailSchema == "startup-run.v2");
        Contract.Require(previous is not null, "No persisted registration locator.");
        var locator = RunCase.Entry(previous).Locator;
        var read = registry.Read(locator.Hive, locator.ValueName, token);
        var presence = Revalidate(locator, read);
        if (presence == TargetPresence.NotChecked)
        {
            var path = RunCommand.Resolve(read.Value!);
            presence = path is null ? TargetPresence.Unsupported : probe.Inspect(path, token);
            // A second value read detects registration drift during the filesystem observation.
            var after = Revalidate(locator, registry.Read(locator.Hive, locator.ValueName, token));
            if (after != TargetPresence.NotChecked) presence = after;
        }
        token.ThrowIfCancellationRequested();
        Contract.Require(presence != TargetPresence.NotChecked && Enum.IsDefined(presence), "Invalid target probe result.");
        var c = new QueryCoverage(query, snapshot.Source.Scope, QueryPortion.Target,
            presence is TargetPresence.Present or TargetPresence.Missing ? CoverageState.Complete : CoverageState.Partial, 1, 1, 1);
        var evidence = Entry(snapshot, request.CapabilityId, locator, presence, c);
        ImmutableArray<Finding> findings = presence == TargetPresence.Missing ? [new($"finding-{Guid.NewGuid():N}", Severity.Attention,
            "The revalidated Run registration references a supported local executable that was absent at observation time. Enablement and performance impact are unknown; no repair is authorized.", [evidence.Id])] : [];
        return new([evidence], findings);
    }

    private CollectionResult Inventory(CaseSnapshot snapshot, string query, CancellationToken token)
    {
        var observations = ImmutableArray.CreateBuilder<Observation>();
        foreach (var hive in new[] { RunHive.CurrentUser, RunHive.LocalMachine })
        {
            token.ThrowIfCancellationRequested();
            RunListing listing;
            try { listing = registry.Enumerate(hive, token); }
            catch (UnauthorizedAccessException) { listing = new(CollectionStatus.PermissionDenied, [], 0); }
            catch (System.Security.SecurityException) { listing = new(CollectionStatus.PermissionDenied, [], 0); }
            catch (Exception ex) when (ex is not OperationCanceledException and not RefusalException)
            { listing = new(CollectionStatus.Failed, [], 0); }
            Contract.Require(!listing.Values.IsDefault && listing.Values.Length <= RunIdentity.PerKeyLimit &&
                listing.Examined >= listing.Values.Length && listing.Examined <= RunIdentity.PerKeyLimit + 1 &&
                listing.Status is CollectionStatus.Observed or CollectionStatus.Empty or CollectionStatus.Limited or CollectionStatus.PermissionDenied or
                    CollectionStatus.Failed or CollectionStatus.Unavailable or CollectionStatus.Unsupported, "Invalid registry query result.");
            foreach (var value in listing.Values) RunIdentity.ValidateValue(value);
            var groups = listing.Values.GroupBy(v => v.Name, StringComparer.OrdinalIgnoreCase).ToArray();
            var duplicate = groups.Any(g => g.Count() != 1);
            var values = groups.Where(g => g.Count() == 1).Select(g => g.Single()).ToArray();
            var status = duplicate ? CollectionStatus.Limited : listing.Status;
            Contract.Require(status is CollectionStatus.Observed or CollectionStatus.Limited || values.Length == 0, "Unavailable query supplied registrations.");
            Contract.Require(status != CollectionStatus.Empty || listing.Examined == 0, "Empty query has examined values.");
            var coverage = new QueryCoverage(query, snapshot.Source!.Scope,
                hive == RunHive.CurrentUser ? QueryPortion.CurrentUserRun : QueryPortion.LocalMachineRun,
                status is CollectionStatus.Observed or CollectionStatus.Empty ? CoverageState.Complete : CoverageState.Partial,
                listing.Examined, values.Length, RunIdentity.PerKeyLimit);
            foreach (var value in values)
            {
                var locator = new RunLocator(hive, registry.Scope, value.Name, RunIdentity.Registration(value));
                observations.Add(Entry(snapshot, "startup.inventory", locator, TargetPresence.NotChecked, coverage));
            }
            observations.Add(new($"ev-{Guid.NewGuid():N}", "startup.inventory", Id, Id, "collection", DateTimeOffset.UtcNow,
                status, "startup-coverage.v2", JsonCodec.Element(new RunCoverageResult(values.Length)), coverage));
        }
        return new(observations.ToImmutable(), []);
    }

    private static TargetPresence Revalidate(RunLocator locator, RunRead read)
    {
        if (read.Status == CollectionStatus.PermissionDenied) return TargetPresence.Inaccessible;
        if (read.Status == CollectionStatus.Unavailable || read.Status == CollectionStatus.Empty) return TargetPresence.Stale;
        if (read.Status != CollectionStatus.Observed || read.Value is null) return TargetPresence.Unknown;
        RunIdentity.ValidateValue(read.Value);
        return string.Equals(locator.ValueName, read.Value.Name, StringComparison.OrdinalIgnoreCase) &&
            RunIdentity.Registration(read.Value) == locator.RegistrationHash ? TargetPresence.NotChecked : TargetPresence.Stale;
    }

    public static CollectionStatus Status(TargetPresence presence) => presence switch
    {
        TargetPresence.NotChecked or TargetPresence.Present or TargetPresence.Missing => CollectionStatus.Observed,
        TargetPresence.Inaccessible => CollectionStatus.PermissionDenied,
        TargetPresence.Unsupported => CollectionStatus.Unsupported,
        TargetPresence.Stale => CollectionStatus.Stale,
        _ => CollectionStatus.Unavailable
    };

    private static Observation Entry(CaseSnapshot snapshot, string capability, RunLocator locator, TargetPresence presence, QueryCoverage coverage) =>
        new($"ev-{Guid.NewGuid():N}", capability, "startup", RunIdentity.Target(snapshot, locator), RunIdentity.TargetKind,
            DateTimeOffset.UtcNow, Status(presence), "startup-run.v2", JsonCodec.Element(new RunEntry(locator, Enablement.Unknown, presence)), coverage);
}
