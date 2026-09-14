# Orthonis architecture direction

Document ID: `orthonis.doc.architecture`.

Status: proposed product boundaries for future design and implementation. No product component is implemented by OED1.

## Component boundary

```text
Windows UI / command-line or test harness
  -> case and evidence core
  -> findings and comparison logic
  -> report exporter / proposal importer
  -> capability policy and action journal
  -> platform adapters and a narrowly scoped privileged helper, if needed
```

Keep the portable core free of direct registry, service, event-log, and UI calls. A Windows collector supplies typed evidence with its collection outcome and limitations. Tests can supply synthetic evidence through the same interface; that proves only the tested core behavior, not collector accuracy on real machines.

The case store, report format, rule system, persistence engine, and exact interfaces remain design tasks. Do not freeze a database or dependency simply because it appears in an illustrative architecture.

## Evidence and findings

An observation should preserve its source, collection method, identity, time, relevant system scope, and outcome. Distinguish event time from collection time. A finding cites observations and separately records a hypothesis or recommendation. No historical baseline is invented when the app was not recording it.

Missing, failed, unsupported, stale, and healthy are different outcomes. A coincident update is not proven causation. Two similar applications are not a demonstrated conflict. Where useful, choose a bounded measurement or controlled comparison to distinguish alternatives.

## Discovery and repair proposals

The AI returns a work order in a versioned format, not executable code. The app publishes its actual capability catalog. Each requested action uses a supported operation and constrained parameters; local IDs resolve targets without trusting model-supplied filesystem paths.

Required future importer behavior:

- Reject malformed, ambiguous, duplicate-key, oversized, unsupported, truncated, and non-finite input before effects. Validate all operations, not just the first.
- Bind proposals to a case, evidence snapshot, capabilities, and relevant target preconditions. Refuse stale or already-completed operations when replay would be unsafe.
- Enforce risk and approval locally. Calling a plan discovery or marking it safe does not change an operation's permissions.
- Use structured argument passing and scoped implementations. No generic elevated shell escape hatch in the normal packet workflow.
- Apply filesystem confinement at the actual operation boundary, including links/reparse points and changes between validation and use where applicable.

A digest identifies selected bytes; it is not authorization or proof that a diagnosis is correct. These are requirements to test, not an implemented security boundary.

## Execution, recovery, and verification

Separate read-only collection from state changes. For each supported repair, specify applicability, prerequisites, exact effects, approval, expected observation, interruption handling, and realistic recovery. Journal before/after state and uncertain outcomes where feasible. Do not represent multi-step Windows changes as universally atomic or reversible.

Prefer one justified intervention and a meaningful comparison over a batch of unrelated optimizations. A change without improvement is a useful negative result, not automatically a successful repair.

Broad registry deletion, firmware flashing, boot-configuration changes, and disabling security protections are outside routine AI-directed maintenance. Future tests of consequential effects begin in a disposable Windows environment, not the owner's everyday PC.

## Privacy and privilege

Keep full evidence local by default. Export selected, minimized records with stable aliases; present an export preview. Do not automatically export memory dumps, entire logs, unrestricted command lines, secrets, or personal content. Redaction is a tested feature with limits, not a guarantee supplied by a prompt.

Treat log messages and AI proposals as untrusted data. The normal UI and model process should not run as administrator. A future privileged helper exposes a small authenticated, validated operation interface and cannot be bypassed through another agent tool. Model access does not imply host permissions.

## Development and execution environments

| Environment | Valid evidence | Not established |
| --- | --- | --- |
| GitHub connector | Selected committed source and actual read/write/readback results | Local dirty state, PC access, a callable installed runtime |
| Isolated development container | Commands and tests actually run on its files | Windows behavior, a persistent installed service, another checkout's state |
| GitHub Actions, when selected | Results on the specific runner/image and commit | The owner's Windows desktop or hardware behavior |
| Windows pilot | The explicitly tested machine, build, permissions, and scenario | Every supported-looking device or all possible repairs |

EUTONOS supports development continuity through the repository owners. It is not an application dependency. Adding a runtime later must preserve this repository's IDs, owners, public-safety boundary, and acceptance distinctions.
