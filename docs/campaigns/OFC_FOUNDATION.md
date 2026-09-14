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

## Remaining Windows pilot and launch boundary

The next checkpoint is real read-only adapters behind `IStartupSource` and `IReliabilitySource`, tested on an explicitly accessible Windows host. First define event occurrence time, query coverage, reliable target identity/freshness, and live-data privacy boundaries. Preserve the fixtures and all core tests, then validate real enumeration, permissions, missing data and supported Windows conditions. Record the exact OS/build and results here or in a selected successor result.

No actual PC was scanned or repaired. The Windows runner executed synthetic code, not the owner's desktop workload. A desktop UI, installer, live model connection, real-data redactor, privileged helper, tested repair/recovery recipe, signing and distribution remain future work. A broad launch plan should follow the Windows pilot rather than treating this foundation as a finished utility.

Delivery: the source is published on `codex/ofc-foundation` in draft review PR #1. No merge or deployment is authorized by this result. Final documentation/catalog closeout is identified by its containing commit and corresponding PR checks; the pinned runs above retain the actually observed implementation evidence. Review the latest check result before integrating. Current Task retains the next concrete decision without reselecting completed OFC1/OFC2 work.
