# Orthonis project

Document ID: `orthonis.doc.project`.

## Purpose and initial users

Help a Windows PC owner understand a problem, investigate relevant evidence, make a controlled repair, and determine whether it helped. The initial pilot is owner-operated; a broader consumer release remains future scope.

Orthonis is a diagnostics, maintenance, and evidence-based repair project rather than a registry cleaner or generic speed booster. Success is a useful supported diagnosis, fewer unnecessary changes, and honest uncertainty, not the number of issues reported.

## Present capabilities

The OFC foundation is a C#/.NET 10 command-line application with a portable case/evidence core, built-in synthetic Startup and Reliability modules, bounded strict JSON, local versioned case storage, Markdown reports, and a manual discovery-plan preview/approval/follow-up loop.

OFC3 provides opt-in live Windows Startup collection for HKCU and HKLM `Software\Microsoft\Windows\CurrentVersion\Run` in the native registry view, bounded to 32 values per key. Cases record source scope and coverage, use opaque case-scoped targets, re-read the selected registration before and after conservative local executable attribute inspection, and never execute a target. Changed or removed registrations become stale/unavailable rather than being remapped. Enablement is unknown; registration does not prove execution, boot delay, malware, health or a required repair.

OFC4 provides a separate opt-in live Windows Reliability source in the same case/evidence/storage system. It reads local Application event metadata for a fixed seven-day, 64-record subset, preserves occurrence times and query intervals, records partial results and retention/clear uncertainty, and deduplicates records across persisted query history. Event-record counts are not incident/crash counts. Provider-specific payloads, messages, dumps and causes are not interpreted. See [the Reliability contract](OFC4_RELIABILITY.md).

Live cases remain private local working data. Default live summaries exclude raw registry commands, paths, provider strings and event payloads. The synthetic `report`/`example-plan` export path is blocked for live cases. The local plan/approval loop requests supported follow-up reads without enabling cloud sharing. Synthetic reports remain available for the manual AI exchange.

There is no desktop UI, repair executor, installer, tested minimized real-data AI exporter or model-provider integration. The OFC foundation was integrated into `main` through PR #1; that integration was not a release or deployment. [Foundation Guide](FOUNDATION_GUIDE.md) owns commands and [OFC](campaigns/OFC_FOUNDATION.md) owns execution evidence and acceptance limits.

## Intended product capabilities

| Area | Intended outcome |
| --- | --- |
| Maintain | Storage analysis, carefully scoped cleanup, startup review, update visibility, and supported Windows integrity checks. |
| Investigate | Symptom-led cases, snapshots, changes over time, bounded performance capture, reliability events, and evidence-based conflict hypotheses. |
| Repair and verify | Supported actions with prerequisites, clear approvals, execution records, operation-specific recovery, and comparison after a change. |
| Explain | Plain-language findings separating observed facts, hypotheses, unavailable checks, and recommended measurements. |

Storage growth, Windows integrity, crash-cause analysis, driver history and software conflicts describe future scope. A missing registry or executable reference, or a reported event, does not justify deletion or establish performance impact.

## Optional AI

The implemented exchange mechanism for synthetic cases exports a report with available capabilities and accepts a structured discovery proposal after local validation and approval. The transport is manual copying of text/JSON, not a live agent or API connection. Live Windows cases deliberately do not use that unrestricted exporter.

Future Codex, API-provider, direct-chat-tool and local-model connections remain optional adapters. Their authentication, current provider policies, costs, permissions and privacy projection must be verified at implementation. Orthonis should remain useful without AI; losing model access must not trigger hidden paid usage or bypass local controls.

## Development direction

Keep the small built-in modular architecture and CLI-first policy described in [Architecture](ARCHITECTURE.md). OFC3 and OFC4 add narrow read-only Windows sources. OFC5 completed a bounded cross-module pilot on one selected owner PC and retained the blocked live-export policy; this is not comprehensive collector, hardware-health or consumer-Windows acceptance.

Distribution/signing, software licensing, desktop UX, supported consumer Windows versions, any future minimized live-data export and privileged-operation threat modeling remain open decisions.
