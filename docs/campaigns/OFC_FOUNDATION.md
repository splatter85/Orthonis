# OFC: Orthonis Foundation Campaign

Document ID: `orthonis.doc.ofc`.

## Outcome and admission

Prove a small modular application with one case/evidence core, built-in diagnostic modules and a controlled manual AI discovery loop, then establish one narrow real Windows read-only source without leaking live evidence through the unrestricted synthetic exporter. The owner approved the campaign on 2026-09-14 and later directed CLI-first continuation.

Baseline: `splatter85/Orthonis/main` at `561affc9197ad6e704d074ed4b022448d542c446`. Implementation branch: `codex/ofc-foundation`, draft review PR #1. Campaign admission was committed as `36d8e14dbca6e238a5b014700e1ba104ba8fe0b9`. Main remains the initial EUTONOS setup rather than the application foundation. Current Task alone selects live work; this file owns slice definitions/results.

OFC1 and OFC2 are complete at their synthetic foundation boundaries. OFC3 is complete at the repository plus hosted `windows-2022` bounded Startup-read boundary. Owner-PC acceptance, live Reliability, general live-data export, UI and repairs remain uncompleted work.

## Required reading

Start at [Current Task](../CURRENT_TASK.md), [Architecture](../ARCHITECTURE.md), [Workflow](../WORKFLOW.md), [Project Health](../PROJECT_HEALTH.md), and [Foundation Guide](../FOUNDATION_GUIDE.md). Source/tests and actual CI jobs are evidence owners; prose alone is not execution proof.

## OFC1: One complete read-only path

Outcome: a buildable C#/.NET CLI creates a synthetic case, uses a fixture-backed Startup module, records typed evidence/findings, saves a versioned case and exports a report. Windows enumeration was excluded.

Result: implemented at `88ac39fd9398b8a69089cf510e2dd24514681509`, tree `db0e2711fb68601367091e432d73f27f96addd3c`. GitHub run `34870171068` passed Ubuntu and Windows runners with 24 C# checks, CLI start/report, repository consistency and Python regressions. This was fixture execution, not a Windows startup scan.

## OFC2: Second module and manual discovery round trip

Outcome: add synthetic Reliability while preserving Startup; export actual capabilities; validate a complete discovery proposal before calls; require local approval; persist next revision/applied-plan ID; reject malformed, wrong, stale and replayed plans.

Result: implemented at `6b5af33c0d1150c9f7fb7acc5bc98c265d634cc9`, tree `f486d1a75379b84983bc40446d39eb9b0165b5ca`. Run `34871192918` passed jobs `104067202156` and `104067201731`. Its checked PR merge view `684cff21ff1c7ad7a20177ed103c8557ae8633a8` had the same implementation tree; that test merge did not merge PR #1 into main.

The expanded foundation harness contained 53 passing C# checks and the Python repository suite contained 24 tests. Separate-process smoke established preview/no mutation, invalid-plan/no mutation, explicit approval, persisted follow-up, replay refusal and fresh-process reload. This proved the protocol path, not model diagnostic quality.

The final OFC1/OFC2 source baseline was `fa2b5dc567213043ac7871a43c278c598ff81446`; run `34871838719` passed both runners. The CLI-first documentation amendment `9c769a676836184b09fa5881cc56910bb1120a7a` was checked by run `34873536758` and selected OFC3 as the next implementation slice without changing product source.

## CLI-first continuation plan

Owner direction, 2026-09-14: stay command-line-first until real functionality makes a future UI clearer. The CLI is a host, not the owner of collection, analysis, privacy or authorization. UI, repairs, installer/signing, provider integration and arbitrary command execution stay outside OFC3-OFC5.

| Slice | Outcome | Current boundary |
| --- | --- | --- |
| OFC1 | Fixture-backed Startup case/report path | Completed at recorded synthetic boundary. |
| OFC2 | Reliability module and validated manual discovery loop | Completed at recorded synthetic boundary. |
| OFC3 | Live-data-aware CLI and bounded Windows Startup collection | Completed at repository and hosted Windows-runner boundary; owner-PC acceptance unrun. |
| OFC4 | Bounded Windows Reliability collection in the same case system | Planned only; not selected. |
| OFC5 | Cross-module Windows pilot and privacy/export decision | Planned only; not selected. |

## Source findings that shaped OFC3

At the continuation baseline, `Program.CreateEngine` accepted only synthetic source labels and every start/report/apply path used the unrestricted synthetic renderer. Cases lacked explicit source/coverage semantics, fixture Startup forced Boolean enablement, live locators did not exist, and target admission relied on historical evidence. OFC3 therefore required a versioned live case boundary rather than swapping a Windows source into the fixture path.

## OFC3: Live-data-aware CLI and bounded Windows Startup collection

Work identity: `orthonis.work.ofc3`.

### Outcome and implementation

OFC3 adds an explicit `start-windows` mode that never falls back to fixtures. It creates schema-2 `windows:startup` cases with validated native-view source context and bounded query coverage. The Windows adapter reads only current-user and local-machine `Software\Microsoft\Windows\CurrentVersion\Run`, retaining at most 32 values per key. It records `Observed`, `Empty`, `PermissionDenied`, `Failed`, `TimedOut`, `Limited`, `Unsupported`, `Unavailable` and `Stale` distinctions rather than treating incomplete reads as healthy.

