# Project health and verification

Document ID: `orthonis.doc.health`.

## Build and executable checks

Use the SDK selected in [global.json](../global.json) and Python 3.12 or later. From the repository root:

```sh
dotnet --info
dotnet build Orthonis.slnx --configuration Release
dotnet run --project tests/Orthonis.Tests --configuration Release --no-build
python tools/smoke_foundation.py
python tools/smoke_ofc3.py
python tools/smoke_ofc4.py
python tools/check_repository.py
python -m unittest discover -s tests -v
```

The C# suite is an executable harness, not a `dotnet test` discovery project. `Ofc3Runner` executes the 53 retained foundation checks, the OFC3 source/permission/privacy/compatibility/CLI regressions and the OFC4 Reliability regressions. Controlled tests use injected source seams and synthetic privacy sentinels; they do not manufacture autorun entries or write Windows events.

`smoke_foundation.py` preserves the synthetic separate-process report/plan path. `smoke_ofc3.py` exercises fresh-process schema-2 create/reload/preview/approval/contradictory-inspection/reload with controlled sources, unchanged bytes on refusal/replay and live export gating. On Windows it also performs the bounded no-dump Run-key read and local executable attribute probes.

`smoke_ofc4.py` exercises fresh-process Reliability create/preview/approval/reload, overlapping-record deduplication, replay/refusal byte preservation and privacy sentinels. On Windows it performs a real bounded Application event query and channel-metadata read; on other hosts it checks explicit refusal without fixture fallback. No native events are written and no live case artifacts are uploaded.

The solution builds Core, Modules, Windows, CLI and the regression harness. Nullable checking, analyzers, warnings-as-errors and deterministic build settings remain enabled. `NuGet.Config` continues to clear external package feeds; OFC3/OFC4 add no third-party package or desktop framework.

## Repository consistency checks

`python tools/check_repository.py` validates the explicit EUTONOS file catalog, stable IDs, single task owner, source paths, local links/anchors and exclusions. Its regression suite tests malformed/missing/ambiguous references, escaping, symlinks and stale catalog behavior. It is repository-structure evidence, not native EUTONOS runtime certification or application behavior evidence.

## Hosted checks and evidence owners

[Foundation CI](../.github/workflows/foundation.yml) runs pull-request commits on standard `ubuntu-24.04` and `windows-2022` runners with read-only repository permissions, pinned actions, no persisted checkout credentials, no configured cache/artifact uploads and no model calls.

OFC3 implementation and permission follow-up passed their recorded hosted jobs; the exact source/run/job identities remain in [OFC](campaigns/OFC_FOUNDATION.md). Those runs exercised the native Run adapter, not Reliability.

OFC4 source `2848e7fd8c6f48d37559018c3d721283f0c617de` passed run `34947701494`, Windows job `104310923329` and Ubuntu job `104310923110`. Both passed build, the expanded executable suite, foundation/OFC3/OFC4 smokes, repository consistency and Python regressions. Windows additionally passed the real native Startup and Application adapter reads; Ubuntu checked portable/control paths and unsupported-host refusal. The initial OFC4 parser build failed and was fixed before this successful run; see the campaign record.

The documentation closeout `7f62d3574b3d9bfedda0bbc4626edab385125af0` passed final-head run `34948312201`: Windows job `104312920996` and Ubuntu job `104312920648` each passed the relevant build, executable, smoke, repository consistency and Python regression lanes; Windows also repeated the real bounded Startup and Application reads. PR CI may test a generated merge commit; retain the branch SHA and run/job identities rather than calling that a merge into `main`.

## OFC5 owner-PC evidence

On 2026-09-16, OFC5 selection checkpoint `1df0996a4195cbe57ac5c3685952a8b48822b57c` used one selected x64 Windows 25H2 build `26200.9457` host with .NET SDK `10.0.401`. The Release solution build passed with zero warnings/errors. The first local .NET invocation performed SDK first-run setup and reported installation of an untrusted ASP.NET Core HTTPS development certificate; no certificate trust command was run. The executable harness reported 53 foundation checks, 110 cumulative foundation/OFC3 checks and 68 OFC4 controlled checks passed; the foundation, OFC3 and OFC4 smoke lanes passed locally. The native smokes performed only the documented bounded Run and Application reads and retained no live artifacts.

Separate ignored Startup and Reliability cases each completed create, fresh-process reload, preview/no mutation, approved revision advancement and replay refusal/no mutation. Startup retained 21 opaque target rows from complete bounded reads and an approved target inspection returned `Unsupported` safely. Reliability remained `Limited`/`Partial`, retained 64 of 65 examined records with six unknown envelopes and zero unparsed records, and a second read kept 64 distinct records across deduplicated history. Both live `report` and `example-plan` routes refused with exit 2. Private cases/plans remained local and were not uploaded.

The documentation-only OFC5 publication head `c5fe7e05715bc68d984c9135009f72651e8f2942` passed PR #2 run `35966218291`: Ubuntu job `107525264970` and Windows job `107525265153` each passed the build, executable harness, foundation/OFC3/OFC4 smokes, repository checker and Python regressions. GitHub reported the PR merged at `079bc1bb3b3d4dd4ccb5388c49d021c4e548188e`; `main` was read back at that commit. This hosted run rechecks the unchanged product source and public documentation; it does not enlarge the one-PC pilot claim.

## Acceptance limits

Hosted Windows reads establish the implemented bounded native adapters in that runner environment. The OFC5 owner-PC run establishes only the recorded bounded behavior on its one selected host; it does not establish comprehensive Startup/Reliability coverage, crash-cause accuracy, consumer Windows support, PC health, UI quality, installer behavior, repairs, privileged safety or a minimized AI-export projection. Event-record counts are not incident counts; empty/incomplete results never certify a healthy PC.

The authoring environment used for OFC3/OFC4 had no `dotnet` executable; OFC5 supplied the later local build/pilot evidence above without changing product source. Live export privacy remains fail-closed by blocking the unrestricted exporter, not proof that a future minimized export policy is safe. Controlled cancellation and source-drift regressions passed, but no live owner-PC cancellation or drift was manufactured. Native cancellation is cooperative, not kernel-call isolation; before/after log metadata is not an atomic snapshot or proof of uninterrupted history.
