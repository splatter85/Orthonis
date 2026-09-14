# OED1 deployment checkpoint

Document ID: `orthonis.memory.oed1`.

Work: `orthonis.work.oed1`. Event date: 2026-09-14. Attribution: the repository-connected assistant performing the owner-requested setup; not a signed or independently verified agent identity.

## Observed setup boundary

The owner chose Orthonis and asked to deploy EUTONOS before application work. The repository was empty and public at discovery. OED1's scope was recorded in `docs/CURRENT_TASK.md` at commit `2ee477d791a1b547488eddda931cc79d1a4c7883` before substantive setup writes.

The selected upstream source is EUTONOS commit `4eeff6de668a664ce2ebbad249f8eeb8fb21f945`. Its repository-only adapter contract provides a basis for a file-based workflow, but does not turn published documentation into an installed runtime.

## Current observation

The populated files passed the repository checker and all 24 regression tests in the isolated Python 3.13.5 Linux environment. Publication/readback completion is not yet claimed in this checkpoint version. [Adoption evidence](../docs/EUTONOS_ADOPTION.md) owns the actual verification record, and [Current Task](../docs/CURRENT_TASK.md) owns live selection.

## Durable boundary

Keep the manual report/proposal AI loop and controlled-repair requirements in the [Architecture owner](../docs/ARCHITECTURE.md). Do not implement the Windows app during OED1. No model access, runtime installation, private credentials, Windows acceptance, native NAV/RAM execution, or next product slice is authorized merely by this checkpoint.

Independent fresh-agent pickup and the Windows handoff remain unrun. The useful next step after verified deployment is an owner-selected read-only product-core slice.
