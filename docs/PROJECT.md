# Orthonis project

Document ID: `orthonis.doc.project`.

## Purpose and initial users

Help a Windows PC owner understand a problem, investigate relevant evidence, make a controlled repair, and determine whether it helped. The initial development pilot is an owner-operated PC; a broader consumer release is a future possibility, not an existing product.

The working name is Orthonis. The intended identity is diagnostics, maintenance, and evidence-based repair rather than a registry cleaner or generic speed booster. Product success is a useful supported diagnosis, fewer unnecessary changes, and honest uncertainty, not the number of issues reported.

## Present state

Only the repository workflow and its consistency-check tooling are being established. There is no application runtime, C# solution, Windows collector, repair action, installer, or AI connection. [EUTONOS adoption](EUTONOS_ADOPTION.md) owns deployment evidence; [Current Task](CURRENT_TASK.md) owns live work.

## Intended capabilities

| Area | Intended outcome |
| --- | --- |
| Maintain | Storage analysis, carefully scoped cleanup, startup review, update visibility, and supported Windows integrity checks. |
| Investigate | Symptom-led cases, snapshots, changes over time, bounded performance capture, reliability events, and evidence-based conflict hypotheses. |
| Repair and verify | Supported actions with prerequisites, clear approvals, execution records, operation-specific recovery, and comparison after a change. |
| Explain | Plain-language findings that distinguish observed facts, hypotheses, unavailable checks, and recommended next measurements. |

Examples such as startup investigation, storage growth, Windows integrity, crash analysis, driver history, and software conflicts describe future scope. They are not implemented detections or claims that a current PC has those problems. Missing paths or suspicious registry references do not alone justify deletion.

## Optional AI

The first intended AI workflow is manual: export a minimized report and available capabilities, analyze it in a chat service, then import a structured discovery or repair proposal. The application must validate it locally. Subsequent reports support an iterative investigation rather than a one-shot recommendation.

Live Codex, API-provider, direct-chat-tool, and local-model connections remain optional future adapters. Their authentication, current provider policies, account availability, cost, and tool permissions must be verified at implementation time. Do not promise unlimited/free usage or reuse credentials by extracting browser cookies or private authentication stores.

The product remains useful without AI. Loss of model access pauses AI investigation rather than silently incurring another charge or skipping local safety controls. Development-time EUTONOS and product-time diagnostic AI are separate concerns.

## Working development direction

Build a portable evidence/report/validation core against clearly labeled synthetic cases. Add Windows collectors behind interfaces, then verify them on Windows. C#/.NET with a Windows-native interface is a proposed technical direction, not a selected SDK version or shipped stack.

A candidate first product milestone is a read-only investigation flow: create a case, import or collect a snapshot, explain findings, export a report, and validate a request for more evidence. This direction is not authorization to start that milestone during OED1.

## Success measures

Evaluate correctness, false positives, evidence quality, unnecessary changes avoided, successful post-change verification, recovery behavior, collection overhead, and AI usage when available. A healthy computer should be allowed to have nothing that needs repair. Exact performance attribution must come from actual measurements.

## Deferred decisions

Select the first application slice, SDK/UI toolkit and supported Windows versions, distribution/signing strategy, application license, and threat-model details before the affected implementation. Do not introduce accounts, cloud storage, telemetry, monetization, or a privileged agent merely to complete repository setup.

Future possibilities include richer tracing, configuration history, more application-specific collectors, and live AI. Their order depends on measured usefulness and selected work, not this list alone.
