# Current task

Document ID: `orthonis.doc.current-task`.

## Selected implementation: OFC3

```tokenslang-work
{
  "profile": "eutonos.tokenslang.work.v1",
  "work_id": "orthonis.work.ofc3",
  "goal": "Implement the live-data-aware CLI and bounded read-only Windows Run-key Startup slice, preserving the synthetic foundation.",
  "status": "selected",
  "required": [
    {"scope_id":"orthonis.repo","namespace":"orthonis.native","resource_id":"orthonis.doc.ofc"},
    {"scope_id":"orthonis.repo","namespace":"orthonis.native","resource_id":"orthonis.doc.workflow"},
    {"scope_id":"orthonis.repo","namespace":"orthonis.native","resource_id":"orthonis.doc.architecture"},
    {"scope_id":"orthonis.repo","namespace":"orthonis.native","resource_id":"orthonis.doc.health"},
    {"scope_id":"orthonis.repo","namespace":"orthonis.native","resource_id":"orthonis.doc.foundation-guide"}
  ]
}
```

Owner assignment: execute [OFC3](campaigns/OFC_FOUNDATION.md#ofc3-live-data-aware-cli-and-bounded-windows-startup-collection) only, with source, tests, affected documentation and normal publication to `codex/ofc-foundation`, updating draft PR #1. No merge, deployment, UI, repair, registry writes, target execution, elevation, provider calls, live export, paid runners or account changes. OFC4 and OFC5 remain unselected.

## Baseline and scope

Resolved remote baseline on 2026-09-14: `9c769a676836184b09fa5881cc56910bb1120a7a`, matching the supplied handoff. Previous implementation baseline: `fa2b5dc567213043ac7871a43c278c598ff81446`. Main is not the application starting point. Preserve concurrent changes; use expected blob versions and nonforced publication.

Scope: explicit validated live source/coverage semantics; bounded current-user and local-machine Run-key collection with declared registry views; persistent opaque case-scoped targets and registration revalidation; conservative local file-presence inspection; gated summaries and blocked live AI export; focused compatibility, adverse-case, privacy and cross-process tests. Preserve immutable evidence, strict import, whole-batch validation, approval/replay, storage refusal boundaries and the synthetic Startup/Reliability loop.

Acceptance: actual full build, expanded executable C# suite, fixture CLI smoke and Python repository checks on available hosts; injected Windows-source tests; safe no-dump Windows adapter smoke if a Windows runner is available. Distinguish portable checks, Windows-runner reads and owner-PC acceptance. Record any missing evidence instead of substituting fixture success.

## Execution checkpoint and restart point

Status: source inspection and baseline verification in progress; implementation not yet published. Required owner documents and core/CLI/Startup paths have been read at the pinned baseline. This authoring container has no `dotnet` executable; `dotnet --info` returned command not found. Direct `git clone` failed DNS resolution, leaving no local Git checkout to inspect. GitHub connector reads/writes work. No claim about an unobserved owner-PC working tree is made. Use the existing bounded public PR workflow for actual C# execution; inspect its results, not just historical success prose.

Next action: finish relevant source/test/build inspection, verify the unchanged foundation through available CI, then implement the OFC3 boundary and adapter. Publish against the inspected parent and read back the exact result. Update this checkpoint and the campaign at meaningful boundaries and closeout.

## Preserved foundation history

OFC1/OFC2 are completed at their fixture-backed construction/execution boundaries: 53 C# checks at the implementation baseline, the 24-test Python repository suite, and separate-process CLI round-trip smoke. Historical results, exact source views and unrun limits remain in the [campaign](campaigns/OFC_FOUNDATION.md). Run `34871838719` checked the implementation baseline; run `34873536758` checked the later documentation amendment. Neither establishes live Windows Startup collection or owner-PC acceptance.

Keep EUTONOS file-based, preserve OED1 history and stable IDs, and use this as the sole task board. Private upstream EUTONOS access is not required.
