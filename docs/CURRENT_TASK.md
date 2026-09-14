# Current task

Document ID: `orthonis.doc.current-task`.

## Selected documentation work

```tokenslang-work
{
  "profile": "eutonos.tokenslang.work.v1",
  "work_id": "orthonis.work.ofc-cli-plan",
  "goal": "Revise the foundation campaign for CLI-first development and prepare an exact-source handoff for the next implementation slice.",
  "status": "selected",
  "required": [
    {"scope_id": "orthonis.repo", "namespace": "orthonis.native", "resource_id": "orthonis.doc.agents"},
    {"scope_id": "orthonis.repo", "namespace": "orthonis.native", "resource_id": "orthonis.doc.workflow"},
    {"scope_id": "orthonis.repo", "namespace": "orthonis.native", "resource_id": "orthonis.doc.architecture"},
    {"scope_id": "orthonis.repo", "namespace": "orthonis.native", "resource_id": "orthonis.doc.health"}
  ]
}
```

Authority: the owner requested continued command-line development until functionality is clearer, campaign amendments based on implemented work, and a detailed continuation prompt. This turn changes documentation and directly affected routes only. It does not start live collectors, UI, repairs, a model connection, or a merge.

Source baseline: `splatter85/Orthonis`, branch `codex/ofc-foundation`, commit `fa2b5dc567213043ac7871a43c278c598ff81446`; draft PR #1 remains open and unmerged. Preserve the checked OFC1/OFC2 implementation and OED1 history. [OFC campaign](campaigns/OFC_FOUNDATION.md) owns the slice definitions and historical evidence.

Allowed edits: the existing campaign, current task, architecture, project, foundation guide, and short agent-entry routing/policy clarification as needed. Preserve stable IDs; no new product code, SDK change, dependency, workflow, real machine evidence, or upstream EUTONOS edit.

Next action: reconcile the live-data prerequisites against actual source, define the next bounded CLI/Windows slice and later checkpoints, validate the documentation, publish a nonforced update on this branch, and return the source-pinned implementation handoff. A replacement session must recheck branch state before resuming.

## Retained foundation status

OFC1 and OFC2 reached their fixture-backed construction/execution boundaries. Their final head above passed GitHub run `34871838719` on Ubuntu and Windows runners; the earlier implementation runs remain in the campaign. A Windows runner executing fixtures is not proof of a real Windows collector. No PC was scanned or repaired.

The [foundation guide](FOUNDATION_GUIDE.md) owns existing CLI commands and source routes. Its current synthetic export path is not a real-data redactor. Event-time semantics, coverage, source identity, and target freshness need explicit treatment before live evidence is enabled.
