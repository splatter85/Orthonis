# Orthonis architecture

Document ID: `orthonis.doc.architecture`.

Status: OFC1/OFC2 provide the fixture-backed case/report/discovery foundation. OFC3 adds bounded Windows Startup and OFC4 adds bounded Windows Application Reliability collection. General real-data export, combined owner-PC acceptance, UI and repairs remain later work rather than shipped security claims. [OFC](campaigns/OFC_FOUNDATION.md) owns exact execution evidence.

## CLI-first development decision

Owner decision, 2026-09-14: keep Orthonis command-line-first while useful capabilities and workflows take shape. Do not build a desktop shell, dashboard, web UI, TUI framework, UI mockups or choose a UI toolkit during the remaining OFC slices. Help text, readable terminal output, cancellation, explicit approval and tested exit behavior are application functionality, not a reason to introduce a graphical framework.

The CLI is one host, not the owner of collection, interpretation, privacy policy or authorization. `CaseOperations` exposes application operations independently of console parsing; future UI code must reuse those operations rather than reimplementing policy.

## Implemented component boundary

| Component | Current responsibility |
| --- | --- |
| `Orthonis.Core` | Immutable cases/evidence/findings, explicit capabilities, schema-1 fixture and schema-2 live source/coverage contracts, bounded collection coordination, strict JSON, synthetic report rendering and discovery-plan validation/execution. |
| `Orthonis.Modules` | Portable synthetic Startup and Reliability modules. References Core only. |
| `Orthonis.Windows` | Windows-only Run-key source, Startup module, conservative command resolution/local executable probe; bounded native Application event-log source, Reliability module, event identity/history and metadata validation. No configuration/event-log writes or target execution API. |
| `Orthonis.Cli` | Explicit composition, reusable case operations, commands, local approval, local case storage and console output/export gating. |
| `Orthonis.Tests` | Foundation regressions, injected live-source adverse cases, privacy sentinels, fresh-process tests and native no-dump smoke entry points. |

Core does not call Windows APIs, filesystem storage, UI controls or model SDKs. Windows collection is isolated behind reviewed interfaces; there is no dynamic plugin loader, separate feature database, Windows service or privileged executor. OFC4 uses native interop without adding NuGet dependencies or changing existing Startup collectors.

## Evidence, source context and freshness

Schema-1 synthetic cases remain byte-compatible and do not gain live semantics implicitly. Schema-2 live cases require a validated source mode/scope and case-salted machine/user bindings. Startup and Reliability have separate explicit source identities and case directories in the same case system; existing cases are not silently converted or combined. Every live observation carries bounded coverage distinguishing complete, partial and not-queried outcomes.

Live Startup inventory uses opaque case-scoped target IDs backed by private persisted locators. Target admission from historical evidence is only the first gate: the Windows module re-establishes the selected machine/user/view context, rereads the original registration, and refuses silent remapping on drift/removal/substitution. Supported executable inspection rereads the registration again after filesystem observation so concurrent registration change supersedes a present/missing result.

Findings remain historical immutable evidence. Startup summary logic distinguishes a current missing-target attention finding from a later contradictory observation so an older finding is not presented as current state.

Reliability preserves event occurrence time separately from query/observation time, retains query history and counts distinct event identities rather than summing repeated queries. Record-ID reuse and changed log anchors remain explicit history uncertainty. Saved-case validation recomputes required query/history gap flags, including reused-ID warnings. A source binding change before/after the read produces stale evidence and discards records from that attempt.

## Conservative Windows Startup boundary

OFC3 reads only current-user and local-machine `Software\Microsoft\Windows\CurrentVersion\Run` in the process-native 32- or 64-bit registry view, at most 32 retained values per key. It excludes the alternate view, RunOnce, Startup folders, scheduled tasks, services, drivers and shell extensions. Unknown enablement stays unknown.

Command parsing supports only a narrow absolute local-drive `.exe` form. Environment-variable expansion, ambiguous quoting, launchers, shell/script/DLL/URL forms, relative/search-path targets, UNC paths and device paths are unsupported rather than guessed. `LocalExecutableProbe` opens local filesystem components for attributes only, rejects nonstandard drive mappings and reparse traversal, and distinguishes explicit missing statuses from permission/error outcomes. It never loads or executes the target.

Filesystem and registry observations remain races, not atomic snapshots. A supported result is evidence at observation time, not proof of future execution or causation.

## Conservative Windows Reliability boundary

[OFC4 Reliability](OFC4_RELIABILITY.md) owns the API selection and detailed collection contract. The adapter queries only the local Application channel for the preceding seven days, newest first, at most 64 retained records plus one limit sentinel. It selects levels 1/2/3 and Windows Error Reporting records. There is no arbitrary channel, query, date span or remote-session parameter in a discovery plan.

Only bounded System-envelope metadata is projected; payloads, localized messages and crash dumps are not persisted or interpreted. Unknown/missing envelopes remain incomplete. Query intervals, permission/failure/timeout outcomes, invalid records, limits and before/after retention anchors are explicit. Retention/clear observations are not atomic and cannot prove an uninterrupted history or distinguish every clear from retention. Event-record counts are not crash/incident counts and produce no causal or health findings.

All native event handles are used and closed in their creating synchronous worker. A finite EvtNext wait, capped XML allocation, local budget and coordinator cancellation bound cooperative work; they do not isolate a blocked native call. Render preflight preserves native permission/stale/timeout errors rather than relabeling them as oversized XML.

## Discovery and export policy

The AI returns a bounded discovery work order, not code. Whole-plan validation occurs before any collector call; preview performs no collection; execution requires explicit local approval after revision/hash revalidation. Unknown capabilities/targets/versions, duplicate requests, stale plans and replays fail closed.

Synthetic cases retain the existing Markdown report/manual AI exchange. Live cases cannot use `report` or `example-plan`; `SourceData.RequireExportableFixture` blocks that path in Core as well as the CLI. Live default output is a local/private bounded summary using opaque IDs and coverage. `local-plan` is deliberately local/private and does not mean the case has been sanitized for sharing. Reliability exposes only a bounded refresh, not an event-detail/dump/repair capability.

## Execution, recovery and privilege

Collection is bounded and cooperatively cancellable. Timeouts, permissions and failures produce explicit outcomes without exporting adapter exception details. `UnauthorizedAccessException` and `SecurityException` retain permission-denied semantics. In-process modules remain trusted code; cancellation is not a sandbox.

The case store uses one cooperating-writer lock, revision preconditions and same-directory replacement; failure injection proves only its defined before-replacement boundary. Raw case files are private local working data, not authenticated or signed evidence. Cancellation/refusal does not authorize replacing a valid stored revision.

The normal host does not elevate. The Windows adapters expose no registry/event-log write, clear, repair, shell, launcher or arbitrary-command escape hatch. Any future consequential repair needs a separate threat model, prerequisites, approval and recovery contract.

## Development evidence

GitHub connector access proves selected committed source, not a PC state. Hosted Windows CI can establish only the exact read-only runner behavior recorded in its job. It is not owner-PC acceptance. Keep EUTONOS as repository-file development continuity rather than a product runtime dependency, preserve stable IDs and one task board, and keep private diagnostics/runtime state outside this public repository.
