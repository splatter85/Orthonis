# Orthonis navigation

Document ID: `orthonis.doc.nav`.

This is an authored routing map, not a generated NAV sweep or ATLAS graph. Resolve IDs through [the catalog](../eutonos.read.json), then inspect source at the selected commit. Known direct paths remain valid.

| Question | Read | Stable ID |
| --- | --- | --- |
| What is selected now? | [Current Task](../docs/CURRENT_TASK.md) | `orthonis.doc.current-task` |
| What rules govern work? | [Agent entry](../AGENTS.md), [Workflow](../docs/WORKFLOW.md) | `orthonis.doc.agents`, `orthonis.doc.workflow` |
| What does the product do now versus later? | [Project](../docs/PROJECT.md) | `orthonis.doc.project` |
| How are components and safety boundaries arranged? | [Architecture](../docs/ARCHITECTURE.md) | `orthonis.doc.architecture` |
| What did the first campaign establish? | [OFC](../docs/campaigns/OFC_FOUNDATION.md) | `orthonis.doc.ofc` |
| How do I run it and hand off to Windows? | [Foundation guide](../docs/FOUNDATION_GUIDE.md) | `orthonis.doc.foundation-guide` |
| What checks must actually run? | [Project Health](../docs/PROJECT_HEALTH.md) | `orthonis.doc.health` |
| Which data types connect modules? | [Contracts](../src/Orthonis.Core/Contracts.cs) | `orthonis.source.contracts` |
| What schedules collection and validates requests? | [Investigation](../src/Orthonis.Core/Investigation.cs) | `orthonis.source.investigation` |
| Where is the imported-plan boundary? | [Discovery plans](../src/Orthonis.Core/DiscoveryPlans.cs), [JSON](../src/Orthonis.Core/JsonCodec.cs) | `orthonis.source.discovery-plans`, `orthonis.source.json-codec` |
| Where does the report come from? | [Report](../src/Orthonis.Core/Report.cs) | `orthonis.source.report` |
| Where are collection and analysis modules? | [Startup](../src/Orthonis.Modules/Startup.cs), [Reliability](../src/Orthonis.Modules/Reliability.cs) | `orthonis.source.startup`, `orthonis.source.reliability` |
| Where are host composition and persistence? | [CLI](../src/Orthonis.Cli/Program.cs), [Case store](../src/Orthonis.Cli/CaseStore.cs) | `orthonis.source.cli`, `orthonis.source.case-store` |
| Where are the executable regression checks? | [Core tests](../tests/Orthonis.Tests/Program.cs), [Plan tests](../tests/Orthonis.Tests/PlanChecks.cs), [CLI smoke](../tools/smoke_foundation.py) | `orthonis.source.core-tests`, `orthonis.source.plan-tests`, `orthonis.source.cli-smoke` |
| Is a native EUTONOS runtime installed? | [Adoption evidence](../docs/EUTONOS_ADOPTION.md) | `orthonis.doc.eutonos-adoption` |
| What history or experiments are available? | [RAM](../RAM/README.md), [Experiments](../docs/Experiments.md) | `orthonis.doc.ram-guide`, `orthonis.doc.experiments` |

Keep issued identities. Add useful routes when source meaning grows; a file move or version does not create a new identity. The catalog and links establish locations, not source correctness or permission to run a repair.
