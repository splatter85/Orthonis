# Current task

Document ID: `orthonis.doc.current-task`.

## Selected work

```tokenslang-work
{
  "profile": "eutonos.tokenslang.work.v1",
  "work_id": "orthonis.work.ofc2",
  "goal": "Verify the second diagnostic module and strict manual discovery-plan round trip, then record the Windows read-only pilot handoff.",
  "status": "selected",
  "required": [
    {"scope_id": "orthonis.repo", "namespace": "orthonis.native", "resource_id": "orthonis.doc.agents"},
    {"scope_id": "orthonis.repo", "namespace": "orthonis.native", "resource_id": "orthonis.doc.architecture"},
    {"scope_id": "orthonis.repo", "namespace": "orthonis.native", "resource_id": "orthonis.doc.workflow"},
    {"scope_id": "orthonis.repo", "namespace": "orthonis.native", "resource_id": "orthonis.doc.health"}
  ]
}
```

The owner admitted OFC1 and OFC2 in order. [OFC Foundation](campaigns/OFC_FOUNDATION.md) owns definitions and the final result. Branch: `codex/ofc-foundation`; review PR #1. No automatic merge or PC deployment.

OFC1 source `88ac39fd9398b8a69089cf510e2dd24514681509` passed build, 24 executable regression checks, CLI start/report smoke, repository consistency and the repository regression suite on GitHub Ubuntu and Windows runners in run `34870171068`. The Linux log confirms SDK 10.0.401, zero compiler warnings/errors and 24/24 C# checks. This is fixture-based cross-platform execution, not real Windows collector acceptance.

OFC2 checkpoint: second module, plan validation, explicit approval, case revision/replay protection, and separate-process CLI smoke are implemented for the next build. Final OFC2 test outcomes are pending. Next: inspect the exact commit's runner results, correct work-attributable failures, update affected source routes and engineering documents, and retain precise Windows pilot limits.

No repairs, live model calls, real diagnostic data, PC inspection, or EUTONOS runtime installation are selected. Prior OED1 history remains in [Adoption evidence](EUTONOS_ADOPTION.md) and [OED1 history](../RAM/OED1.md).
