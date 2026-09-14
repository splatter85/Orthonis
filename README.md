# Orthonis

**Understand your PC. Repair with evidence.**

Orthonis is a planned Windows diagnostics, maintenance, and controlled-repair application. Its goal is to explain a problem, gather the evidence needed to investigate it, propose a supported change, and check whether that change helped. Cleanup is one feature, not the product's identity.

## Project status

This repository is starting with a **GitHub-first EUTONOS documentation and handoff workflow**. There is no application, installer, Windows scanner, repair executor, or connected AI agent yet. The exact adoption status and verification limits are recorded in [EUTONOS adoption](docs/EUTONOS_ADOPTION.md).

## Start here

Agents begin with [AGENTS.md](AGENTS.md), then [Current Task](docs/CURRENT_TASK.md). People can start with [Project](docs/PROJECT.md). Only Current Task selects work; a roadmap, memory, or experiment does not authorize execution.

| Need | Owner |
| --- | --- |
| Product purpose, intended capabilities, and future direction | [Project](docs/PROJECT.md) |
| Proposed component boundaries and safety invariants | [Architecture](docs/ARCHITECTURE.md) |
| Active assignment and restart point | [Current Task](docs/CURRENT_TASK.md) |
| GitHub/local workflow and publication rules | [Workflow](docs/WORKFLOW.md) |
| Actual verification commands and evidence boundaries | [Project Health](docs/PROJECT_HEALTH.md) |
| Adoption provenance, selected capabilities, and limits | [EUTONOS adoption](docs/EUTONOS_ADOPTION.md) |
| Targeted navigation | [NAV](NAV/README.md) |
| Source-bound historical checkpoints | [RAM](RAM/README.md) |
| Proposed experiments, not an execution queue | [Experiments](docs/Experiments.md) |

[eutonos.read.json](eutonos.read.json) is the one stable-ID/owner binding catalog. The Markdown owners remain readable without an EUTONOS runtime or access to its upstream repository.

## Repository checks

With Python 3.12 or later, from the repository root:

```sh
python tools/check_repository.py
python -m unittest discover -s tests -v
```

These check this repository's documents and references. They do not test a Windows application or certify an installed EUTONOS runtime. See [Project Health](docs/PROJECT_HEALTH.md) before making broader claims.

This is a public repository. Commit synthetic fixtures and reviewed public-safe documentation only, not real diagnostic reports, credentials, private transcripts, or runtime state. Distribution and licensing decisions for the future app remain open; this setup adds no software license.
