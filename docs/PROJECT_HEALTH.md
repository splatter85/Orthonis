# Project health and verification

Document ID: `orthonis.doc.health`.

## Foundation build and executable checks

Use the SDK selected in [global.json](../global.json) and Python 3.12 or later. From the repository root:

```sh
dotnet build Orthonis.slnx --configuration Release
dotnet run --project tests/Orthonis.Tests --configuration Release --no-build
python tools/smoke_foundation.py
python tools/check_repository.py
python -m unittest discover -s tests -v
```

The C# suite is a small executable regression harness with a failing exit code on any failed check. It is intentionally invoked by `dotnet run`, not discovered by `dotnet test`. [Core/module/storage regressions](../tests/Orthonis.Tests/Program.cs) and [plan regressions](../tests/Orthonis.Tests/PlanChecks.cs) own assertions. The [CLI smoke](../tools/smoke_foundation.py) launches separate processes against temporary synthetic cases and checks preview, refusal, approval, replay, unchanged case bytes and fresh-process reload.

The [build configuration](../Directory.Build.props) treats compiler/analyzer warnings as errors. .NET library APIs are used without external package dependencies. The [solution](../Orthonis.slnx) builds Core, Modules, CLI and the regression harness together. CLI usage is in [Foundation guide](FOUNDATION_GUIDE.md).

## Repository consistency checks

The independently authored Python checker validates this deployment's explicit catalog and Markdown owners: unique IDs, case-distinct source paths, owner routing, selected work shape, exact scoped references, local links/anchors and exclusions. It does not fetch external links, repair files, certify all TokenSlang profiles, scan secrets, or execute a native EUTONOS runtime. Its tests include malformed, missing, ambiguous, escaping, symlink and stale-reference cases.

An alternate source root can be checked with `python tools/check_repository.py --root /path/to/Orthonis`. The source catalog is a selected useful inventory, not a claim that every source line is semantically mapped. Build and application behavior require their separate tests.

## Hosted checks and evidence owners

[Foundation CI](../.github/workflows/foundation.yml) runs for pull requests into `main`, only while the repository is public, on standard `ubuntu-24.04` and `windows-2022` runners. Each job has a ten-minute bound and read-only repository permissions. Actions are pinned; checkout credentials are not persisted. The workflow uploads no artifacts or caches and makes no model calls. Changing visibility or using billable resources needs a separate decision.

[OFC campaign](campaigns/OFC_FOUNDATION.md) owns observed application results, exact source views, run IDs and unrun boundaries. [EUTONOS adoption](EUTONOS_ADOPTION.md) preserves OED1's earlier file-mode setup evidence; it is not current product acceptance. GitHub job logs are the direct command/result evidence. A self-written result document is not a substitute for inspecting them.

Before publication review paths, source diff and actual authority. After publication fetch the exact commit/tree and relevant owners. PR CI may test a generated merge commit; retain that identity or compare its tree to the branch head rather than assuming identical bytes. The containing Git commit identifies an evidence record's version; do not invent a future self-referential commit.

## Acceptance limits

C# build/regression and CLI success establish only tested synthetic behavior on the recorded hosts. They do not establish Windows registry/event-log collector accuracy, actual PC health, UI quality, installation, repair safety, provider authentication, real-data redaction, independent model comprehension, or AI savings.

Storage failure injection covers failure before file replacement, not sudden power loss or every filesystem race. The case store is for cooperating local writers, not hostile administrators. Inputs are bounded and strict, but that is not a complete hostile-code sandbox or prompt-injection solution.

The real Windows read-only pilot remains owed. Record its OS/build, permissions, source coverage and actual collector checks separately; do not treat fixture results as real observations. Real-data export and privileged repairs require their own review and tests before they are enabled.
