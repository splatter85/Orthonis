# Foundation usage and Windows handoff

Document ID: `orthonis.doc.foundation-guide`.

## What this build does

Orthonis is still a developer-facing CLI, not a finished desktop PC scanner. The portable foundation provides synthetic Startup and Reliability scenarios and the manual report/discovery-plan loop. OFC3 adds one explicit live Windows path for the current user's and local machine's `Software\Microsoft\Windows\CurrentVersion\Run` keys in the native registry view.

The live path is read-only and bounded to 32 values per key. It does not cover RunOnce, Startup folders, Task Scheduler, services, drivers, shell extensions or the alternate registry view. It does not infer effective enablement, boot delay, malware, health or a required repair. No command modifies Windows configuration or executes a startup target, and no model service is called.

## Build and verify

```sh
dotnet build Orthonis.slnx --configuration Release
dotnet run --project tests/Orthonis.Tests --configuration Release --no-build
python tools/smoke_foundation.py
python tools/smoke_ofc3.py
python tools/check_repository.py
python -m unittest discover -s tests -v
```

The C# regression suite is an executable harness run with `dotnet run`, not `dotnet test`.

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

`start-windows` never falls back to fixtures on an unsupported host. A live case uses schema 2 with validated source scope/coverage and private case-salted machine/user bindings. Inventory output exposes opaque case-scoped target IDs, not raw Run value names, command lines or file paths. Unknown enablement remains `Unknown`.

To prepare a local follow-up plan, optionally select an opaque target ID printed by `summary`:

```sh
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- local-plan .local/windows-startup <target-id> > .local/windows-plan.json
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- apply .local/windows-startup .local/windows-plan.json
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- apply .local/windows-startup .local/windows-plan.json --approve
```

Treat the plan and case as local/private data. Preview validates without collection; approval rereads the persisted registration, inspects only a conservative supported local `.exe` form without execution, rereads the registration after the file observation, and records stale/unavailable/unsupported rather than silently retargeting a changed registration.

Do not copy a live case through the synthetic exporter. These commands intentionally refuse live data:

```sh
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- report .local/windows-startup
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- example-plan .local/windows-startup
```

A live `summary` is a bounded local view, not a sanitized export guarantee. The raw `case.json` contains private locators/bindings needed for fresh-process revalidation and must remain local.

## Version and trust boundaries

DiscoveryPlan remains schema 1 and is always case/revision/snapshot-hash bound. Synthetic capabilities `startup.inventory`, `startup.inspect`, `reliability.summary`, and `reliability.inspect` remain version 1. Windows Startup advertises only `startup.inventory` and `startup.inspect`, version 2. Fixed schema-1 case/plan fixtures verify compatibility; schema-1 cases are not implicitly migrated into live schema-2 cases.

Inputs remain bounded and strict, but a digest/source label is not authentication. Built-in modules are trusted code, cancellation is cooperative, and the case store protects cooperating writers rather than hostile administrators. Exit codes are 0 completed/preview, 1 internal failure, 2 refusal/unsupported/export blocked, 3 cancelled, and 4 storage unavailable/busy/permission failure.

## Restart after OFC3

OFC3 is complete at the repository and hosted Windows-runner boundary. Owner-PC acceptance remains unrun. [Current Task](CURRENT_TASK.md) is intentionally idle; OFC4 live Reliability and OFC5 combined Windows/privacy validation are planned in [OFC](campaigns/OFC_FOUNDATION.md) but are not selected automatically. Keep the CLI-first policy until a later explicit UI decision.
