# Development and handoff workflow

Document ID: `orthonis.doc.workflow`.

## One work owner

[Current Task](CURRENT_TASK.md) alone selects work and its restart point. Before substantive execution, retain the request, scope, source baseline, exclusions, completion checks, and next action there or in one linked slice/campaign owner. A standalone slice is sufficient for a bounded result; routine wording corrections do not need a campaign.

Selected machine-readable work, when present, is one `tokenslang-work` JSON fence in Current Task with profile `eutonos.tokenslang.work.v1`. Its work ID, goal, selected status and references belong to that block; surrounding prose records progress and authority without creating another task record. With no selected work, omit the fence rather than inventing a task for a parser.

`eutonos.read.json` binds stable IDs to owners and paths, not execution priority. Roles may share an owner. Durable facts belong in their canonical owner, not copies across the README, task board, NAV and RAM.

## GitHub-facing work and publication

Resolve a branch to an exact commit before a coherent read. Inspect relevant source and required instructions. Published Markdown remains usable without a local EUTONOS CLI, dashboard, upstream private access, or delegation.

For authorized writes, preserve the selected tree and use an actual expected blob version or nonforced ref update. Bind a multi-file tree to the same inspected parent. Re-read changed remote state and reconcile conflicts instead of attaching old files to a newer parent. Uncertain tool outcomes require readback before retry.

OED1 was authorized on the empty repository's `main`. OFC product implementation is authorized on `codex/ofc-foundation` with review PR #1; it does not authorize automatic merge or live deployment. Later work should use a selected task branch unless the owner chooses otherwise. Respect actual protections and do not change visibility, license, account settings, or upstream EUTONOS as an incidental step.

## Application engineering practices

Keep Core independent of filesystem storage, Windows APIs, desktop controls and AI SDKs. The composition host wires known modules; it must not become the home of every collector and analyzer. Feature sources return typed observations, separate analysis functions interpret them, and shared case/report/approval mechanisms remain shared. A new module should not need changes to an unrelated existing module.

Use explicit interfaces at real boundaries, immutable snapshots for evidence, nullable annotations, structured results for expected failures, and cancellation for potentially long work. Do not expose a generic service locator, dynamic plugin loader, shell-string escape hatch, or giant optional-parameter interface to make future features seem implemented. Dependencies require an actual need and review; the current foundation uses only .NET libraries and clears external NuGet sources.

Reproduce a bug with the smallest relevant failing check, then fix and retain a regression. During implementation run focused checks; at a coherent foundation boundary run the [build, regression and CLI checks](PROJECT_HEALTH.md). Shared-contract/import/storage changes require negative tests for malformed input, wrong versions, stale state, authorization, partial outcomes and replay as applicable. Do not weaken tests, suppress warnings, or call a failure pre-existing without baseline evidence.

[Build properties](../Directory.Build.props) enable nullable checking, .NET analyzers, warnings as errors and deterministic output. [SDK selection](../global.json) supplies the reproducible baseline. These mechanisms enforce what they check, not every architectural rule in this prose. Review project references and API boundaries explicitly. No code-coverage percentage or security certification is inferred from a passing suite.

Keep data format changes explicit. Current case, detail, capability and plan version 1 semantics must not silently change meaning. An incompatible change needs a migration or an explicit unsupported outcome. Do not add generic backwards-compatibility parsing that quietly invents missing evidence or approval.

For asynchronous collection, distinguish user cancellation from timeout, denial and ordinary failure. Bound input, query scope and resource use. In-process timeout handling is cooperative, not isolation. For persistence, preserve prior valid state across the tested failure boundary and identify stronger durability or recovery claims that remain untested.

## Local handoff

The receiving Windows session reads AGENTS and Current Task at its actual branch/commit and inspects its own dirty state. Recover scope from the repo, not a private transcript. A remote checkpoint does not establish that the local checkout is clean or current.

Build and exercise the synthetic foundation first, then implement only the selected Windows adapters. [Foundation guide](FOUNDATION_GUIDE.md) describes interfaces and live-data prerequisites. Record commands, OS/build, permissions, results, unrun checks and blockers in the campaign. Publish reviewed changes normally and return facts to the same owners.

Do not create a PC-only task board. Keep raw diagnostic data, host paths, active reservations, credentials and runtime state outside this public repository. Real EUTONOS runtime adoption is separate work with its own package and target checks.

## Verification and closeout

A failed prerequisite blocks dependent work, not unrelated progress. Never claim an unrun build or scanner acceptance. Preserve the actual commit/runner and distinguish a PR test-merge from its head; compare trees before transferring test evidence. A Windows runner executing fixture code is not proof of a real Windows collector.

Before a substantive final, record the result, continuing/completed/blocked state, unrun limits and delivery disposition. Update only affected facts, routes and the compact task checkpoint. Completed implementation leaves the active selection; outstanding review/publication/Windows work remains explicit. Suggested follow-ups stay unselected until authorized.

[RAM checkpoints](../RAM/README.md) are optional history. Required campaign/task updates apply even without RAM capture. Reuse the campaign's source-backed result rather than creating duplicate status narratives. This file-only deployment claims no native capture receipts, host reservations, signer, token counters, or per-turn automation.

Start with the smallest useful document set and grow when actual source or reading burden requires it. Update authored NAV and stable bindings only where routes change. An EUTONOS upgrade must preserve native owners/IDs and record its own target evidence; never copy an unrelated installation certificate.
