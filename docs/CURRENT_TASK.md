# Current task

Document ID: `orthonis.doc.current-task`.

## State: OFC5 selected

Owner request, 2026-09-16: execute OFC5 on this Windows PC and stop at the completed or honestly blocked pilot boundary. Source baseline: `60f78b283d145bb83889158f9c9e7b569dae19f3` on merged `main`. Work branch: `codex/ofc5-windows-pilot`. The installed SDK resolves the repository-pinned .NET `10.0.401`. No PC health conclusion is assumed before collection.

```tokenslang-work
{
  "profile": "eutonos.tokenslang.work.v1",
  "work_id": "orthonis.work.ofc5",
  "goal": "Run and verify the bounded cross-module Windows pilot on the selected owner PC, preserve private local evidence, and decide the live export policy without enabling uploads or model calls.",
  "status": "selected",
  "required": [
    {"scope_id":"orthonis.repo","namespace":"orthonis.native","resource_id":"orthonis.doc.ofc"},
    {"scope_id":"orthonis.repo","namespace":"orthonis.native","resource_id":"orthonis.doc.architecture"},
    {"scope_id":"orthonis.repo","namespace":"orthonis.native","resource_id":"orthonis.doc.workflow"},
    {"scope_id":"orthonis.repo","namespace":"orthonis.native","resource_id":"orthonis.doc.health"},
    {"scope_id":"orthonis.repo","namespace":"orthonis.native","resource_id":"orthonis.doc.foundation-guide"}
  ]
}
```

## Scope and acceptance

Follow [OFC5](campaigns/OFC_FOUNDATION.md#ofc5-cross-module-windows-pilot-and-privacyexport-decision). Build and run the retained regression lanes first. Use distinct ignored directories `.local/ofc5-startup` and `.local/ofc5-reliability`; do not place their cases, plans or raw output in Git. Exercise both explicit live sources, fresh-process reload/summary, local-plan preview and approved refresh where supported, replay refusal and persistence/history behavior. Record only bounded aggregate statuses and check outcomes in public documentation.

Validate source-drift and cancellation protections through the retained controlled regressions and safe live observations available without changing Windows state. Do not create or edit a Run registration, write/clear an event log or manufacture an owner-PC failure merely to trigger a guard. If a live cancellation or drift condition cannot be observed safely and repeatably, record that limitation rather than broadening effects.

Decide whether live export remains blocked or whether a separately scoped minimized projection is justified. The default remains fail-closed: no export implementation is admitted without an explicit field-level projection, preview and leakage tests. Completion requires actual commands/results, owner-PC versus hosted evidence kept distinct, repository checks, and a source-bound closeout with OFC5 removed from active selection.

## Exclusions and publication

CLI-first. No UI, installer work, repair, elevation for collection, registry/event-log writes or clears, target execution, dump access/upload, arbitrary command, model/provider call, paid service, live artifact publication, release, deployment or `main` merge. Preserve stable identities, synthetic/OFC3/OFC4 behavior and one task board. Publication, if warranted after review, is limited to public-safe source/documentation on the OFC5 branch using nonforced operations and exact readback.

## Checkpoint and restart

OFC1-OFC4 remain complete at their recorded boundaries and PR #1 is integrated into `main`. OFC5 is selected but no product build, owner-PC collector execution or private case creation has run in this slice yet. The two live case directories do not yet exist.

Next: build and run controlled verification, then create and exercise the separate Startup and Reliability cases. Stop before any live export implementation unless the evidence first justifies and separately bounds it.