Live target IDs are opaque and case-scoped. Private locators bind hive, registry view, value identity and a registration-state hash to the case/source context. Targeted inspection rechecks machine/user/view, rereads the exact registration, resolves only a conservative absolute local-drive `.exe` subset, inspects file attributes without execution, then rereads the registration to catch drift during the filesystem observation. Changed, removed, substituted, ambiguous or wrong-scope targets are never silently remapped.

The file probe rejects UNC/device/search-path targets, nonstandard drive mappings and reparse traversal. Only explicit local file/path-not-found outcomes qualify as missing; permissions/errors remain inaccessible or unknown. RunOnce, Startup folders, Task Scheduler, services, drivers, shell extensions and the alternate registry view are excluded. Presence in Run is not proof of enablement, execution, delay, malware, health or a required repair.

Live evidence remains local. `summary` prints bounded coverage and opaque target outcomes but not raw value names, command lines, paths, source bindings or untrusted finding text. `report` and `example-plan` are blocked for schema-2 live cases at the Core export gate and CLI routes. `local-plan` is explicitly local/private and reuses whole-plan validation, preview and approval without enabling cloud sharing. Synthetic report/AI exchange remains unchanged.

The implementation introduced `Orthonis.Windows`, reusable `CaseOperations`, schema-2 source/coverage contracts, fixed version-1 compatibility fixtures, injected live-source regression seams, privacy sentinels and `tools/smoke_ofc3.py`. First implementation commit: `dedf2d16031bb464c917af3e9d1aa8da445cc02f`. Review found that `System.Security.SecurityException` could be flattened to generic failure in coordinator/inventory paths; `15cb3184325425c5e438ee73ab358377f81a2364` maps those cases explicitly to permission denied and adds focused regressions.

### Verification result

Run `34878077559` passed the first OFC3 implementation on both `ubuntu-24.04` and `windows-2022`. Windows job `104090177183` passed build, executable regressions, foundation CLI smoke, OFC3 controlled/fresh-process smoke, repository consistency and Python regressions. The OFC3 smoke performed a real bounded native HKCU/HKLM Run read and local existing/missing executable attribute probes on the Windows runner while logging only pass/fail and nonidentifying counts. Ubuntu job `104090177476` exercised the portable/control paths and correctly skipped native collection.

Permission-outcome follow-up run `34885157156` passed both hosts. Windows job `104113872222` and Ubuntu job `104113872533` each passed build, the expanded executable suite, foundation CLI smoke, OFC3 smoke, repository consistency and repository regressions; the Windows smoke again passed the bounded native Run read. The authoring environment had no `dotnet` executable, so no local C# build is claimed. No owner-PC live read was performed.

Acceptance therefore separates: portable source/fixture behavior checked by hosted builds/tests; real adapter execution checked on the hosted Windows runner; owner-PC acceptance unrun. No autorun entries were created, no registry writes or target execution occurred, no elevation/provider/model call was used, and no live case artifact was uploaded.

OFC3 stops here. OFC4, OFC5, UI, repairs and live AI/export remain unselected.

## OFC4: Bounded Windows Reliability collection

Planned only. Dependency: OFC3's checked live-data/source/privacy contract. Add a bounded Windows Application event-log source behind the Reliability boundary while retaining Startup behavior. Preserve event occurrence time, query interval, record/channel/provider identity, retention/clear gaps and partial/query-limit outcomes. Repeated scans must not inflate counts, missing/unknown schemas remain incomplete, and no-event/incomplete results do not certify health. Choose and verify the Windows API/package only when this slice is selected. No crash-dump upload or repair is included.

## OFC5: Cross-module Windows pilot and privacy/export decision

Planned only. Dependency: both live adapters. Exercise Startup and Reliability through the CLI on an explicitly selected Windows PC, validate persistence/source drift/local follow-up/cancellation/history, and decide whether live export remains blocked or gains a separately selected minimized projection with preview and leakage tests. No model call or upload is required to verify the protocol. A future UI decision remains separate.

## Windows reference routes

Primary guidance checked during OFC3 planning on 2026-09-14:

- [Run and RunOnce](https://learn.microsoft.com/en-us/windows/win32/setupapi/run-and-runonce-registry-keys): registrations are command lines; OFC3 intentionally includes only Run.
- [RegistryView](https://learn.microsoft.com/en-us/dotnet/api/microsoft.win32.registryview?view=net-10.0): registry view semantics must be explicit.
- [File.Exists](https://learn.microsoft.com/en-us/dotnet/api/system.io.file.exists?view=net-10.0): false can represent errors/permissions, one reason OFC3 uses classified native attribute reads instead.
- [EventLogQuery](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.eventing.reader.eventlogquery?view=windowsdesktop-10.0): an OFC4 investigation route, not a selected dependency.

Reverify relevant APIs at later implementation; these references do not expand authorized effects.

## Publication boundary

OFC3 source is published only to `codex/ofc-foundation` and draft PR #1. `main` stays unchanged until explicit integration authority. The containing closeout commit updates this campaign, Current Task, project/architecture/health/guide/NAV/catalog facts; its own PR CI must pass before the closeout is treated as checked. Preserve OED1 history and one task board.
