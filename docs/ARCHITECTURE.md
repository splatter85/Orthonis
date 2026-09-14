# Orthonis architecture

Document ID: `orthonis.doc.architecture`.

Status: OFC1/OFC2 provide the fixture-backed case/report/discovery foundation. OFC3 adds a bounded read-only Windows Startup adapter and live-data privacy boundary. Live Reliability collection, general real-data export, UI and repairs remain later work rather than shipped security claims.

## CLI-first development decision

Owner decision, 2026-09-14: keep Orthonis command-line-first while useful capabilities and workflows take shape. Do not build a desktop shell, dashboard, web UI, TUI framework, UI mockups, or choose a UI toolkit during the remaining OFC slices. Help text, readable terminal output, cancellation, explicit approval and tested exit behavior are application functionality, not a reason to introduce a graphical framework.

The CLI is one host, not the owner of collection, interpretation, privacy policy or authorization. `CaseOperations` exposes application operations independently of console parsing; future UI code must reuse those operations rather than reimplementing policy.

## Implemented component boundary

| Component | Current responsibility |
| --- | --- |
| `Orthonis.Core` | Immutable cases/evidence/findings, explicit capabilities, schema-1 fixture and schema-2 live source/coverage contracts, bounded collection coordination, strict JSON, synthetic report rendering and discovery-plan validation/execution. |
| `Orthonis.Modules` | Portable synthetic Startup and Reliability modules. References Core only. |
| `Orthonis.Windows` | OFC3 Windows-only Run-key source, live Startup module, conservative command resolution and local executable attribute probe. No registry write or target execution API. |
| `Orthonis.Cli` | Explicit composition, reusable case operations, commands, local approval, local case storage and console output/export gating. |
| `Orthonis.Tests` | Foundation regressions plus injected OFC3 adverse cases, privacy sentinels, fresh-process live-case tests and native no-dump smoke entry points. |

Core does not call Windows APIs, filesystem storage, UI controls or model SDKs. Windows collection is isolated behind reviewed interfaces; there is no dynamic plugin loader, separate feature database, Windows service or privileged executor.

## Evidence, source context and freshness

Schema-1 synthetic cases remain byte-compatible and do not gain live semantics implicitly. Schema-2 Windows Startup cases require a validated source mode/scope plus case-salted machine and user bindings. Every live observation carries a bounded query coverage record that distinguishes complete, partial and not-queried outcomes and retains observed, empty, denied, failed, timed-out, limited, unsupported and stale states.

Live Startup inventory uses opaque case-scoped target IDs backed by private persisted locators. Target admission from historical evidence is only the first gate: the Windows module re-establishes the selected machine/user/view context, rereads the original registration, and refuses silent remapping on drift/removal/substitution. Supported executable inspection rereads the registration again after the filesystem observation so concurrent registration change supersedes a present/missing result.

Findings remain historical immutable evidence. Live summary logic distinguishes a current missing-target attention finding from a later contradictory observation so an older finding is not presented as the current state.

## Conservative Windows Startup boundary

OFC3 reads only current-user and local-machine `Software\Microsoft\Windows\CurrentVersion\Run` in the process-native 32- or 64-bit registry view, at most 32 retained values per key. It excludes the alternate view, RunOnce, Startup folders, scheduled tasks, services, drivers and shell extensions. Unknown enablement stays unknown.

Command parsing supports only a narrow absolute local-drive `.exe` form. Environment-variable expansion, ambiguous quoting, launchers, shell/script/DLL/URL forms, relative/search-path targets, UNC paths and device paths are unsupported rather than guessed. `LocalExecutableProbe` opens local filesystem components for attributes only, rejects nonstandard drive mappings and reparse traversal, and distinguishes explicit missing statuses from permission/error outcomes. It never loads or executes the target.

Filesystem and registry observations are still races, not atomic snapshots. A supported result is evidence at observation time, not proof of future execution or causation.

## Discovery and export policy

The AI returns a bounded discovery work order, not code. Whole-plan validation occurs before any collector call; preview performs no collection; execution requires explicit local approval after revision/hash revalidation. Unknown capabilities/targets/versions, duplicate requests, stale plans and replays fail closed.

Synthetic cases retain the existing Markdown report/manual AI exchange. Live cases cannot use `report` or `example-plan`; `SourceData.RequireExportableFixture` blocks that path in Core as well as the CLI. Live default output is a local/private bounded summary using opaque IDs and coverage. `local-plan` is deliberately labeled local/private and does not mean the case has been sanitized for sharing.

## Execution, recovery and privilege

Collection is bounded and cooperatively cancellable. Timeouts, permissions and failures produce explicit generic outcomes without exporting adapter exception details. `UnauthorizedAccessException` and `SecurityException` are treated as permission-denied rather than generic health failures. In-process modules remain trusted code; cancellation is not a sandbox.

The case store uses one cooperating-writer lock, revision preconditions and same-directory replacement; failure injection proves only its defined before-replacement boundary. Raw case files are private local working data, not authenticated or signed evidence.

The normal host does not elevate. OFC3 imports no registry write API and exposes no repair, shell, launcher or arbitrary-command escape hatch. Any future consequential repair needs a separate threat model, prerequisites, approval and recovery contract.

## Development evidence

GitHub connector access proves selected committed source, not a PC state. Hosted Windows CI can establish only the exact read-only runner behavior recorded in its job. It is not owner-PC acceptance. Keep EUTONOS as repository-file development continuity rather than a product runtime dependency, preserve stable IDs and one task board, and keep private diagnostics/runtime state outside this public repository.
