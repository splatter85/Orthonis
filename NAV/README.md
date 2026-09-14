# Orthonis navigation

Document ID: `orthonis.doc.nav`.

This is an authored document-routing map for the repository-only deployment. It is not a generated source index, native NAV sweep result, or ATLAS graph. Resolve IDs through [the catalog](../eutonos.read.json), then inspect the selected source revision. Known direct paths remain valid.

| Question | Read | Stable ID |
| --- | --- | --- |
| What am I authorized to do now? | [Current Task](../docs/CURRENT_TASK.md) | `orthonis.doc.current-task` |
| What rules govern this session? | [Agent entry](../AGENTS.md) | `orthonis.doc.agents` |
| What is Orthonis intended to do? | [Project](../docs/PROJECT.md) | `orthonis.doc.project` |
| Where are privilege, AI, and repair boundaries? | [Architecture](../docs/ARCHITECTURE.md) | `orthonis.doc.architecture` |
| How do I publish or hand off to Windows? | [Workflow](../docs/WORKFLOW.md) | `orthonis.doc.workflow` |
| What can I actually test? | [Project Health](../docs/PROJECT_HEALTH.md) | `orthonis.doc.health` |
| Is an EUTONOS runtime installed? | [Adoption evidence](../docs/EUTONOS_ADOPTION.md) | `orthonis.doc.eutonos-adoption` |
| What useful setup history is available? | [RAM guide](../RAM/README.md) | `orthonis.doc.ram-guide` |
| Which experiments are merely proposed? | [Experiments](../docs/Experiments.md) | `orthonis.doc.experiments` |
| Where do repository checks live? | [Checker](../tools/check_repository.py) and [tests](../tests/test_repository.py) | `orthonis.source.repository-checker`, `orthonis.source.repository-tests` |

There is no application source to map yet. Add source-specific routes when a selected implementation creates meaningful targets. Update only affected bindings/routes and preserve issued IDs; a changed path or version is not a new identity.
