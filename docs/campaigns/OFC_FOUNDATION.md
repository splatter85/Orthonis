# OFC: Orthonis Foundation Campaign

Document ID: `orthonis.doc.ofc`.

## Outcome and admission

Prove a small modular application with one case/evidence core, built-in diagnostic modules, and a controlled manual AI discovery loop. The owner approved this recommendation and requested work on 2026-09-14. OFC1 and OFC2 were admitted in order. Both reached the fixture-based construction and execution boundary below. The real Windows pilot remains unrun, so this is not complete product or launch acceptance.

Baseline: `splatter85/Orthonis/main` at `561affc9197ad6e704d074ed4b022448d542c446`. Implementation branch: `codex/ofc-foundation`, review PR #1. Campaign admission was committed before product implementation as `36d8e14dbca6e238a5b014700e1ba104ba8fe0b9`. Preserve EUTONOS native owners and OED1 history. Current Task alone selects live work; this file owns slice definitions and results.

Scope included source, executable tests, pinned toolchain and bounded hosted workflow, plus directly affected project/architecture/workflow/health/NAV/catalog owners. Publication is to the task branch and a review PR, not automatic merge or deployment. No upstream EUTONOS modifications, real-machine collectors/repairs, model calls, private data, credentials, telemetry, paid runners, artifact/cache uploads, external plugins, or account changes were part of the implementation.

## Required reading

Start at AGENTS and Current Task, then [Architecture](../ARCHITECTURE.md), [Workflow](../WORKFLOW.md) and [Project Health](../PROJECT_HEALTH.md). [Project](../PROJECT.md) distinguishes current capabilities from intended scope. [Foundation guide](../FOUNDATION_GUIDE.md) owns usage and Windows handoff. Source/tests are implementation evidence; documentation or parsing alone is not Windows acceptance.

## OFC1: One complete read-only path

Outcome: buildable C#/.NET CLI creates a synthetic case, uses a fixture-backed Startup module, records typed evidence and findings, saves a versioned case, and exports a report. Windows enumeration was excluded.

Acceptance: successful build; normal/missing/inconclusive/denied and invalid-data checks; demonstrated start/report commands; explicit outcomes; no startup changes. Core stays independent of filesystem storage, Windows APIs, desktop UI and provider SDKs. Built-in composition is sufficient.

Result: implemented at `88ac39fd9398b8a69089cf510e2dd24514681509`, tree `db0e2711fb68601367091e432d73f27f96addd3c`. GitHub run `34870171068` passed on Ubuntu and Windows runners: build, 24 C# checks, CLI start/report, repository consistency and repository regression suite. The inspected Ubuntu log reported SDK 10.0.401 and zero compiler warnings/errors. This was actual fixture-code execution, not a Windows startup scan.

## OFC2: Second module and manual discovery round trip

Dependency: OFC1's checked core. Add Reliability without modifying the Startup module or shared collection coordinator. Export actual capabilities and a concrete return example. Validate the whole proposal before any calls; require explicit approval for collection; persist the next case revision and applied-plan ID; refuse invalid/stale/replayed work.

Acceptance: build and expanded regression suite; two-module CLI round trip; malformed/duplicate/unknown/oversized/version/target/evidence/snapshot cases fail closed; no-call checks on invalid/unauthorized requests; invalid/replayed CLI plans preserve saved case bytes; collection failure is not healthy; pre-replacement storage failure preserves prior state. No arbitrary commands, repair execution, live AI service, or real-data privacy claims.

Result: implemented at `6b5af33c0d1150c9f7fb7acc5bc98c265d634cc9`, tree `f486d1a75379b84983bc40446d39eb9b0165b5ca`. GitHub PR run `34871192918` completed successfully on `ubuntu-24.04` and `windows-2022`; jobs `104067202156` and `104067201731`. The checked PR merge view was `684cff21ff1c7ad7a20177ed103c8557ae8633a8`; GitHub readback confirmed its tree exactly equals the implementation tree. This test merge did not merge the PR into main.

The expanded C# executable contains 53 passing checks. The Ubuntu log confirms 53 passed, zero failed, zero compiler warnings/errors, and 24 Python repository tests OK. Both jobs passed the complete suite, CLI round-trip smoke and repository checks. Platform-dependent repository symlink coverage should be read from each job log rather than inferred from a green job.

The separate-process smoke demonstrated case creation/export, preview with unchanged case bytes, invalid plan refusal with unchanged bytes, approved follow-up, replay refusal with unchanged bytes, and fresh-process revision-2 report. No model call was needed; the returned example was generated deterministically. This proves the protocol path, not that a model made an independent useful diagnosis.

