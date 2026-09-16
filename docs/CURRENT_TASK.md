# Current task

Document ID: `orthonis.doc.current-task`.

## State: idle after OFC4

The owner-selected OFC4 slice is complete at its repository and hosted Windows-runner boundary. There is no selected work block. The checked foundation is ready for the owner-authorized integration of review PR #1; OFC5 is a suggested next slice, not an active assignment. Keep the CLI-first policy; do not start UI, repairs, live AI/export or owner-PC work automatically.

Repository: `splatter85/Orthonis`. Foundation branch: `codex/ofc-foundation`, review PR #1, final checked head `7f62d3574b3d9bfedda0bbc4626edab385125af0`. No release or deployment was performed. A connected branch view does not establish the state of an owner's local checkout.

## Completed boundary and evidence

OFC4 was selected on 2026-09-15 from OFC3 closeout `16a881c0a99eeeda4a69eee0ce7c208fc1dd1518`, recorded in `3d944bc0448077e4226d69280639ffb447d4a095`. Initial implementation `e20fd920df45ddb972af6159b402b2b1c87387e0` had two parser compile errors, subsequently fixed. Checked product/test source is `2848e7fd8c6f48d37559018c3d721283f0c617de`.

Run `34947701494` passed Windows job `104310923329` and Ubuntu job `104310923110` for the checked product/test source: build, retained and new executable regressions, foundation/OFC3/OFC4 smokes, repository consistency and Python regressions. The final documentation head then passed run `34948312201`: Windows job `104312920996` and Ubuntu job `104312920648` repeated those checks; Windows also passed real bounded Run and Application event-log reads without event/registry writes or live artifact upload. The authoring container had no .NET SDK; no local C# build or owner-PC acceptance is claimed. [OFC](campaigns/OFC_FOUNDATION.md#ofc4-bounded-windows-reliability-collection) owns the detailed result/history. [OFC4 Reliability](OFC4_RELIABILITY.md) owns the source contract; [Foundation Guide](FOUNDATION_GUIDE.md) and [Project Health](PROJECT_HEALTH.md) own commands and checks.

## Current product and constraints

The CLI supports synthetic reports/manual discovery plans, bounded live Windows Startup, and a separate opt-in live Windows Reliability source in the same case system. Reliability reads only local Application event metadata for a fixed seven-day, 64-record subset, preserves query/occurrence times and history gaps, and deduplicates event records without calling them crash counts. Source drift, unknown/malformed data, limits, errors and permissions remain explicit.

Live cases and local plans remain private. `report` and `example-plan` are blocked for both live modes. No System/Security channels, arbitrary query/target, dump access, event-log write/clear, target execution, repair, elevation, model call or upload was added. Synthetic and OFC3 behavior remain covered by the retained checks.

## Suggested continuation, not selected

A future explicit OFC5 request can select a Windows PC for cross-module persistence/source-drift/cancellation/history checks and the live export policy decision. Start from the latest verified branch head, inspect the local checkout's dirty state and preserve concurrent work. Keep Startup and Reliability in distinct local case directories and do not upload those cases. Do not infer owner-PC health or acceptance from hosted CI.
