# Orthonis project

Document ID: `orthonis.doc.project`.

## Purpose and initial users

Help a Windows PC owner understand a problem, investigate relevant evidence, make a controlled repair, and determine whether it helped. The initial pilot is owner-operated; a broader consumer release remains future scope.

Orthonis is a diagnostics, maintenance, and evidence-based repair project rather than a registry cleaner or generic speed booster. Success is a useful supported diagnosis, fewer unnecessary changes, and honest uncertainty, not the number of issues reported.

## Present capabilities

The OFC foundation is a C#/.NET 10 command-line application with a portable case/evidence core, built-in synthetic Startup and Reliability modules, bounded strict JSON, local versioned case storage, Markdown reports, and a manual discovery-plan preview/approval/follow-up loop. OFC3 also provides an explicit opt-in live Windows Startup source for a deliberately small subset: HKCU and HKLM `Software\Microsoft\Windows\CurrentVersion\Run` in the native registry view, bounded to 32 values per key.

Live Startup cases record source scope and query coverage, use opaque case-scoped target IDs, re-read the selected registration before inspecting it, and conservatively inspect only supported local executable paths without execution. Changed or removed registrations become stale/unavailable rather than being remapped. Enablement is unknown; a Run registration is not proof of execution, boot delay, malware, health or a required repair.

Live cases remain private local working data. The default live summary excludes raw registry names, command lines and private paths, and the synthetic `report`/`example-plan` export path is blocked for live cases. The local plan/approval loop can request supported follow-up reads without enabling cloud sharing. Synthetic reports remain available for the manual AI exchange.

There is still no desktop UI, repair executor, installer, live Reliability collector, tested minimized real-data AI exporter, or model-provider integration. The code is published on `codex/ofc-foundation` in draft PR #1, not a released or merged application. [Foundation Guide](FOUNDATION_GUIDE.md) owns commands and [OFC](campaigns/OFC_FOUNDATION.md) owns execution evidence.

## Intended product capabilities

| Area | Intended outcome |
| --- | --- |
| Maintain | Storage analysis, carefully scoped cleanup, startup review, update visibility, and supported Windows integrity checks. |
| Investigate | Symptom-led cases, snapshots, changes over time, bounded performance capture, reliability events, and evidence-based conflict hypotheses. |
| Repair and verify | Supported actions with prerequisites, clear approvals, execution records, operation-specific recovery, and comparison after a change. |
| Explain | Plain-language findings separating observed facts, hypotheses, unavailable checks, and recommended measurements. |

Storage growth, Windows integrity, crash analysis, driver history, and software conflicts describe future scope. A missing registry or executable reference alone does not justify deletion or establish performance impact.

## Optional AI

The implemented exchange mechanism for synthetic cases exports a report with available capabilities and accepts a structured discovery proposal after local validation and approval. The transport is manual copying of text/JSON, not a live agent or API connection. Live Windows cases deliberately do not use that unrestricted exporter.

Future Codex, API-provider, direct-chat-tool, and local-model connections remain optional adapters. Their authentication, current provider policies, costs, permissions and privacy projection must be verified at implementation. Orthonis should remain useful without AI; losing model access must not trigger hidden paid usage or bypass local controls.

## Development direction

Keep the small built-in modular architecture and CLI-first policy described in [Architecture](ARCHITECTURE.md). OFC3 established one real read-only Windows source boundary. A possible next campaign slice is OFC4 for bounded Windows Reliability collection, followed by OFC5 for cross-module Windows/private-data validation, but no later slice is selected automatically.

Distribution/signing, software licensing, desktop UX, supported consumer Windows versions, live-data export policy and privileged-operation threat modeling remain open decisions.
