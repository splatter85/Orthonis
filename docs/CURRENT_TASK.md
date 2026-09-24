# Current task

Document ID: `orthonis.doc.current-task`.

## State: OFC5 publication selected

Owner direction on 2026-09-24 selected the proposed OFC5 publication sequence, followed by a separately recorded EXP-002 evaluation. The clean local `codex/ofc5-windows-pilot` head is `ecbf8483848a95f8b4ccd89a580843bf72a611d9`; after fetch, `origin/main` remains `60f78b283d145bb83889158f9c9e7b569dae19f3` and is an ancestor of the branch. The OFC5 Windows pilot is complete at its bounded owner-PC boundary; product source is unchanged from that merged foundation baseline.

```tokenslang-work
{
  "profile": "eutonos.tokenslang.work.v1",
  "work_id": "orthonis.work.ofc5-publication",
  "goal": "Publish the reviewed public-safe OFC5 documentation through a checked pull request and verify the merge into main.",
  "status": "selected",
  "required": [
    {"scope_id":"orthonis.repo","namespace":"orthonis.native","resource_id":"orthonis.doc.workflow"},
    {"scope_id":"orthonis.repo","namespace":"orthonis.native","resource_id":"orthonis.doc.health"},
    {"scope_id":"orthonis.repo","namespace":"orthonis.native","resource_id":"orthonis.doc.project"},
    {"scope_id":"orthonis.repo","namespace":"orthonis.native","resource_id":"orthonis.doc.ofc"}
  ]
}
```

Scope: correct the stale foundation PR status in Project, review the entire OFC5 public diff for private-data leakage, rerun repository checks, then push the branch without force. Open a PR to `main`, verify Windows and Ubuntu CI at its final head, merge with an expected head, and read back the remote result. Completion requires a clean checked tree, actual PR/CI/merge identities, and a compact closeout that removes this selection. No release, deployment, live-case upload, broader collector, UI or repair is in scope. Restart at the branch and remote readback if publication is interrupted.

## Completed boundary and evidence

The selected host reported Windows 25H2 build `26200.9457`, x64, with the repository-pinned .NET SDK `10.0.401`. `dotnet build Orthonis.slnx --configuration Release` passed with zero warnings and zero errors. The executable harness reported 53 foundation checks, 110 cumulative foundation/OFC3 checks and 68 OFC4 controlled checks passed. The synthetic CLI smoke passed. Native OFC3 and OFC4 smoke lanes passed on this PC without Run/event-log writes, target execution or live artifact upload.

Two distinct ignored private cases were created. Startup observed complete bounded reads for 17 current-user and four local-machine Run registrations, retained 21 opaque target rows and produced no attention finding. One approved opaque target inspection revalidated the registration and returned `Unsupported` rather than guessing or executing it. Reliability remained honestly `Limited`/`Partial`: it examined 65 records, retained 64, recorded six unknown envelopes and zero unparsed records. A second bounded read advanced the case while cross-query deduplication kept 64 distinct retained event records; these are not incident or crash counts.

Both local plans previewed with exit 0 and no byte change, then approved with exit 0 and advanced their separate cases from revision 1 to revision 2 with one applied-plan ID each. Replaying either plan was refused with exit 2 and preserved case bytes. Fresh-process reloads preserved both revision-2 cases. Controlled source-drift, timeout and cancellation regressions passed; no Windows state was changed merely to manufacture a live drift or cancellation condition, so an actual owner-PC drift/cancellation event remains unobserved.

## Privacy/export decision

Live export remains blocked. `report` and `example-plan` were each refused with exit 2 for both live cases. The pilot exposed incomplete Reliability coverage and local summaries that remain working views rather than reviewed minimized exports; no field-level projection or leakage-test suite was selected. No model/provider call, upload, dump access, raw event message, provider string, registry command, path or live case artifact was published.

The private case and plan files remain under ignored `.local/` paths on this PC. They are local working evidence, not a health certificate or public handoff. Empty, complete-looking or finding-free bounded observations do not establish PC health.

## Delivery and restart

The OFC5 branch contains documentation-only selection/closeout changes; product code did not change. Before any publication, rerun repository checks, review the public diff for private-data leakage and use a normal nonforced branch/PR flow with exact readback. Any future UI, minimized live export, broader collector, repair, release or deployment is unselected and requires its own scope.
