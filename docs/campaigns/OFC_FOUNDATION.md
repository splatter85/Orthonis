# OFC: Orthonis Foundation Campaign

Document ID: `orthonis.doc.ofc`.

## Outcome and admission

Prove a small modular application with one case/evidence core, built-in diagnostic modules, and a controlled manual AI discovery loop. The owner approved the preceding recommendation and requested implementation on 2026-09-14. OFC1 and OFC2 are admitted in order in this session. The Windows pilot is a later checkpoint requiring access to the actual Windows host; it must not be simulated as completed.

Baseline: `splatter85/Orthonis/main` at `561affc9197ad6e704d074ed4b022448d542c446`. Implementation branch: `codex/ofc-foundation`. Preserve the existing EUTONOS file-mode owners and OED1 historical evidence. Current Task alone selects the live slice. This campaign owns definitions and results, not a parallel queue.

Scope: application source and executable regression tests, pinned build configuration, a narrow GitHub build workflow, this campaign and directly affected project/architecture/workflow/health/NAV/catalog documents. Publish coherent commits on the task branch and open a review PR; do not merge automatically. No upstream EUTONOS modifications, machine repairs, live system collectors, model calls, credential changes, telemetry, paid runners, artifact/cache uploads, external plugins, or account changes. CI is confined to standard runners on this public repository, with read-only repository permission and no secrets.

## Required reading

Read the repository AGENTS, Current Task, [Architecture](../ARCHITECTURE.md), [Workflow](../WORKFLOW.md), and [Project Health](../PROJECT_HEALTH.md). Use [Project](../PROJECT.md) for intended capabilities. Source files and tests become the implementation evidence; diagrams, fixtures, and successful parsing are not Windows acceptance.

## OFC1: One complete read-only path

Outcome: a buildable C#/.NET command-line application creates a synthetic diagnostic case, uses a fixture-backed Startup module through a small interface, records typed evidence and findings, persists a versioned case, and exports an understandable report. Actual Windows enumeration is excluded. Select and verify the build toolchain before claiming this executable works.

Acceptance: a successful build; automated normal, missing-information, permission-denied and invalid-data checks; a demonstrated start/report command sequence; versioned evidence with distinct collection outcomes; no automatic startup changes. Keep the core independent of platform APIs, UI, provider SDKs, and filesystem storage. Built-in composition is sufficient; no dynamic plugin loader.

## OFC2: Second module and manual AI round trip

Depends on OFC1's checked core. Outcome: add a fixture-backed Reliability module without rewriting Startup or the core contract. Export the actual discovery capability catalog and a return-plan example. Validate a complete imported discovery plan before calls; reject unknown operations/fields, malformed or duplicate-key JSON, excessive input, stale case/revision/hash, invalid targets, and replay. Execution of an accepted discovery plan requires explicit local approval. Save a new case revision and export follow-up evidence.

Acceptance: build and executable regression suite; two-module CLI demonstration; negative tests proving invalid plans make zero collector calls and leave persisted case bytes unchanged; evidence reference integrity; denied/failed collection is not healthy; interrupted writes retain a readable prior case; no arbitrary commands, repairs, live model access, or unsupported privacy claims. The report is synthetic-only in this milestone, not a tested real-data redactor.

## Windows pilot checkpoint

After the foundation is checked, select real read-only Startup and Reliability adapters and validate their outcomes on an actual Windows machine. Record OS/build, permissions, collector behavior, unavailable fields, and fixture/live differences. First pilot performs no repairs. This checkpoint does not authorize access to a machine unavailable to this session.

## Execution checkpoint

OFC1 starting. The Linux container has Python 3.13.5 and Git but no .NET SDK; direct GitHub clone and Microsoft SDK download failed in this environment. Do not mark the build blocked without attempting the selected GitHub runner route. No product tests have run yet.

Next: implement the small portable core, Startup module, CLI, regression harness, and bounded build workflow. Then observe actual CI before advancing the completion claim. Continue into OFC2 only after resolving work-attributable build/test failures. Keep unrun Windows pilot explicit.
