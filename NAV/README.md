# Orthonis navigation

Document ID: `orthonis.doc.nav`.

This is an authored routing map, not a generated NAV sweep or ATLAS graph. Resolve IDs through [the catalog](../eutonos.read.json), then inspect source at the selected commit. Known direct paths remain valid.

| Question | Read | Stable ID |
| --- | --- | --- |
| What is selected now? | [Current Task](../docs/CURRENT_TASK.md) | `orthonis.doc.current-task` |
| What rules govern work? | [Agent entry](../AGENTS.md), [Workflow](../docs/WORKFLOW.md) | `orthonis.doc.agents`, `orthonis.doc.workflow` |
| What does the product do now versus later? | [Project](../docs/PROJECT.md) | `orthonis.doc.project` |
| How are components and safety boundaries arranged? | [Architecture](../docs/ARCHITECTURE.md) | `orthonis.doc.architecture` |
| What did the foundation campaign establish? | [OFC](../docs/campaigns/OFC_FOUNDATION.md) | `orthonis.doc.ofc` |
| How do I run synthetic and OFC3 Windows flows? | [Foundation guide](../docs/FOUNDATION_GUIDE.md) | `orthonis.doc.foundation-guide` |
| What checks must actually run? | [Project Health](../docs/PROJECT_HEALTH.md) | `orthonis.doc.health` |
| Which case/evidence types connect modules? | [Contracts](../src/Orthonis.Core/Contracts.cs) | `orthonis.source.contracts` |
| Where are live source identity and coverage defined? | [Source data](../src/Orthonis.Core/SourceData.cs) | `orthonis.source.source-data` |
| What schedules collection and validates requests? | [Investigation](../src/Orthonis.Core/Investigation.cs) | `orthonis.source.investigation` |
| Where is the imported-plan boundary? | [Discovery plans](../src/Orthonis.Core/DiscoveryPlans.cs), [JSON](../src/Orthonis.Core/JsonCodec.cs) | `orthonis.source.discovery-plans`, `orthonis.source.json-codec` |
| Where does the synthetic export report come from? | [Report](../src/Orthonis.Core/Report.cs) | `orthonis.source.report` |
| Where are portable synthetic modules? | [Startup](../src/Orthonis.Modules/Startup.cs), [Reliability](../src/Orthonis.Modules/Reliability.cs) | `orthonis.source.startup`, `orthonis.source.reliability` |
| Where are CLI dispatch and reusable operations? | [CLI](../src/Orthonis.Cli/Program.cs), [Case operations](../src/Orthonis.Cli/CaseOperations.cs), [Case store](../src/Orthonis.Cli/CaseStore.cs) | `orthonis.source.cli`, `orthonis.source.case-operations`, `orthonis.source.case-store` |
| Where is the bounded Windows Run collector? | [Run contracts](../src/Orthonis.Windows/RunContracts.cs), [Registry](../src/Orthonis.Windows/WindowsRunRegistry.cs), [Startup module](../src/Orthonis.Windows/WindowsStartupModule.cs) | `orthonis.source.windows-run-contracts`, `orthonis.source.windows-run-registry`, `orthonis.source.windows-startup-module` |
| Where are command/path safety checks? | [Command resolver](../src/Orthonis.Windows/RunCommand.cs), [Executable probe](../src/Orthonis.Windows/LocalExecutableProbe.cs) | `orthonis.source.windows-run-command`, `orthonis.source.windows-executable-probe` |
| Where are executable regressions? | [Core tests](../tests/Orthonis.Tests/Program.cs), [Plan tests](../tests/Orthonis.Tests/PlanChecks.cs), [Live tests](../tests/Orthonis.Tests/LiveChecks.cs), [Live CLI tests](../tests/Orthonis.Tests/LiveCliChecks.cs), [Permission tests](../tests/Orthonis.Tests/PermissionChecks.cs) | `orthonis.source.core-tests`, `orthonis.source.plan-tests`, `orthonis.source.live-tests`, `orthonis.source.live-cli-tests`, `orthonis.source.permission-tests` |
| Where are separate-process smokes? | [Foundation smoke](../tools/smoke_foundation.py), [OFC3 smoke](../tools/smoke_ofc3.py) | `orthonis.source.cli-smoke`, `orthonis.source.ofc3-smoke` |
| Is a native EUTONOS runtime installed? | [Adoption evidence](../docs/EUTONOS_ADOPTION.md) | `orthonis.doc.eutonos-adoption` |
| What history or experiments are available? | [RAM](../RAM/README.md), [Experiments](../docs/Experiments.md) | `orthonis.doc.ram-guide`, `orthonis.doc.experiments` |

Keep issued identities. Add useful routes when source meaning grows; a file move or version does not create a new identity. The catalog and links establish locations, not source correctness or permission to run a repair.
