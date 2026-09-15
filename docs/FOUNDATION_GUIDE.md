# Foundation usage and Windows handoff

Document ID: `orthonis.doc.foundation-guide`.

## What this build does

Orthonis is still a developer-facing CLI, not a finished desktop PC scanner. The portable foundation provides synthetic Startup and Reliability scenarios and the manual report/discovery-plan loop. OFC3 adds an explicit live Windows Startup path; OFC4 adds a separate explicit live Windows Application Reliability path. Both use the same case/coverage/storage/approval system, but their case directories and source modes remain separate.

The live paths are read-only and incomplete by design. No command modifies Windows configuration, writes/clears event logs, executes a startup target or calls a model service. A successful command exit does not certify PC health.

## Build and verify

```sh
dotnet build Orthonis.slnx --configuration Release
dotnet run --project tests/Orthonis.Tests --configuration Release --no-build
python tools/smoke_foundation.py
python tools/smoke_ofc3.py
python tools/smoke_ofc4.py
python tools/check_repository.py
python -m unittest discover -s tests -v
```

The C# regression suite is an executable harness run with `dotnet run`, not `dotnet test`. [Project Health](PROJECT_HEALTH.md) and [OFC](campaigns/OFC_FOUNDATION.md) distinguish actual execution from unrun acceptance.

## Synthetic demonstration and manual AI loop

```sh
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- start .local/demo missing
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- report .local/demo
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- example-plan .local/demo
```

Synthetic scenarios are `healthy`, `missing`, `denied`, and `inconclusive`; they are not host diagnoses. Save a returned schema-1 discovery-plan JSON as UTF-8, preview it, then explicitly approve it:

```sh
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- apply .local/demo .local/plan.json
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- apply .local/demo .local/plan.json --approve
```

Preview performs no collector calls and no case mutation. Approval revalidates the whole proposal and rejects stale/replayed/wrong-case plans.

## OFC3 Windows Startup path

Use a new local directory and opt in explicitly:

```sh
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- capabilities --windows-startup
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- start-windows .local/windows-startup
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- summary .local/windows-startup
```

The live path reads current-user and local-machine `Software\Microsoft\Windows\CurrentVersion\Run` in the native registry view, at most 32 values per key. RunOnce, Startup folders, Task Scheduler, services, drivers, shell extensions and the alternate view are excluded. Enablement remains unknown; registration does not prove execution, boot delay, malware, health or a required repair.

`start-windows` never falls back to fixtures on an unsupported host. A live case uses schema 2 with validated source scope/coverage and private case-salted machine/user bindings. Inventory output exposes opaque case-scoped target IDs, not raw Run value names, command lines or paths.

To prepare a local follow-up plan, optionally select an opaque target ID printed by `summary`:

```sh
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- local-plan .local/windows-startup <target-id> > .local/windows-plan.json
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- apply .local/windows-startup .local/windows-plan.json
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- apply .local/windows-startup .local/windows-plan.json --approve
```

Treat the plan and case as local/private data. Preview validates without collection; approval rereads the persisted registration, inspects only a conservative supported local `.exe` form without execution, rereads the registration after the file observation, and records stale/unavailable/unsupported rather than silently retargeting a changed registration.

## OFC4 Windows Reliability path

Use a different new local directory:

```sh
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- capabilities --windows-reliability
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- start-reliability .local/windows-reliability
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- summary .local/windows-reliability
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- local-plan .local/windows-reliability > .local/reliability-plan.json
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- apply .local/windows-reliability .local/reliability-plan.json
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- apply .local/windows-reliability .local/reliability-plan.json --approve
```

The [Reliability contract](OFC4_RELIABILITY.md) defines the narrow local Application channel selection: levels 1/2/3 plus Windows Error Reporting records, preceding seven days, newest 64 records plus a limit sentinel. It retains occurrence times, bounded query intervals and explicit incomplete/history-gap states. Overlapping scans do not inflate distinct-record counts. Reused record IDs remain distinct and carry a history warning.

The local refresh plan takes no target, custom channel, query or time range. Payloads, localized messages, dump files and causes are not interpreted. No-event, unknown-envelope, missing-history, limited, failed or denied results never establish PC health. Multiple log records may represent one incident, so the summary does not call record counts crash counts.

## Live privacy and version boundaries

Do not copy a live case through the synthetic exporter. For either live directory these commands intentionally refuse:

```sh
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- report .local/windows-reliability
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- example-plan .local/windows-reliability
```

A live `summary` is a bounded local view, not a sanitized export guarantee. Raw `case.json` and local plans contain private working data. Keep them local and outside GitHub. Save plans as UTF-8 JSON; shell redirection must use UTF-8 (PowerShell 7 does, older Windows PowerShell defaults can differ).

DiscoveryPlan remains schema 1 and is always case/revision/snapshot-hash bound. Synthetic capabilities `startup.inventory`, `startup.inspect`, `reliability.summary`, and `reliability.inspect` remain version 1. Windows Startup advertises only inventory/inspect version 2. Windows Reliability advertises only `reliability.summary` version 2. Fixed schema-1 fixtures remain supported; existing cases are not implicitly migrated or combined.

Inputs are bounded and strict, but a digest/source label is not authentication. Built-in modules are trusted code, cancellation is cooperative, and the case store protects cooperating writers rather than hostile administrators. Exit codes are 0 completed/preview, 1 internal failure, 2 refusal/unsupported/export blocked, 3 cancelled, and 4 storage unavailable/busy/permission failure.

## Continuation boundary

[Current Task](CURRENT_TASK.md) alone selects work. OFC5 combined owner-PC/private-data validation remains planned, not automatically authorized by either live adapter. Hosted Windows checks are not owner-PC acceptance. Keep the CLI-first policy until a later explicit UI decision.
