# OED1 deployment checkpoint

Document ID: `orthonis.memory.oed1`.

Work: `orthonis.work.oed1`. Event date: 2026-09-14. Attribution: the repository-connected assistant performing the owner-requested setup; not a signed or independently verified agent identity.

## Observed setup boundary

The owner chose Orthonis and asked to deploy EUTONOS before application work. The repository was empty and public at discovery. OED1's scope was recorded in `docs/CURRENT_TASK.md` at commit `2ee477d791a1b547488eddda931cc79d1a4c7883` before substantive setup writes.

The selected upstream source is EUTONOS commit `4eeff6de668a664ce2ebbad249f8eeb8fb21f945`. Its repository-only adapter contract provides a basis for a file-based workflow, but does not turn published documentation into an installed runtime.

## Verified deployment observation

The populated 16-file source was published on `main` at `c2242c52473aff53ae360a7662317b0dcff25252`. Its tree `fe7c8fc55bf0208cac3151adbecc0410bb1ad35a` matched the tested staging tree exactly. The repository checker and all 24 regression tests passed in the isolated Python 3.13.5 Linux environment. Separate connector reads recovered the published entry, selected work, adoption evidence, and this checkpoint's earlier version.

Source routes at that commit: `AGENTS.md` for boot constraints; `eutonos.read.json` for identity bindings; `docs/CURRENT_TASK.md` for the completed deployment's original selection; `docs/EUTONOS_ADOPTION.md` for provenance and initial verification; `tools/check_repository.py` and `tests/test_repository.py` for the checks. This is same-agent structural/readback evidence, not independent comprehension or a native RAM capture receipt.

[Adoption evidence](../docs/EUTONOS_ADOPTION.md) records closeout verification. [Current Task](../docs/CURRENT_TASK.md) is now idle and owns any future selection. This checkpoint preserves the event and exact historical source; it must not keep OED1 active or select product implementation.

## Durable boundary

Keep the manual report/proposal AI loop and controlled-repair requirements in the [Architecture owner](../docs/ARCHITECTURE.md). Do not implement the Windows app during OED1. No model access, runtime installation, private credentials, Windows acceptance, native NAV/RAM execution, or next product slice is authorized merely by this checkpoint.

Independent fresh-agent pickup and the Windows handoff remain unrun. The useful next step after verified deployment is an owner-selected read-only product-core slice.
