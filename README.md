# Orthonis

**Understand your PC. Repair with evidence.**

Orthonis is a Windows diagnostics, maintenance, and controlled-repair project. Its goal is to explain a problem, gather the evidence needed to investigate it, propose a supported change, and check whether that change helped. Cleanup is one feature, not the product's identity.

## Current working milestone

The OFC foundation provides a **runnable C#/.NET command-line demonstration using synthetic data**: one case/evidence core, built-in Startup and Reliability modules, local case persistence, understandable reports, and a validated manual discovery-plan round trip. It does not scan or repair your PC. There is no desktop interface, live AI connection, installer, real-data redactor, or Windows collector yet.

The [foundation campaign](docs/campaigns/OFC_FOUNDATION.md) records exact build/test evidence and the remaining Windows pilot. The [foundation guide](docs/FOUNDATION_GUIDE.md) explains commands, report exchange, and limits. Work is on `codex/ofc-foundation` in review PR #1; publication on that branch does not mean it is merged into `main`.

## Build and try the synthetic workflow

Use the .NET SDK selected by [global.json](global.json). Python 3.12 or later is needed for the smoke and repository checks. Run from the repository root:

```sh
dotnet build Orthonis.slnx --configuration Release
dotnet run --project tests/Orthonis.Tests --configuration Release --no-build
python tools/smoke_foundation.py
```

The smoke command uses temporary synthetic cases and checks preview, approval, stale/replay refusal, and persistence through separate CLI processes. To keep a demonstration case:

```sh
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- start .local/demo missing
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- report .local/demo
```

No administrator permission is needed for the fixture demonstration. Use a new directory for another case; `start` does not overwrite a saved case. Read the [guide](docs/FOUNDATION_GUIDE.md) before exchanging a plan.

## Start here for development

Agents begin with [AGENTS.md](AGENTS.md), then [Current Task](docs/CURRENT_TASK.md). Only Current Task selects work; a roadmap, memory, or experiment does not authorize execution.

| Need | Owner |
| --- | --- |
| Product purpose, current capabilities, future direction | [Project](docs/PROJECT.md) |
| Component boundaries and safety invariants | [Architecture](docs/ARCHITECTURE.md) |
| Active assignment and restart point | [Current Task](docs/CURRENT_TASK.md) |
| Development practices, GitHub/local handoff, publication | [Workflow](docs/WORKFLOW.md) |
| Verification commands and evidence boundaries | [Project Health](docs/PROJECT_HEALTH.md) |
| EUTONOS adoption provenance and limits | [EUTONOS adoption](docs/EUTONOS_ADOPTION.md) |
| Targeted navigation | [NAV](NAV/README.md) |
| Historical checkpoints | [RAM](RAM/README.md) |
| Proposed experiments, not an execution queue | [Experiments](docs/Experiments.md) |

[eutonos.read.json](eutonos.read.json) is the stable-ID/owner catalog. EUTONOS is a repository-files development workflow, not an application dependency or installed runtime.

This public repository contains synthetic fixtures and reviewed public-safe documentation. Do not commit real diagnostic reports, credentials, private transcripts, or runtime state. The foundation includes no software license; distribution/licensing decisions remain open.
