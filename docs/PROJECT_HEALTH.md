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
python tools/check_repository.py
python -m unittest discover -s tests -v
```

The C# suite is an executable harness, not a `dotnet test` discovery project. `Ofc3Runner` first executes all 53 retained foundation checks, then the OFC3 source, permission, privacy, compatibility and CLI regressions. Controlled live tests use injected registry/probe seams and synthetic privacy sentinels; they do not manufacture autorun entries.

`smoke_foundation.py` preserves the synthetic separate-process report/plan path. `smoke_ofc3.py` exercises a fresh-process schema-2 create/reload/preview/approval/contradictory-inspection/reload path with controlled sources, verifies unchanged bytes on refusal/replay, verifies live report/example export gating, and on Windows also performs the bounded no-dump Run-key read plus native local executable attribute probes.

The solution now builds Core, Modules, Windows, CLI and the regression harness. Nullable checking, analyzers, warnings-as-errors and deterministic build settings remain enabled. `NuGet.Config` continues to clear external package feeds; OFC3 added no third-party package or desktop framework.

## Repository consistency checks

`python tools/check_repository.py` validates the explicit EUTONOS file catalog, stable IDs, single task owner, source paths, local links/anchors and exclusions. Its regression suite tests malformed/missing/ambiguous references, escaping, symlinks and stale catalog behavior. It is repository-structure evidence, not native EUTONOS runtime certification or application behavior evidence.

## Hosted checks and evidence owners

[Foundation CI](../.github/workflows/foundation.yml) runs pull-request commits on standard `ubuntu-24.04` and `windows-2022` runners with read-only repository permissions, pinned actions, no persisted checkout credentials, no caches/artifact uploads and no model calls.

The first OFC3 implementation `dedf2d16031bb464c917af3e9d1aa8da445cc02f` passed run `34878077559` on both runners. The Windows job's OFC3 smoke performed the actual bounded native HKCU/HKLM Run read and local existing/missing executable attribute probes while logging only nonidentifying counts/pass-fail data.

The permission-outcome follow-up `15cb3184325425c5e438ee73ab358377f81a2364` passed run `34885157156`. Windows job `104113872222` and Ubuntu job `104113872533` each passed build, executable regressions, foundation CLI smoke, OFC3 smoke, repository consistency and repository regressions. The Windows OFC3 smoke again passed the bounded native read. The closeout documentation's containing commit receives its own PR run.

[OFC](campaigns/OFC_FOUNDATION.md) owns slice results and unrun boundaries. A written result is not a substitute for the actual job. PR CI may test a generated merge commit; retain the branch SHA and run/job identities rather than calling that a merge into `main`.

## Acceptance limits

The Windows runner read proves only the implemented native-view bounded Run-key adapter and its tested runner environment. It does not establish owner-PC acceptance, comprehensive Startup coverage, live Reliability accuracy, consumer Windows support, PC health, UI quality, installer behavior, repairs, privileged safety or a tested minimized AI-export projection. Empty/incomplete results never certify a healthy PC.

The authoring environment used for OFC3 had no `dotnet` executable, so no local C# build is claimed. Owner-PC acceptance remains unrun. Live case/export privacy is fail-closed by blocking the unrestricted exporter; it is not yet evidence that a future minimized export policy is safe.
