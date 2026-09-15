# OFC4 Windows Reliability contract

Document ID: `orthonis.doc.ofc4-reliability`.

## Selected boundary

OFC4 adds an explicit `windows:reliability` source in the existing schema-2 case system. It does not convert Startup cases, replace the synthetic module, combine live modules in one case or select OFC5. Source mode/scope, storage, approval and export policy remain shared. The one capability is `reliability.summary` version 2, a bounded refresh without arbitrary parameters or targets.

The Windows adapter reads the local Application channel only. The query covers the preceding seven days, includes levels 1/2/3 and Windows Error Reporting records, and reads newest-first with at most 64 retained entries and one extra limit sentinel. These are distinct event-record counts, not crash or incident counts. Multiple records can describe one incident. No cause, health score, repair or enablement inference is produced.

## API and dependency decision

Primary API documentation checked on 2026-09-15:

- [EvtQuery](https://learn.microsoft.com/en-us/windows/win32/api/winevt/nf-winevt-evtquery): local session is null, channel/XPath queries and thread affinity.
- [EvtNext](https://learn.microsoft.com/en-us/windows/win32/api/winevt/nf-winevt-evtnext): finite wait, explicit no-more-items and closing every event handle.
- [EvtRender](https://learn.microsoft.com/en-us/windows/win32/api/winevt/nf-winevt-evtrender): byte-counted, null-terminated Unicode XML rendering.
- [EvtGetLogInfo](https://learn.microsoft.com/en-us/windows/win32/api/winevt/nf-winevt-evtgetloginfo) and [log properties](https://learn.microsoft.com/en-us/windows/win32/api/winevt/ne-winevt-evt_log_property_id): creation time, record count and oldest record metadata.
- [Query flags](https://learn.microsoft.com/en-us/windows/win32/api/winevt/ne-winevt-evt_query_flags): strict channel query with newest-first results. Tolerate-query-errors is deliberately not used.
- [EVT_VARIANT](https://learn.microsoft.com/en-us/windows/win32/api/winevt/ns-winevt-evt_variant): selected scalar metadata layout and types.

Use a narrow `wevtapi.dll` interop adapter, not a new package or desktop framework. External NuGet feeds remain cleared. Native handles are created, read and closed in a single synchronous worker, never carried across awaits. Safe handles close query/log/event resources; XML buffers are capped at 64 KiB and freed. EvtNext waits at most 250 milliseconds; the adapter checks a three-second budget and the coordinator retains its five-second cooperative limit. This is not process isolation or a guarantee against a blocked kernel call. Buffer preflight preserves native permission/stale/timeout errors rather than reclassifying them as oversized XML.

## Evidence and gaps

Every adapter result records the exact UTC query interval, collection time, status, examined/retained counts, invalid-record count, before/after log metadata when available and immutable event metadata. Coordinator-level failures remain not-queried outcomes rather than fabricated completed intervals. Occurrence time is separate from collection time. The System-envelope projection retains channel, provider name/optional GUID, record ID, event ID/version/level and occurrence time. A digest distinguishes differing rendered records without persisting their payload. Public output omits provider strings, raw hashes and bindings.

Only the bounded standard System envelope is recognized. Missing version/level, future versions or unknown System fields remain unknown/incomplete; missing identity/time, duplicate fields, wrong channel, out-of-window records, malformed XML and oversized XML are rejected or recorded as unparsed. Provider-specific EventData/UserData payload schemas and localized messages are explicitly not interpreted. Recognizing an envelope does not establish that its payload, reported cause or provider is trustworthy.

Event identity is case/source scoped and includes record ID, provider, occurrence time and content fingerprint. Repeated reads are deduplicated within a query and across persisted history for distinct-record counts. Query history is retained rather than summing query counts. Reused record IDs with changed identity remain distinct and flag possible history change. Saved-case validation recomputes required query/history warnings so removing a reuse/continuity flag cannot turn retained contradictory evidence into a complete result.

Creation time, oldest record, oldest occurrence/fingerprint and record count are observed before and after collection. Missing metadata, a retained history beginning after the query start, changed anchors/count regression, unknown envelopes, invalid records and query limits remain explicit gaps. Differences cannot reliably distinguish retention from clearing; no false attribution is made. These are non-atomic observations, not proof that the log has never been cleared or that events were never delayed/lost.

Machine/user bindings are checked before and after collection. A changed source produces stale, empty evidence rather than silently attaching records from a different context. Source bindings/digests are provenance checks, not authentication of an editable case file.

## CLI

```sh
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- capabilities --windows-reliability
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- start-reliability .local/windows-reliability
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- summary .local/windows-reliability
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- local-plan .local/windows-reliability > .local/reliability-plan.json
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- apply .local/windows-reliability .local/reliability-plan.json
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- apply .local/windows-reliability .local/reliability-plan.json --approve
```

Use a new directory. No fallback to synthetic data is allowed on unsupported hosts. Local plans accept no Reliability target, arbitrary channel, query string, time span or execution text. Preview performs no collector calls; approval revalidates and persists the next revision. Replay and stale plans are refused without replacing saved state. Save redirected plan JSON as UTF-8.

`report` and `example-plan` remain blocked for live cases at both Core and CLI gates. Local summaries are not sanitized export guarantees. Keep case files and plans local/private. No dump, event message, remote log, System/Security channel, event write/clear, repair, elevation, model/provider call or upload is included.

## Verification boundary

OFC4 started from `16a881c0a99eeeda4a69eee0ce7c208fc1dd1518`, selected by Current Task in `3d944bc0448077e4226d69280639ffb447d4a095`. The first implementation failed two parser compile checks. Corrected source `2848e7fd8c6f48d37559018c3d721283f0c617de` passed hosted run `34947701494`, Windows job `104310923329` and Ubuntu job `104310923110`, including retained foundation/OFC3 checks, new OFC4 checks and repository checks. [OFC](campaigns/OFC_FOUNDATION.md#ofc4-bounded-windows-reliability-collection) owns the full historical result and closeout-publication evidence.

The added executable checks and `python tools/smoke_ofc4.py` cover controlled source outcomes, parser/privacy cases, overlapping/reused identities, source/history drift, saved-warning tampering, native buffer/error preflight, cancellation/timeout and fresh-process create/preview/approval/replay/reload. Synthetic fixture times are preserved across processes without tying the suite to one calendar week. Windows smoke invokes the real read-only Application adapter, requires a successful bounded query and channel metadata, and logs only pass/fail, never event details or artifacts. No event-log fixtures are written to Windows.

The authoring container has no .NET SDK, so no local C# execution is claimed. Hosted Windows evidence is not owner-PC acceptance. OFC5 remains unselected. Empty or incomplete results never certify PC health.
