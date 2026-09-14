# Development and handoff workflow

Document ID: `orthonis.doc.workflow`.

## One work owner

[Current Task](CURRENT_TASK.md) alone selects work and its restart point. For substantive execution, retain the request, scope, source baseline, exclusions, completion checks, and next concrete step there or in one linked slice/campaign owner. A small standalone slice is sufficient for a bounded result; routine wording corrections do not need a campaign.

The selected machine-readable work, when present, is one `tokenslang-work` JSON fence in Current Task with profile `eutonos.tokenslang.work.v1`. Its work ID, goal, selected status, and references belong to that block; surrounding prose describes progress and authority without creating another editable task record. When no work is selected, omit the fence and state that plainly. Do not create a fictitious selected task to make an orientation command succeed.

`eutonos.read.json` binds stable resource IDs to owners and paths. It is a catalog, not an execution queue. Document roles may share an owner. New durable facts go in their canonical owner, not in copies across the README, task board, NAV, and RAM.

## GitHub-facing work

Resolve the requested branch to an exact commit before a coherent read. Inspect only relevant source and required instructions. A repository reader may use the published Markdown directly without a local CLI, dashboard, private upstream access, or model delegation.

When writes are authorized, preserve the selected tree and use an actual expected blob version or a nonforced ref update. Bind a multi-file tree to the same inspected parent. Re-read changed remote state before publication and reconcile a conflict instead of attaching old files to a newer parent. An uncertain tool response requires readback before retry.

The initial OED1 setup is authorized on `main` because the owner asked to deploy into the empty repository. Subsequent application implementation should use a task branch based on current `main` unless the owner selects otherwise. Respect any actual repository protections. Deployment does not authorize changing visibility, license, upstream EUTONOS, or account settings.

## Local handoff

The receiving Windows session first reads AGENTS and Current Task at its actual branch/commit, then inspects its own working tree. Recover the selected scope and checks from the repository, not a transcript. A remote checkpoint does not establish that a local checkout is clean or current.

Implement and validate only the selected Windows work. Record commands, OS/build and important test conditions, actual results, unrun checks, and any blocker. Publish reviewed changes through normal Git operations. Return results to the same canonical owners and work record, preserving stable IDs.

Do not create a second PC-only task board. Keep host paths, active reservations, raw diagnostic evidence, credentials, and private runtime state outside the public repository. A fresh local EUTONOS runtime adoption is separate work with its own package and target checks.

## Verification and closeout

[Project Health](PROJECT_HEALTH.md) owns repeatable check commands. Run focused checks during work and relevant broader checks at a coherent boundary. A failed prerequisite blocks what depends on it, not unrelated work. Do not suppress failures, invent baseline success, or report a build that was not run.

Before a substantive final, record what changed, observed results, continuing/completed/blocked state, and delivery disposition. Update only affected facts, links/catalog entries, and the compact work checkpoint. Remove completed work from the active selection and link its historical result. A suggested next step stays unselected until authorized.

Published [RAM checkpoints](../RAM/README.md) are optional evidence of past work. Capture only useful public-safe facts and source routes. They do not complete a task, authorize a repair, replace the board, or contain hidden reasoning. In this file-only mode no runtime capture receipt, native signer, host reservation, token counter, or per-turn automation is claimed.

## Growing this deployment

Start small. Add a document when a real owner or reading burden warrants it, not to fill a template. Maintain the authored NAV when meaningful routes change. Do not rebuild a native source map that has not been installed.

When upgrading EUTONOS, inspect the selected upstream revision and actual consumer state. Preserve owner IDs and public checkpoints, review intended differences, and record target verification. Never overwrite the file-mode record with an unrelated upstream installation certificate. Missing upstream access does not prevent ordinary work from these self-contained instructions.
