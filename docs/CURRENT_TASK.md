# Current task

Document ID: `orthonis.doc.current-task`.

## Selected work

```tokenslang-work
{
  "profile": "eutonos.tokenslang.work.v1",
  "work_id": "orthonis.work.ofc1",
  "goal": "Build and verify the first fixture-backed read-only Orthonis investigation path, then continue to the admitted OFC2 manual discovery round trip.",
  "status": "selected",
  "required": [
    {"scope_id": "orthonis.repo", "namespace": "orthonis.native", "resource_id": "orthonis.doc.agents"},
    {"scope_id": "orthonis.repo", "namespace": "orthonis.native", "resource_id": "orthonis.doc.architecture"},
    {"scope_id": "orthonis.repo", "namespace": "orthonis.native", "resource_id": "orthonis.doc.workflow"},
    {"scope_id": "orthonis.repo", "namespace": "orthonis.native", "resource_id": "orthonis.doc.health"}
  ]
}
```

The owner approved the foundation recommendation and requested work. [OFC Foundation](campaigns/OFC_FOUNDATION.md) defines OFC1 and OFC2, both admitted in order, and the later Windows pilot. Working branch: `codex/ofc-foundation`, from `main` at `561affc9197ad6e704d074ed4b022448d542c446`. Preserve concurrent work and inspect the actual branch before resuming. Publication is to the task branch and a review PR, not an automatic merge or PC deployment.

Checkpoint: OFC1 starting. Build the portable C# core and Startup fixture path, a real CLI, and executable checks. Verify on selected standard GitHub runners because the current Linux container lacks an accessible .NET toolchain. Product build/test results are not yet known. Then complete OFC2 and record exact results before the Windows handoff.

No repairs, live model calls, private data, real PC inspection, or EUTONOS runtime installation are selected. Prior OED1 deployment history remains in [Adoption evidence](EUTONOS_ADOPTION.md) and [OED1 history](../RAM/OED1.md).
