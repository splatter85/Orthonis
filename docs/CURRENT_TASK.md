# Current task

Document ID: `orthonis.doc.current-task`.

## Foundation implementation complete; review and Windows pilot remain

OFC1 and OFC2 reached their fixture-backed construction and execution boundaries on `codex/ofc-foundation`, in draft review PR #1. [OFC campaign](campaigns/OFC_FOUNDATION.md) owns slice definitions, exact commits/runs and remaining limits. OFC2 source `6b5af33c0d1150c9f7fb7acc5bc98c265d634cc9` passed the expanded C# suite, CLI round trip and repository checks on GitHub Ubuntu and Windows runners in run `34871192918`.

There is no active `tokenslang-work` block: the two admitted implementation slices are complete, not an implicit assignment to build repairs or perform the Windows pilot. The final documentation/catalog update has its own containing commit and PR check status; inspect that status before integration. Product work is published on the task branch, not merged into main or deployed to a PC.

## Next concrete work

Review PR #1 and select the actual Windows read-only collector checkpoint. The [foundation guide](FOUNDATION_GUIDE.md) provides build/CLI commands, source interfaces, and the local handoff. A receiving session must inspect its actual branch/revision and dirty state, run the fixture checks, then implement only its selected live adapters. Real-data export controls, event-time semantics and target freshness need explicit treatment before enabling live reports.

No PC access is available through repository text. No live Windows collector, repair executor, UI, installer, AI provider, real-data redactor, or native EUTONOS runtime has been implemented. Do not infer those capabilities from a green Windows-runner fixture test.

Record the next substantive selection here before execution, following [Workflow](WORKFLOW.md). Preserve OED1 historical adoption evidence and stable source IDs; no separate PC-only board is needed.
