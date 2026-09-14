# Orthonis architecture

Document ID: `orthonis.doc.architecture`.

Status: OFC implements the fixture-backed read-only foundation below. Real Windows collection, real-data export controls, UI and repair boundaries remain requirements for later work, not shipped security claims.

## CLI-first development decision

Owner decision, 2026-09-14: keep Orthonis command-line-first while its useful capabilities and workflows take shape. Do not build a desktop shell, dashboard, web UI, TUI framework, UI mockups, or choose a UI toolkit during the remaining OFC slices. Help text, progress, readable terminal output, explicit approval, and tested exit codes are application functionality, not a reason to introduce a graphical framework.

Keep domain operations callable without console I/O. The CLI is one host, not the owner of collection, analysis, export policy, or authorization logic. Extract a small application service only when concrete reuse or complexity requires it; do not turn UI readiness into an empty framework or platform rewrite. A later UI must reuse the tested operations and be selected explicitly after the owner reviews real CLI workflows. Completing OFC does not automatically start UI development.

The [remaining campaign slices](campaigns/OFC_FOUNDATION.md#cli-first-continuation-plan) add live-data safeguards and bounded Windows adapters before broader features. This is a planned direction, not a claim that the current fixture sources already inspect Windows.

## Implemented component boundary

The product is a small modular desktop-application foundation, currently hosted through a CLI. Built-in modules are compiled and shipped together; there is no dynamic plugin loader or module marketplace.

| Component | Current responsibility |
| --- | --- |
| `Orthonis.Core` | Immutable cases/evidence/findings, explicit capabilities, bounded collection coordination, strict JSON, report rendering, discovery-plan validation and execution orchestration. |
| `Orthonis.Modules` | Startup and Reliability capability implementations; typed details, separate analysis functions, source interfaces and synthetic sources. References Core only. |
| `Orthonis.Cli` | Explicit component composition, commands, approval input, local case storage and console output. References Modules and its transitive Core dependency. |
| `Orthonis.Tests` | Executable behavioral regressions, including persistence fault injection; no live Windows data. |

The [contracts](../src/Orthonis.Core/Contracts.cs) and [Investigation coordinator](../src/Orthonis.Core/Investigation.cs) are shared. Adding [Reliability](../src/Orthonis.Modules/Reliability.cs) did not require modifying the existing [Startup implementation](../src/Orthonis.Modules/Startup.cs) or collection coordinator. The host explicitly registers known modules instead of discovering arbitrary assemblies. Module-specific source retrieval and interpretation stay outside the host.

Core must not call Windows APIs, filesystem storage, UI controls, or model SDKs. Keep source adapters behind interfaces and share the case/report/approval mechanisms. Avoid separate feature databases and direct cross-module call chains. A future UI invokes application operations rather than reimplementing policy.

## Evidence and findings

An observation retains its identity, originating module/capability, target identity and kind, observation time, collection outcome, detail schema, and typed serialized details. A finding references actual evidence. Cases and findings are immutable snapshots; collection creates the next revision rather than mutating the input case.

The current fixtures use observation time. Windows adapters must additionally represent actual event time and bounded query coverage where needed; do not infer event time from when a scan ran. Missing, failed, denied, timed-out, unsupported, and healthy are different outcomes. A coincident update is not proven causation, and two similar applications are not a demonstrated conflict.

The report retains all case evidence, including repeated observations. Repeated hang summaries are not new crashes, and an old missing-target finding does not prove the latest state. Richer current-versus-historical finding reconciliation is future work before presenting a live health dashboard. No historical baseline is invented when the application was not recording it.

## Discovery and repair proposals

The AI returns a work order, not executable code. [DiscoveryPlans](../src/Orthonis.Core/DiscoveryPlans.cs) binds version 1 discovery proposals to the case/revision/snapshot hash and existing evidence. The application advertises actual capabilities; targeted requests resolve known local IDs rather than model-provided paths. The entire batch is validated before a collector call. Preview performs no collection, and execution requires explicit local approval after revalidation.

[JsonCodec](../src/Orthonis.Core/JsonCodec.cs) rejects unknown fields/versions, duplicate JSON keys, missing required fields, malformed/truncated input, excessive size/depth, and non-finite numbers. Unknown capabilities, invalid targets, mismatched snapshot preconditions, duplicate requests and applied-plan replay fail closed. Bounds are in source and the [foundation guide](FOUNDATION_GUIDE.md). There is no generic elevated shell or repair command in this protocol.

Current target admission requires matching observed module/kind/identity in the case. It is not proof that a real Windows target still exists. Actual Windows operations must re-establish relevant live target preconditions. A digest identifies selected bytes; it is not authorization, authentication, or proof of a diagnosis. Copying a supported schema does not confer trust.

## Execution, recovery, and verification

The coordinator processes a bounded request list with cooperative cancellation and a per-collector timeout. Failure/denial/timeout produce explicit generic outcomes without copying exception details into reports. A malformed collector result is refused, not quietly accepted. In-process modules remain trusted code; a timeout or interface is not a sandbox and does not guarantee termination of a noncooperating operation.

The [case store](../src/Orthonis.Cli/CaseStore.cs) uses one cooperating writer lock, sequential revision checking and same-directory temporary-file replacement. Regression tests inject a failure before replacement and establish a readable unchanged prior case. This is not universal power-loss durability, authenticated storage, hostile-process isolation, or a guarantee of race-proof reparse confinement. Raw case files are local working data, not signed evidence.

For each future repair, specify applicability, exact prerequisites/effects, approval, expected observation, interruption handling, and realistic recovery. Journal uncertain outcomes. Windows changes are not universally atomic or reversible. Prefer one justified intervention and comparison over unrelated optimizations; no improvement is a useful negative result.

Broad registry deletion, firmware flashing, boot-configuration changes, and disabling security protections remain outside routine AI-directed maintenance. Consequential repair tests must start in a disposable Windows environment, not an everyday PC.

## Privacy and privilege

The present application only constructs synthetic sources. Its reports are not a validated real-data redaction system; the source label is not proof that manually edited case content is safe to export. Real collection must not silently reuse unrestricted fixture exports. Keep evidence local by default, minimize and preview exports, and add tested filtering/aliasing before live personal data leaves the machine. Memory dumps, full logs, unrestricted command lines and secrets must not be automatically exported.

Treat logs, reports, and AI output as untrusted data. The normal host/model process should not run as administrator. A future privileged helper exposes only authenticated, validated, narrowly scoped operations and rechecks requests itself. Other agent tools must not bypass that boundary. Privilege requirements belong to each operation; read-only does not imply no elevation on every Windows source.

## Development and execution evidence

GitHub connector access establishes selected committed source and actual writes/readback, not the state of a PC. Container checks establish only commands actually run there. CI establishes behavior on its recorded runner/source view. A real Windows pilot establishes only its tested machine/build/permissions/scenarios. Synthetic sources do not establish Windows collector accuracy.

EUTONOS supports development continuity through readable repository owners. It is not a product dependency. Preserve stable IDs and one work board, and keep host secrets/runtime state outside this public repository. Future EUTONOS runtime adoption is separate selected work.