## Architecture result and review

Reliability was composed into the host through the existing module/source pattern. Startup and the collection coordinator remained unchanged in OFC2. The common case store, report and proposal validator serve both modules. There is no dynamic plugin loading, separate feature database, Windows service, or privileged executor.

Adverse cases are retained as assertions, not swallowed failures. Unsupported requests produce refusal, denied/failed collection has distinct outcomes, and absent attention findings do not claim PC health. Storage failure injection proves only its defined before-replacement boundary. Same-agent implementation and test review are not independent security review.

The toolchain could not be installed in the authoring container, whose direct GitHub clone and Microsoft SDK download attempts failed. Local Python syntax/XML/JSON checks were possible; C# compilation and execution evidence came from actual GitHub runners. No local C# build is claimed.

## CLI-first continuation plan

Owner direction, 2026-09-14: remain command-line-first until actual functionality makes the future UI clearer, revise this campaign against the implemented foundation, and provide a next-agent build handoff. The presentation decision is owned by [Architecture](../ARCHITECTURE.md#cli-first-development-decision). This amendment is planning/documentation only; it does not implement or run OFC3. A receiving assignment must select OFC3 in Current Task before execution. OFC4 and OFC5 remain planned, not automatically dispatched.

Continuation baseline: `fa2b5dc567213043ac7871a43c278c598ff81446` on `codex/ofc-foundation`. Its final foundation checks passed in run `34871838719` on `ubuntu-24.04` and `windows-2022`, as recorded in PR #1. The amendment's containing commit supplies the new documentation identity. Preserve OFC1/OFC2 source and evidence; do not rebuild the repository setup or infer a main merge. Continue on the same task branch unless the owner changes the branch policy. If the remote has advanced, reconcile the current source rather than resetting to the historical pin.

| Slice | Outcome | Current boundary |
| --- | --- | --- |
| OFC1 | Fixture-backed Startup case/report path | Completed at its recorded synthetic boundary. |
| OFC2 | Reliability module and validated manual discovery loop | Completed at its recorded synthetic boundary. |
| OFC3 | Live-data-aware CLI and bounded Windows Startup collection | Next implementation slice; prepared, not started by this amendment. |
| OFC4 | Bounded Windows Reliability collection in the same case system | Planned after OFC3; no implementation selected. |
| OFC5 | Cross-module Windows pilot and privacy/export decision | Planned after the live adapters; no implementation selected. |

UI, repair execution, broader maintenance modules, installer, signing/distribution and live AI adapters are outside OFC3-OFC5. Decide a broader build/launch campaign after the real CLI evidence; do not silently equate this foundation with a finished utility.

## Source findings that shape the next slice

At the continuation baseline, [Program.CreateEngine](../../src/Orthonis.Cli/Program.cs) accepts only four `synthetic:` labels and always composes both fixture sources. The existing commands `start`, `report` and approved `apply` automatically call [Report.Render](../../src/Orthonis.Core/Report.cs), which prints a synthetic banner and all retained evidence. Replacing a source without changing those paths would mislabel or disclose live data.

[Contracts.cs](../../src/Orthonis.Core/Contracts.cs) records observation time but not an explicit collection-scope/coverage contract. [StartupEntry](../../src/Orthonis.Modules/Startup.cs) contains a Boolean `Enabled` and no persisted Windows locator or source fingerprint. [ReliabilityEntry](../../src/Orthonis.Modules/Reliability.cs) contains a hang count and window length, not individual event identities/times. The analyses use fixture-specific wording. The interfaces are useful starting boundaries, not a reason to squeeze live semantics into fields that cannot represent them.

Discovery currently validates targets against historical case evidence. A live adapter also needs to resolve the selected local target and recheck relevant source state. Retain the existing strict parsing, full-batch validation, approval, revision/digest binding and replay tests while making only the necessary versioned contract changes. Do not assert that a past missing-target finding is current after a later contradictory inspection.

## OFC3: Live-data-aware CLI and bounded Windows Startup collection

Work identity for a receiving assignment: `orthonis.work.ofc3`.

Outcome: an opt-in CLI path reads a declared subset of Windows startup registrations, stores a clearly identified local live case, and permits targeted read-only inspection after a fresh-process reload. Synthetic scenarios and their report/plan smoke remain usable. No startup item is created, executed, disabled or deleted.

### Change boundary and implementation order

First inspect the existing contracts/host/report/storage paths, then implement the minimum shared live-data boundary and one real source adapter as a working end-to-end slice. Do not stop after a plan or empty interfaces when implementation tools are available. OS-specific code belongs behind a Windows adapter boundary, preferably a small `src/Orthonis.Windows/` project if that keeps references clean; that directory is proposed and does not exist at the baseline. Core and the fixture suite must remain runnable on Linux. Do not add a Windows desktop-framework dependency just to read the registry.

1. Make source mode, source scope, collection time and coverage explicit and validated. Preserve unavailable/denied/partial/truncated/unsupported distinctions rather than treating a shortened scan as complete. Unknown source modes and incompatible saved cases fail with an actionable outcome; never fall back from a requested live scan to fixtures. Required incompatible data/detail/capability changes need a versioned route with old-case compatibility or explicit migration, source preservation and rejection of stale pre-migration plans. A source label is provenance metadata, not authentication or a redaction guarantee.
2. Implement a bounded read-only Startup inventory for the current user's and local machine's `Software\Microsoft\Windows\CurrentVersion\Run` keys. Declare the intended registry view(s), handle unavailable/denied views and prevent view aliasing from double-counting. `RunOnce`, Startup folders/shortcuts, Task Scheduler, services, drivers, shell extensions and other startup surfaces are explicitly not covered by OFC3. Display that limitation. Presence in `Run` is not proof of effective enabled state, actual execution or measured boot delay; represent unknown enablement instead of forcing it into `true`.
3. Give targets opaque case-scoped IDs backed by validated local locators that survive a separate CLI process. Bind them to the selected machine/user/source scope and original relevant value state without putting sensitive identifiers into public output. On inspection re-read the mapped registration. A changed, removed, substituted, ambiguous or wrong-scope target yields a stale/unavailable/unsupported outcome, not an invented match. Read-only inspection may observe a current file absence at the same target, but must not retarget a changed registration silently.
4. Parse only a conservative documented subset of startup command lines for target existence checks. Never launch the referenced executable, shell, script, DLL, URL or association. Preserve ambiguous quoting, unresolved environment variables, launcher arguments, relative/search-path targets and network/device paths as unknown/unsupported unless explicitly and safely supported. Do not probe remote shares or activate targets. A false `File.Exists` result alone is not evidence of absence because errors/permissions can also return false; retain enough error classification to distinguish missing from inaccessible. Review link/reparse and filesystem race limitations honestly.
5. Keep live evidence local. Separate a local inspection summary from an AI/export report. In OFC3, fail closed on live AI/export payloads until an explicitly tested privacy path is selected later. Fix every implicit output route, including `start`, `report`, approved `apply`, exception paths and example generation, so live raw evidence/commands are not silently printed as a paste-ready report. Default output should be bounded summaries, opaque IDs, outcomes and coverage. Any detailed local display requires a deliberate local-view choice and warning; it is not a sanitized export. The synthetic report loop remains supported and correctly labeled. Live local discovery may use supported case-bound requests and the same approval controls without enabling cloud sharing.
6. Keep console parsing/formatting in the CLI and collection/analysis/policy in their appropriate components. Preserve the existing commands unless an intentional compatibility change is documented. Define and test any new command/flag rather than advertising a proposed name as already implemented. No UI shell, provider SDK, generic command executor, privileged service or dynamic plugin loader.

### Acceptance and stop boundary

Run the existing 53-check C# foundation suite, Python repository suite, full build and separate-process fixture smoke before and after changes. Extend the tests rather than preserving an arbitrary count. Add targeted cases for registry view identity, empty/denied/partial/limited enumeration, malformed values, conservative command parsing, unknown enablement, source-mode mismatch, wrong-machine/user scope, cross-process target resolution, drift/removal, cancellation/timeout, and stale/replayed/unsupported requests. If a shared schema changes, retain a fixed version-1 case/plan fixture and test the actual compatibility/migration refusal contract.

Add negative tests proving that every live export/implicit-report route refuses or emits only its approved local summary, while synthetic reports still work. Use synthetic privacy sentinel strings, not real secrets. Retain no-call assertions for invalid/unapproved plans and unchanged prior case bytes on refused requests/storage failure. A safe historical-versus-current finding presentation is required for live refresh; a full diagnosis engine is not.

Demonstrate a CLI create -> reload -> targeted inspection -> reload path with injected Windows-source seams, plus a real read-only invocation on an available Windows host when possible. Do not create live autorun entries to arrange a test; use fake source seams or a disposable test abstraction for mutations. Windows CI may perform a no-dump read smoke, recording only pass/fail and nonidentifying counts. Never print raw registrations, full command lines, private paths or real cases into Actions logs/artifacts. A genuine empty result is a valid scenario, not permission to claim broad startup coverage.

Record three separate outcomes: portable build/fixture behavior; Windows adapter execution on its actual runner; owner-PC acceptance. If no Windows host is available, finish feasible adapter/source-seam work and checkpoint Windows execution as unrun rather than claiming completion or switching to unrelated UI. No real machine read is implied by a repository connection. Stop after OFC3's documented result or its specific blocked boundary; do not automatically start OFC4/OFC5.

## OFC4: Bounded Windows Reliability collection

Planned only. Dependency: OFC3's live-data/source/privacy contract and checked shared core. Add a Windows implementation behind `IReliabilitySource` or an explicitly versioned successor, retaining Startup behavior. Keep the same case, approval, persistence and source-mode controls.

Select a small, documented Application event-log scope for hangs/crashes, with specific providers/IDs/schema versions validated against actual source. Preserve event occurrence time separately from observation time, queried interval, record/channel/provider identity, grouping basis, retention/clear gaps and partial/query-limit outcomes. Repeated scans of the same events must not inflate counts. Missing fields and unknown provider schemas remain unsupported/incomplete, not evidence invented from localized rendered message text. An implicated module is a clue, not a proven culprit; no-event and incomplete-query results do not certify system health.

Choose the supported Windows API and dependency from current primary documentation at implementation. `NuGet.Config` currently clears feeds; a necessary Windows-only package needs explicit narrow configuration and a pinned reviewed version, not a broad silent relaxation or a UI framework dependency. Read-only collection stays local with OFC3's export gate. Include source-seam tests for duplicates, ordering, missing/cleared/denied logs, time windows, cancellation and bounded output; record actual Windows read results separately. No crash-dump upload or repair is part of this slice.

## OFC5: Cross-module Windows pilot and privacy/export decision

Planned only. Dependency: the two live adapters. Exercise both through the CLI on an explicitly selected Windows PC, with actual build/architecture, permissions and source coverage recorded privately where needed. Validate fresh-process pickup, case persistence, source drift, local follow-up requests, cancellation, historical finding presentation and representative absence/denial outcomes. Public campaign evidence must not include raw machine diagnostics or personal identifiers.

Decide whether the live report/AI return loop is ready for an export sub-slice. Either keep the live export gate closed and record that limitation, or implement an explicitly selected minimized field projection/aliasing policy with preview and negative leakage tests before any export. No model/provider call or live upload is required for protocol verification. An internal local snapshot binding must remain separate from the minimized payload; export consistency must not depend on including private fields in the AI report.

Close the foundation's actual Windows boundary from observed evidence, then propose the next useful functional campaign. UI readiness requires stable, demonstrated workflows and a separate owner decision; no launch date, graphical design or repair activation is implied by reaching OFC5.

## Windows implementation reference routes

Primary API guidance checked for this planning amendment on 2026-09-14:

- [Run and RunOnce](https://learn.microsoft.com/en-us/windows/win32/setupapi/run-and-runonce-registry-keys): registrations are command lines and do not provide boot-delay measurements. OFC3 intentionally includes only Run.
- [RegistryView](https://learn.microsoft.com/en-us/dotnet/api/microsoft.win32.registryview?view=net-10.0): select view semantics explicitly; a requested 64-bit view can alias the 32-bit view on a 32-bit OS.
- [File.Exists](https://learn.microsoft.com/en-us/dotnet/api/system.io.file.exists?view=net-10.0): false can represent an error or lack of permission, not only absence.
- [EventLogQuery](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.eventing.reader.eventlogquery?view=windowsdesktop-10.0): an API investigation route for OFC4, not a selected package version or proof that every event schema is supported.

Reverify relevant APIs/packages when implementing. These references do not expand the slice's allowed effects.

## Planning amendment and delivery

The CLI-first amendment was recorded before changes in `docs/CURRENT_TASK.md` at `d560be498e827d0b4f8560743dabc6661219542e`. It preserves the exact OFC1/OFC2 result sections and adds OFC3-OFC5 definitions, the CLI-first architecture decision and a routed continuation checkpoint. It changes no product code, source interfaces, tests, toolchain, workflow or installed runtime. New acceptance items above are requirements, not checks already passed.

The amendment's containing commit and PR checks identify its publication/verification. The handoff returned to the owner is an as-of projection of these owners, not another editable task board. Required next-agent source routes and current selection remain in the repository. Keep PR #1 unmerged and preserve `main` until integration is explicitly authorized.
