# Current task

Document ID: `orthonis.doc.current-task`.

## Selected work

```tokenslang-work
{
  "profile": "eutonos.tokenslang.work.v1",
  "work_id": "orthonis.work.oed1",
  "goal": "Deploy the bounded GitHub-first EUTONOS repository workflow into Orthonis and verify its published source without implementing the Windows application.",
  "status": "selected",
  "required": [
    {"scope_id": "orthonis.repo", "namespace": "orthonis.native", "resource_id": "orthonis.doc.agents"},
    {"scope_id": "orthonis.repo", "namespace": "orthonis.native", "resource_id": "orthonis.doc.workflow"},
    {"scope_id": "orthonis.repo", "namespace": "orthonis.native", "resource_id": "orthonis.doc.eutonos-adoption"},
    {"scope_id": "orthonis.repo", "namespace": "orthonis.native", "resource_id": "orthonis.doc.health"}
  ],
  "optional": [
    {"target": {"scope_id": "orthonis.repo", "namespace": "orthonis.native", "resource_id": "orthonis.doc.project"}, "why": "Product context when populating or reviewing project owners."},
    {"target": {"scope_id": "orthonis.repo", "namespace": "orthonis.native", "resource_id": "orthonis.doc.architecture"}, "why": "Future product safety boundaries, not an implementation assignment."}
  ]
}
```

## OED1 checkpoint

Progress: populated adoption set prepared; verification and exact publication readback remain the closeout boundary. Authority is the owner's request to deploy EUTONOS into the new Orthonis repository first. The initial 16-path scope and exclusions are retained in commit `2ee477d791a1b547488eddda931cc79d1a4c7883`, `docs/CURRENT_TASK.md`.

Source selection: Orthonis `main`, initially empty; EUTONOS pinned at `4eeff6de668a664ce2ebbad249f8eeb8fb21f945`. Recheck current remote state before any write and preserve concurrent work. No application code, runtime installation, paid model call, Actions dispatch, or upstream modification is selected.

Next action: run the repository checks and negative tests against the proposed files, publish using the inspected parent/nonforced update, compare the exact tree to tested bytes, and read back the boot/task/adoption/checkpoint owners. Record results in [Adoption evidence](EUTONOS_ADOPTION.md), then remove this selection and retain the [historical checkpoint](../RAM/OED1.md).

After OED1, selecting the first read-only product-core slice is a proposed next decision, not admitted work.
