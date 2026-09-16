# Orthonis

**Understand your PC. Repair with evidence.**

Orthonis is a Windows diagnostics, maintenance, and controlled-repair project. Its goal is to explain a problem, gather the evidence needed to investigate it, propose a supported change, and check whether that change helped. Cleanup is one feature, not the product's identity.

## Current working milestone

The OFC foundation is a C#/.NET CLI with a portable case/evidence core, synthetic Startup and Reliability modules, local case persistence, reports, and a validated manual discovery-plan round trip. OFC3 adds an explicit opt-in, read-only Windows Startup source for a bounded native-view HKCU/HKLM `Run` subset plus conservative target revalidation and local executable-presence inspection. OFC4 adds a separate opt-in Windows Reliability source for bounded local Application event metadata in the same case system.

Neither live path is comprehensive. Startup never executes, disables or deletes a target. Reliability reads a seven-day, 64-record subset, records coverage/history uncertainty and deduplicates repeated event records without calling them incident or crash counts. It does not interpret event payloads or infer causes.

Live case data stays local: the ordinary synthetic report/example exporter rejects live cases, while local summaries expose bounded outcomes and opaque IDs instead of registry commands, paths, provider strings or raw event payloads. There is no desktop UI, installer, repair executor, live AI connection or general real-data exporter.

The checked OFC foundation was integrated into `main` through PR #1 at `a8609aa3fbb46b2e9eb7180345f52db14c651477`. This is not a release or deployment, and OFC5 remains unselected. The [foundation campaign](docs/campaigns/OFC_FOUNDATION.md) records exact slice/test evidence and the [foundation guide](docs/FOUNDATION_GUIDE.md) owns commands and privacy limits.

## Build and verify

Use the .NET SDK selected by [global.json](global.json) and Python 3.12 or later:

```sh
dotnet build Orthonis.slnx --configuration Release
dotnet run --project tests/Orthonis.Tests --configuration Release --no-build
python tools/smoke_foundation.py
python tools/smoke_ofc3.py
python tools/smoke_ofc4.py
python tools/check_repository.py
python -m unittest discover -s tests -v
```

To try only synthetic data:

```sh
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- start .local/demo missing
dotnet run --project src/Orthonis.Cli --configuration Release --no-build -- report .local/demo
```

On a supported Windows host, `start-windows <case-directory>` and `start-reliability <case-directory>` are separate explicit live opt-ins. Use distinct new directories. Read the [foundation guide](docs/FOUNDATION_GUIDE.md) before using them; live case files are private working data and `report`/`example-plan` are deliberately blocked for them.

## Start here for development

Agents begin with [AGENTS.md](AGENTS.md), then [Current Task](docs/CURRENT_TASK.md). Only Current Task selects work; a roadmap, campaign, memory or experiment does not authorize execution.

| Need | Owner |
| --- | --- |
| Product purpose, current capabilities, future direction | [Project](docs/PROJECT.md) |
| Component boundaries and safety invariants | [Architecture](docs/ARCHITECTURE.md) |
| Active assignment and restart point | [Current Task](docs/CURRENT_TASK.md) |
| Development practices, GitHub/local handoff, publication | [Workflow](docs/WORKFLOW.md) |
| Verification commands and evidence boundaries | [Project Health](docs/PROJECT_HEALTH.md) |
| Bounded Reliability contract and API decision | [OFC4 Reliability](docs/OFC4_RELIABILITY.md) |
| EUTONOS adoption provenance and limits | [EUTONOS adoption](docs/EUTONOS_ADOPTION.md) |
| Targeted navigation | [NAV](NAV/README.md) |
| Historical checkpoints | [RAM](RAM/README.md) |
| Proposed experiments, not an execution queue | [Experiments](docs/Experiments.md) |

[eutonos.read.json](eutonos.read.json) is the stable-ID/owner catalog. EUTONOS is a repository-files development workflow, not an application dependency or installed runtime.

This public repository contains synthetic fixtures, source and public-safe documentation. Do not commit live case files, real diagnostic reports, credentials, private transcripts or runtime state. The foundation includes no software license; distribution/licensing decisions remain open.
