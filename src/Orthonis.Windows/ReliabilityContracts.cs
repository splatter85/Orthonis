using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Orthonis.Core;

namespace Orthonis.Windows;

// Only System-envelope metadata is interpreted. Payloads/messages are neither persisted nor diagnosed.
public sealed record ApplicationEvent(string Channel, string Provider, Guid? ProviderId, ulong RecordId,
    int EventId, int? Version, int? Level, DateTimeOffset OccurredAt, string ContentHash, bool KnownEnvelope);
public sealed record LogBoundary(DateTimeOffset? CreatedAt, ulong? OldestRecord, ulong? RecordCount,
    DateTimeOffset? OldestOccurredAt, string? OldestHash);
public sealed record ApplicationRead(CollectionStatus Status, int Examined, int InvalidRecords,
    ImmutableArray<ApplicationEvent> Records, LogBoundary? Before, LogBoundary? After);
[Flags]
public enum ReliabilityGap
{
    None = 0, MetadataUnavailable = 1, HistoryStartsAfterWindow = 2, LogChangedDuringRead = 4,
    HistoryChangedSinceScan = 8, RecordIdReused = 16, UnknownEnvelope = 32, InvalidRecords = 64,
    QueryLimit = 128, SourceChanged = 256
}
public sealed record ReliabilityQuery(DateTimeOffset From, DateTimeOffset To, ApplicationRead Read, ReliabilityGap Gaps);

public interface IApplicationEventLog
{
    SourceContext Context(string caseId);
    ApplicationRead Read(DateTimeOffset from, DateTimeOffset to, CancellationToken token);
}

public static class ReliabilityData
{
    public const int Limit = 64;
    public const int MaxXmlBytes = 65536;
    public const string DetailSchema = "reliability-application.v2";
    public const string Channel = "Application";
    public static bool Text(string? value) => value is { Length: > 0 and <= 256 } && !value.Any(char.IsControl);
    public static bool Selected(ApplicationEvent value) => value.Level is 1 or 2 or 3 || value.Provider == "Windows Error Reporting";
    public static string Identity(CaseSnapshot snapshot, ApplicationEvent value) => "event-" + RunIdentity.Hash(new
        { snapshot.CaseId, snapshot.Source, value.Channel, value.RecordId, value.Provider, value.ProviderId,
            value.EventId, value.Version, value.OccurredAt, value.ContentHash });
    public static bool BoundaryKnown(LogBoundary? value) => value is { CreatedAt: not null, RecordCount: not null, OldestRecord: not null } &&
        (value.RecordCount == 0 || (value.OldestOccurredAt is not null && SourceData.Digest(value.OldestHash)));
    public static bool HistoryChanged(LogBoundary earlier, LogBoundary later) => earlier.CreatedAt != later.CreatedAt ||
        earlier.OldestRecord != later.OldestRecord || earlier.OldestHash != later.OldestHash || later.RecordCount < earlier.RecordCount;

    // Native buffer preflight is portable/testable. Do not relabel permission or stale errors as oversized XML.
    public static void ValidateRenderProbe(bool succeeded, int error, int required)
    {
        if (succeeded) throw new Win32Exception(13);
        if (error != 122) throw new Win32Exception(error);
        if (required < 2 || required > MaxXmlBytes || required % 2 != 0) throw new Win32Exception(122);
    }

