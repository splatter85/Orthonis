# Orthonis project

Document ID: `orthonis.doc.project`.

## Purpose and initial users

Help a Windows PC owner understand a problem, investigate relevant evidence, make a controlled repair, and determine whether it helped. The initial pilot is owner-operated; a broader consumer release remains future scope.

Orthonis is a diagnostics, maintenance, and evidence-based repair project rather than a registry cleaner or generic speed booster. Success is a useful supported diagnosis, fewer unnecessary changes, and honest uncertainty, not the number of issues reported.

## Present capabilities

The OFC foundation implements a C#/.NET 10 command-line application with a portable case/evidence core, built-in fixture-backed Startup and Reliability modules, bounded strict JSON, local versioned case storage, Markdown reports, and a manual discovery-plan preview/approval/follow-up loop. [Foundation guide](FOUNDATION_GUIDE.md) owns usage; [OFC campaign](campaigns/OFC_FOUNDATION.md) owns the actual execution evidence. The code is published on the task branch in review PR #1, not a released application.

All collection currently uses synthetic scenarios. There is no live Windows collector, desktop UI, repair executor, installer, tested real-data redactor, or model-provider integration. The local case files are real; the system observations are deliberately simulated. [EUTONOS adoption](EUTONOS_ADOPTION.md) retains the historical development-workflow setup, and [Current Task](CURRENT_TASK.md) selects live work.

## Intended product capabilities

| Area | Intended outcome |
| --- | --- |
| Maintain | Storage analysis, carefully scoped cleanup, startup review, update visibility, and supported Windows integrity checks. |
| Investigate | Symptom-led cases, snapshots, changes over time, bounded performance capture, reliability events, and evidence-based conflict hypotheses. |
| Repair and verify | Supported actions with prerequisites, clear approvals, execution records, operation-specific recovery, and comparison after a change. |
| Explain | Plain-language findings separating observed facts, hypotheses, unavailable checks, and recommended measurements. |

Storage growth, Windows integrity, crash analysis, driver history, and software conflicts describe future scope. Fixture findings are not a diagnosis of a current PC. A missing registry or executable reference alone does not justify deletion or establish performance impact.

## Optional AI

The first implemented exchange mechanism exports a report with available capabilities and accepts a structured discovery proposal after local validation and approval. The transport is manual copying of text/JSON, not a live agent or an API connection. A deterministic example and smoke test prove the protocol path; they do not establish that a real model chose a useful diagnosis.

Live Codex, API-provider, direct-chat-tool, and local-model connections remain optional future adapters. Their authentication, current provider policies, availability, costs, and tool permissions must be verified at implementation. Do not promise unlimited/free usage or extract browser cookies/private authentication stores.

The product should remain useful without AI. Losing model access should pause AI assistance rather than incur a hidden charge or bypass local controls. Development-time EUTONOS and product-time diagnostic AI remain separate.

## Development direction

Use the small built-in modular application described in [Architecture](ARCHITECTURE.md). The selected foundation toolchain is recorded in `global.json` and the build properties. The Windows desktop UI toolkit and supported consumer Windows versions remain undecided. Add real collectors behind source interfaces, then verify on Windows before enabling real-data exports or repairs.

The first foundation milestone proves the two-module case/report/discovery loop. The next checkpoint is real Windows read-only collection. A broad launch campaign should be refined from that evidence rather than treating this entire intended capability list as admitted implementation.

## Success measures and open decisions

Evaluate diagnostic correctness, false positives, evidence quality, unnecessary changes avoided, meaningful verification, recovery, collection overhead, and measured AI usage where available. A healthy computer may need no repair. Exact performance attribution must come from measurements.

Distribution/signing, software licensing, desktop UX, supported Windows versions, real-data export policy, and privileged-operation threat modeling remain open. Accounts, cloud storage, telemetry, monetization, and an elevated agent are not foundation requirements.
