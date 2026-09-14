# Current task

Document ID: `orthonis.doc.current-task`.

## State: idle after OFC3

OFC3 (`orthonis.work.ofc3`) is complete at its selected repository and hosted-Windows boundary. There is no selected implementation slice now. OFC4 and OFC5 remain planned campaign entries only; neither may begin without a new owner selection here.

The continuation baseline was `9c769a676836184b09fa5881cc56910bb1120a7a` on `codex/ofc-foundation`, with `fa2b5dc567213043ac7871a43c278c598ff81446` as the prior verified implementation baseline. The first OFC3 implementation was published as `dedf2d16031bb464c917af3e9d1aa8da445cc02f`. Review then tightened `System.Security.SecurityException` handling so live permission failures retain the explicit `PermissionDenied` outcome in `15cb3184325425c5e438ee73ab358377f81a2364`.

## Completed boundary

OFC3 adds an explicit `start-windows` path for the current user's and local machine's `Software\Microsoft\Windows\CurrentVersion\Run` keys in one native registry view. It records schema-2 source context and bounded coverage, persists opaque case-scoped targets, revalidates the original registration before and after conservative local executable inspection, preserves drift/removal as stale or unavailable, and never executes a target. RunOnce, Startup folders, Task Scheduler, services, drivers, shell extensions, alternate registry views, repairs, elevation and Reliability live collection remain outside this slice.

Live case files are private local working data. Default live output is a bounded summary using opaque IDs and coverage outcomes; `report` and `example-plan` reject live cases. `local-plan` remains a deliberately local/private discovery-plan path with preview and explicit approval. Synthetic Startup/Reliability reports and their manual AI return loop remain unchanged and exportable.

The authoring environment did not provide a local .NET SDK, so no local C# build is claimed. GitHub Actions is the execution owner for the published C# result. Run `34878077559` passed the first OFC3 implementation on `ubuntu-24.04` and `windows-2022`, including the no-dump Windows Run-key read. Permission-outcome follow-up run `34885157156` also passed both jobs (`104113872222` Windows and `104113872533` Ubuntu), including build, the expanded executable suite, foundation and OFC3 smokes, repository consistency and Python regressions. Owner-PC acceptance has not been run and is not substituted by a hosted runner.

## Restart point

Begin future work by reading [OFC](campaigns/OFC_FOUNDATION.md), [Architecture](ARCHITECTURE.md), [Workflow](WORKFLOW.md), [Project Health](PROJECT_HEALTH.md) and [Foundation Guide](FOUNDATION_GUIDE.md), then write exactly one selected `tokenslang-work` block here before implementation. A reasonable next candidate is OFC4, bounded Windows Reliability collection, but it is not selected by this checkpoint. Do not merge PR #1, change `main`, start UI work, enable repairs or open live AI/export until separately authorized.
