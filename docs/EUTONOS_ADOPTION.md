# EUTONOS repository adoption

Document ID: `orthonis.doc.eutonos-adoption`.

## Identity, authority, and status

Repository: `splatter85/Orthonis`, GitHub repository ID `1370159187`, stable repository/scope ID `orthonis.repo`, namespace `orthonis.native`.

Deployment mode: **repository-files**. OED1 is the owner-selected initial deployment into the empty repository. No pre-existing branch, commit, native owner, or product source was present at inspection. The initial scope record was published as commit `2ee477d791a1b547488eddda931cc79d1a4c7883`.

Status: **repository-files deployment verified** at the OED1 boundary. The populated source was published on `main` at `c2242c52473aff53ae360a7662317b0dcff25252` and read back. No installed-runtime acceptance is claimed.

The owner authorized setup of EUTONOS first, not application implementation. The permitted plan is the 16-file set recorded in OED1's initial scope commit. The public destination receives project-specific instructions, public-safe provenance, and independently authored tooling, not copied private runtime source or diagnostic data. Visibility and licensing are not changed.

## Upstream provenance

Selected source: `splatter85/EUTONOS`, `main`, exact commit `4eeff6de668a664ce2ebbad249f8eeb8fb21f945`. Its current-task owner records the 0.5.3 delivery boundary. This is the inspected source revision, not an Orthonis-installed package version.

Inspected owners at that revision:

- `AGENTS.md` and `docs/CURRENT_TASK.md`: task/source authority and actual selection.
- `docs/DEPLOYMENT_STARTUP.md`: population, preservation, target verification, and installation limits.
- `docs/EXECUTION_ADAPTERS.md`: explicit repository-only operation, separate hosted execution, published records, and unavailable local state.
- `modules/docs/CONTRACT.md` and `docs/WORK_MODEL.md`: minimal populated owners, one board, organic growth, checkpoint and closeout duties.
- `modules/tokenslang/CONTRACT.md` and `tests/fixtures/read_repo/eutonos.read.json`: selected catalog/reference/work-block shapes.
- `docs/research/EHF5_CAMPAIGN_RESULT.md`: earlier synthetic connector proof, not consumer acceptance.

These upstream paths are provenance, not mandatory boot dependencies. Upstream access may be private; the public repository's useful rules and project facts are self-contained. No upstream files are modified by this deployment.

## Selected capabilities and explicit limits

| Capability | This deployment |
| --- | --- |
| Docs | Populated native owners, boot instructions, one live work owner, and public-safe source routes. |
| Stable references / TokenSlang | One `eutonos.read.json` catalog using the documented `catalog.v1` shape; selected work uses one `work.v1` block when a task is active. |
| NAV | Authored document routing in `NAV/README.md`; no native NAV runner, sweeps, semantic-code map, or ATLAS installation. |
| RAM-lite | Optional Markdown checkpoints under `RAM/`; not native RAM objects, capture APIs, or automated memory. |
| Repository checks | Independently authored standard-library tooling; not an upstream runtime certification. |
| Runtime / Hub / dashboard / coordinator | Not installed. No host admission, trust, locks, service, or local CLI claimed. |
| Delegation / MCP / hooks / usage tracking | Not configured; no paid calls, credential access, or inferred counters. |
| GitHub Actions / Windows application | Not created, dispatched, built, or tested in OED1. |

Do not add `eutonos.installation.json` or `eutonos.acceptance.json` by copying EUTONOS's own local certificates. This document owns the actual file-mode adoption state. A later runtime setup must inspect and preserve these owners and supply its own selected package, host configuration, target checks, and migration evidence.

## Verification record

Observed on 2026-09-14 in an isolated Linux development container with Python 3.13.5, using the authored staging files:

- `python tools/check_repository.py`: passed; 14 bindings, 12 Markdown owners, one selected OED1 work record.
- `python -m unittest discover -s tests -v`: 24 tests passed, including the symlink case; no skips.
- No external dependencies, network fetch, Windows utility, model API, or privileged operation was required by these checks.

Publication was completed through a nonforced `main` update to `c2242c52473aff53ae360a7662317b0dcff25252`. GitHub returned tree `fe7c8fc55bf0208cac3151adbecc0410bb1ad35a`, exactly matching the locally checked 16-file Git tree. A separate branch read confirmed that commit/tree, and separate exact-commit fetches of `AGENTS.md`, `docs/CURRENT_TASK.md`, this adoption owner, and `RAM/OED1.md` returned the expected text and blob identities.

`git diff --cached --check` passed in the isolated staging repository. The closeout version removes the completed selection rather than fabricating another task. The checker and all 24 regression tests were rerun successfully with the idle board. Git identifies this closeout record's own publication version; the source pin above is the verified deployment it describes.

These checks establish the published file-mode workflow, not a remote Windows build, native EUTONOS execution, or independent-agent understanding. The container could not clone GitHub directly because network name resolution was unavailable; source publication and readback used the connected GitHub tools. No unsupported credential workaround was used.

## Retained lessons and remaining boundaries

A GitHub repository connection can consume and publish source-backed project records without running the EUTONOS service. It cannot observe an unconnected PC, enforce local reservations, execute a Windows repair, or turn a Markdown instruction into a tool.

The catalog owns bindings; Current Task owns selection; source/tests own their actual behavior; RAM keeps history. An idle board has no selected work block. Native runtime orientation is not promised for idle prose, and no fabricated selected task is introduced to satisfy a parser.

Independent cold-agent comprehension and a Windows handoff remain unrun. No token/cost savings, secure attestation, native-runtime compatibility test, application acceptance, or consumer release is established by OED1. The proposed follow-up is to select the first read-only product-core slice, not begin it automatically.
