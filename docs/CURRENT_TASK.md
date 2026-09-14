# Current task

Document ID: `orthonis.doc.current-task`.

## Next implementation slice: OFC3

The owner requested continued command-line development until the app's functionality has a clearer shape, a campaign revision, and a detailed continuation handoff. The documentation amendment is complete in this source version. No OFC3 implementation or real Windows scan was performed by that amendment.

Next slice: [OFC3: live-data-aware CLI and bounded Windows Startup collection](campaigns/OFC_FOUNDATION.md#ofc3-live-data-aware-cli-and-bounded-windows-startup-collection). Status: prepared for the receiving implementation assignment, not yet started. When assigned the handoff, record `orthonis.work.ofc3` here with its scope and restart point before substantive execution. There is currently no selected `tokenslang-work` block; completed planning is not an active implementation grant. OFC4 and OFC5 are planned follow-ons only.

## Source and delivery checkpoint

Repository: `splatter85/Orthonis`. Continue on `codex/ofc-foundation` and existing draft PR #1; do not start from documentation-only `main` or merge automatically. The implementation baseline is `fa2b5dc567213043ac7871a43c278c598ff81446`, with both final runner jobs successful in run `34871838719`. This amendment's containing Git commit identifies the updated documentation. Re-resolve the actual remote head and preserve concurrent work at pickup; a historical pin is not reset authority.

OFC1/OFC2 are completed at their fixture-backed construction/execution boundaries: 53 C# behavioral checks at that baseline, the 24-test Python repository suite, and separate-process CLI round-trip smoke. Exact historical evidence remains in the [campaign](campaigns/OFC_FOUNDATION.md). No Windows adapter, owner-PC acceptance, repair, UI, live AI integration or tested live-data redaction is implied.

## Receiving action and constraints

Start with [AGENTS](../AGENTS.md), [Workflow](WORKFLOW.md), the [CLI-first Architecture decision](ARCHITECTURE.md#cli-first-development-decision), [Project Health](PROJECT_HEALTH.md), the OFC3 campaign section and [Foundation guide](FOUNDATION_GUIDE.md#next-windows-checkpoint-ofc3). Inspect current code, run the baseline checks available on that host, then build the bounded Startup slice rather than just another framework scaffold.

Keep the fixture loop working. OFC3 must address live source identity, coverage, unknown enablement, source-aware findings/output, persistent opaque targets and drift checks before live collection is exposed. Keep live AI/export payloads blocked; no repairs, autorun writes, target execution, elevation, cloud upload or UI work. Complete feasible code/test work without inventing unavailable Windows execution, then stop at OFC3's actual evidence boundary and leave later slices unselected.

Privacy and platform limitations are prerequisites for affected functionality, not permission to replace missing live evidence with fixtures or call a partial scan healthy. Return all progress to these native owners; preserve OED1 history, the source catalog and one task board.
