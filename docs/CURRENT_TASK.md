# Current task

Document ID: `orthonis.doc.current-task`.

## State: OFC4 selected

Owner request, 2026-09-15: implement OFC4 only in `splatter85/Orthonis`. Source baseline: `16a881c0a99eeeda4a69eee0ce7c208fc1dd1518` on `codex/ofc-foundation`, draft PR #1. Main remains `561affc9197ad6e704d074ed4b022448d542c446`. No local PC state is inferred.

```tokenslang-work
{
  "profile": "eutonos.tokenslang.work.v1",
  "work_id": "orthonis.work.ofc4",
  "goal": "Implement and verify bounded read-only Windows Application event-log Reliability collection with explicit coverage, event identity, deduplication and local-only case handling; preserve Startup and synthetic workflows.",
  "status": "selected",
  "required": [
    {"scope_id":"orthonis.repo","namespace":"orthonis.native","resource_id":"orthonis.doc.ofc"},
    {"scope_id":"orthonis.repo","namespace":"orthonis.native","resource_id":"orthonis.doc.architecture"},
    {"scope_id":"orthonis.repo","namespace":"orthonis.native","resource_id":"orthonis.doc.workflow"},
    {"scope_id":"orthonis.repo","namespace":"orthonis.native","resource_id":"orthonis.doc.health"},
    {"scope_id":"orthonis.repo","namespace":"orthonis.native","resource_id":"orthonis.doc.foundation-guide"}
  ]
}
```

## Scope and acceptance

Follow [OFC4](campaigns/OFC_FOUNDATION.md#ofc4-bounded-windows-reliability-collection). Keep collection behind the Windows Reliability module and reuse case storage, source validation, plan preview/approval and export blocking. Preserve occurrence time, bounded query interval, channel/provider/record identity, retention/clear uncertainty and partial/query-limit outcomes. Repeated observations of an event must not inflate distinct-event counts. Missing/unknown schemas, empty queries and incomplete reads never establish PC health.

Verify the Windows API choice against primary documentation. Add controlled regressions for successful/empty/denied/failed/limited/timed-out/cancelled reads, schema uncertainty, overlapping scans and reused record IDs, source drift, persistence and privacy. Run existing foundation/OFC3 checks and new OFC4 checks on the hosted Linux/Windows CI boundary when available. A hosted Windows native read is not owner-PC acceptance.

## Exclusions and publication

CLI-first. No UI, repair, elevation, event-log writes/clears, crash-dump access/upload, arbitrary commands, live AI/export, provider calls, paid runners, deployment, merge or main changes. OFC5 remains unselected. Preserve OED1 history, stable identities, existing tests and one task board. Publish with expected-version/nonforced operations and verify readback.

## Checkpoint and restart

OFC3 remains complete at its recorded repository and hosted-Windows boundary; its historical implementation and verification are retained in [OFC](campaigns/OFC_FOUNDATION.md). OFC4 is in source/API review. The authoring container has no `dotnet` and cannot clone GitHub directly (DNS unavailable); connected GitHub reads/writes and hosted CI are the execution route. No OFC4 build or live read has run yet.

Next: implement the bounded Reliability source/contracts and explicit CLI composition, then focused tests, hosted checks and source-bound closeout. Stop at OFC4's completed or honestly blocked boundary.