    public static ApplicationEvent Parse(string xml)
    {
        Contract.Require(xml.Length <= MaxXmlBytes / 2, "Event XML exceeds the selected bound.");
        using var input = new StringReader(xml);
        using var reader = XmlReader.Create(input, new XmlReaderSettings
        { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = MaxXmlBytes / 2 });
        var root = XDocument.Load(reader, LoadOptions.None).Root;
        XNamespace ns = "http://schemas.microsoft.com/win/2004/08/events/event";
        Contract.Require(root is not null && root.Name == ns + "Event" && root.Elements(ns + "System").Count() == 1,
            "Missing or unknown event envelope.");
        var system = root.Element(ns + "System")!;
        Contract.Require(system.Elements().GroupBy(e => e.Name).All(g => g.Count() == 1), "Ambiguous event envelope.");
        string? Value(string name) => system.Element(ns + name)?.Value;
        var provider = system.Element(ns + "Provider");
        var name = (string?)provider?.Attribute("Name");
        var guidText = (string?)provider?.Attribute("Guid");
        var hasRecord = ulong.TryParse(Value("EventRecordID"), NumberStyles.None, CultureInfo.InvariantCulture, out var record);
        var hasEvent = int.TryParse(Value("EventID"), NumberStyles.None, CultureInfo.InvariantCulture, out var eventId);
        Contract.Require(Text(name) && Value("Channel") == Channel && hasRecord && record > 0 &&
            hasEvent && eventId is >= 0 and <= 65535, "Missing event identity.");
        var time = (string?)system.Element(ns + "TimeCreated")?.Attribute("SystemTime");
        Contract.Require(time is not null && time.EndsWith('Z') && DateTimeOffset.TryParse(time,
            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out _), "Missing occurrence time.");
        var occurred = DateTimeOffset.Parse(time, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        Guid? guid = null;
        if (guidText is not null)
        {
            Contract.Require(Guid.TryParse(guidText, out var parsed), "Malformed provider identity.");
            guid = parsed;
        }
        int? Number(string field) => int.TryParse(Value(field), NumberStyles.None, CultureInfo.InvariantCulture, out var n) && n is >= 0 and <= 255 ? n : null;
        var version = Number("Version");
        var level = Number("Level");
        string[] known = ["Provider", "EventID", "Version", "Level", "Task", "Opcode", "Keywords", "TimeCreated",
            "EventRecordID", "Correlation", "Execution", "Channel", "Computer", "Security"];
        var supported = version == 0 && level is >= 0 and <= 5 && system.Elements().All(e => e.Name.Namespace == ns && known.Contains(e.Name.LocalName));
        return new(Channel, name!, guid, record, eventId, version, level, occurred, RunIdentity.Hash(xml), supported);
    }

    public static ReliabilityQuery Query(Observation e) => JsonCodec.Decode<ReliabilityQuery>(JsonCodec.Encode(e.Detail));
    public static void ValidateRead(ApplicationRead read, DateTimeOffset from, DateTimeOffset to)
    {
        Contract.Require(read is not null && Enum.IsDefined(read.Status) && !read.Records.IsDefault &&
            read.Examined is >= 0 and <= Limit + 1 && read.Records.Length <= Limit && read.InvalidRecords >= 0 &&
            read.InvalidRecords <= read.Examined - read.Records.Length, "Invalid bounded Application read.");
        foreach (var e in read.Records)
            Contract.Require(e is not null && e.Channel == Channel && Text(e.Provider) && e.RecordId > 0 &&
                e.EventId is >= 0 and <= 65535 && (e.Version is null or >= 0 and <= 255) && (e.Level is null or >= 0 and <= 255) &&
                e.OccurredAt >= from && e.OccurredAt <= to && e.OccurredAt.Offset == TimeSpan.Zero && SourceData.Digest(e.ContentHash) && Selected(e) &&
                (!e.KnownEnvelope || (e.Version == 0 && e.Level is >= 0 and <= 5)), "Invalid or out-of-query event metadata.");
        foreach (var b in new[] { read.Before, read.After }.OfType<LogBoundary>())
            Contract.Require((b.CreatedAt is null || b.CreatedAt != default(DateTimeOffset)) &&
                (b.OldestHash is null || SourceData.Digest(b.OldestHash)) &&
                (b.RecordCount != 0 || b.OldestOccurredAt is null), "Invalid log boundary.");
        Contract.Require(read.Status != CollectionStatus.Empty || (read.Records.IsEmpty && read.Examined == 0), "Contradictory empty query.");
        Contract.Require(read.Status != CollectionStatus.Observed || !read.Records.IsEmpty, "Observed query has no records.");
    }

    public static ReliabilityGap Gaps(ApplicationRead read, DateTimeOffset from, ReliabilityQuery? prior)
    {
        var gaps = ReliabilityGap.None;
        if (!BoundaryKnown(read.Before) || !BoundaryKnown(read.After)) gaps |= ReliabilityGap.MetadataUnavailable;
        if (read.Before?.OldestOccurredAt is null || read.Before.OldestOccurredAt > from || read.Before.CreatedAt > from)
            gaps |= ReliabilityGap.HistoryStartsAfterWindow;
        if (BoundaryKnown(read.Before) && BoundaryKnown(read.After) && HistoryChanged(read.Before!, read.After!))
            gaps |= ReliabilityGap.LogChangedDuringRead;
        if (prior is not null && BoundaryKnown(prior.Read.After) && BoundaryKnown(read.Before) && HistoryChanged(prior.Read.After!, read.Before!))
            gaps |= ReliabilityGap.HistoryChangedSinceScan;
        if (read.Records.Any(e => !e.KnownEnvelope)) gaps |= ReliabilityGap.UnknownEnvelope;
        if (read.InvalidRecords > 0) gaps |= ReliabilityGap.InvalidRecords;
        if (read.Status == CollectionStatus.Limited || read.Examined > Limit) gaps |= ReliabilityGap.QueryLimit;
        return gaps;
    }

    public static void ValidateCase(CaseSnapshot snapshot)
    {
        Contract.Validate(snapshot);
        Contract.Require(SourceData.Mode(snapshot) == SourceMode.WindowsReliability && snapshot.Findings.IsEmpty,
            "A Windows Reliability case without inferred diagnoses is required.");
        ReliabilityQuery? prior = null;
        var identities = new Dictionary<ulong, string>();
        var reusedRecordId = false;
        foreach (var e in snapshot.Evidence)
        {
            Contract.Require(SourceData.Opaque(e.Id, "ev-") && e.ModuleId == "reliability" && e.CapabilityId == "reliability.summary" &&
                e.TargetId == "reliability" && e.TargetKind == "collection", "Invalid Reliability evidence route.");
            if (e.DetailSchema == "collection-outcome.v1")
            {
                Contract.Require(e.Status is CollectionStatus.PermissionDenied or CollectionStatus.Failed or CollectionStatus.TimedOut or CollectionStatus.Unavailable or CollectionStatus.Unsupported &&
                    e.Coverage!.Portion == QueryPortion.Collection && e.Coverage.State == CoverageState.NotQueried &&
                    e.Coverage.Examined == 0 && e.Coverage.Returned == 0, "Invalid failed Reliability collection.");
                continue;
            }
            Contract.Require(e.DetailSchema == DetailSchema, "Unsupported Reliability detail schema.");
            var q = Query(e);
            Contract.Require(q.From != default && q.To > q.From && q.To - q.From == TimeSpan.FromDays(7) && q.To <= e.ObservedAt &&
                q.From.Offset == TimeSpan.Zero && q.To.Offset == TimeSpan.Zero && ((int)q.Gaps & ~511) == 0, "Invalid Reliability interval or gap state.");
            ValidateRead(q.Read, q.From, q.To);
            Contract.Require(e.Coverage!.Portion == QueryPortion.ApplicationEvents && e.Coverage.Limit == Limit &&
                e.Coverage.Examined == q.Read.Examined && e.Coverage.Returned == q.Read.Records.Length && e.Status == q.Read.Status &&
                q.Read.Records.Select(r => Identity(snapshot, r)).Distinct(StringComparer.Ordinal).Count() == q.Read.Records.Length,
                "Contradictory Reliability coverage or duplicate events within a query.");
            var complete = q.Gaps == ReliabilityGap.None && e.Status is CollectionStatus.Observed or CollectionStatus.Empty;
            Contract.Require(e.Coverage.State == (complete ? CoverageState.Complete : CoverageState.Partial), "Contradictory Reliability completeness.");
            foreach (var r in q.Read.Records)
            {
                var identity = Identity(snapshot, r);
                if (identities.TryGetValue(r.RecordId, out var existing) && existing != identity) reusedRecordId = true;
                identities[r.RecordId] = identity;
            }
            var requiredGaps = Gaps(q.Read, q.From, prior);
            if (reusedRecordId) requiredGaps |= ReliabilityGap.RecordIdReused;
            Contract.Require((q.Gaps & requiredGaps) == requiredGaps, "Missing required historical or query gap indication.");
            Contract.Require(!q.Gaps.HasFlag(ReliabilityGap.SourceChanged) ||
                (q.Read.Status == CollectionStatus.Stale && q.Read.Examined == 0 && q.Read.Records.IsEmpty), "Source change must discard records.");
            prior = q;
        }
    }

    public static ImmutableArray<ApplicationEvent> DistinctRecords(CaseSnapshot snapshot)
    {
        ValidateCase(snapshot);
        return snapshot.Evidence.Where(e => e.DetailSchema == DetailSchema).SelectMany(e => Query(e).Read.Records)
            .GroupBy(e => Identity(snapshot, e), StringComparer.Ordinal).Select(g => g.First()).ToImmutableArray();
    }
    public static string Summary(CaseSnapshot snapshot)
    {
        ValidateCase(snapshot);
        var text = new StringBuilder("LOCAL / PRIVATE Windows Reliability summary. Not an AI export or a sanitized case file.\n");
        text.AppendLine($"Case: {snapshot.CaseId}; revision: {snapshot.Revision}; scope: ApplicationEventLog.");
        text.AppendLine("Selection: local Application channel, levels 1/2/3 plus Windows Error Reporting; preceding seven days; newest 64 records plus one limit sentinel.");
        var last = snapshot.Evidence.LastOrDefault();
        if (last is not null)
        {
            text.AppendLine($"Latest query: {last.Status}; {last.Coverage!.State}; examined {last.Coverage.Examined}; retained {last.Coverage.Returned}.");
            if (last.DetailSchema == DetailSchema)
            {
                var q = Query(last);
                text.AppendLine($"Interval: {q.From:O} through {q.To:O}; observed {last.ObservedAt:O}; gaps: {q.Gaps}.");
                text.AppendLine($"Unparsed records: {q.Read.InvalidRecords}; unknown envelopes: {q.Read.Records.Count(r => !r.KnownEnvelope)}.");
                foreach (var r in q.Read.Records)
                    text.AppendLine($"Record {Identity(snapshot, r)}: occurred {r.OccurredAt:O}; level {r.Level}; envelope {(r.KnownEnvelope ? "known" : "unknown")}.");
            }
        }
        text.AppendLine($"Distinct retained event records across history: {DistinctRecords(snapshot).Length}. Overlapping scans are deduplicated; this is not an incident/crash count.");
        text.AppendLine("Payloads, localized messages, dump files and causes are not interpreted. Multiple records may describe one incident. History can be cleared/retained away, delayed or incomplete; no atomic snapshot is promised.");
        text.AppendLine("Excluded: System/Security and other channels, remote logs, arbitrary queries, repairs, dump access and live AI/export. Empty/incomplete queries never establish a healthy PC.");
        return text.ToString(); // Never print provider strings, raw XML, hashes, source bindings or exception messages.
    }
}
