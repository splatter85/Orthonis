#!/usr/bin/env python3
"""No-dump OFC3 fresh-process checks. Controlled sources run only in the test assembly."""
from __future__ import annotations
import json
import os
import subprocess
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PREFIX = ["dotnet", "run", "--project"]
SUFFIX = ["--configuration", "Release", "--no-build", "--"]
SENTINEL = "PRIVATE_SENTINEL_7348"


def run(project: str, *args: str, expected: int = 0) -> str:
    result = subprocess.run([*PREFIX, str(ROOT / project), *SUFFIX, *args], cwd=ROOT,
                            capture_output=True, text=True, encoding="utf-8", timeout=45, check=False)
    # Do not include argv, stdout, stderr, paths or case bytes in failures.
    if result.returncode != expected:
        raise RuntimeError("OFC3 subprocess returned an unexpected exit code; output withheld")
    if SENTINEL in result.stdout or SENTINEL in result.stderr:
        raise RuntimeError("OFC3 private-data sentinel escaped the output boundary")
    return result.stdout.lstrip("\ufeff")


def fixture_round_trip() -> None:
    with tempfile.TemporaryDirectory(prefix="orthonis-ofc3-controlled-") as temp:
        directory = Path(temp) / "case"
        plan = Path(temp) / "plan.json"
        def phase(name: str) -> None:
            run("tests/Orthonis.Tests", "--ofc3-phase", name, str(directory), str(plan))
        phase("create")
        file = directory / "case.json"
        before = file.read_bytes()
        saved = json.loads(before)
        assert saved["schemaVersion"] == 2 and saved["source"]["mode"] == "windowsStartup"
        assert SENTINEL in before.decode() and "--token" not in before.decode()
        phase("plan")
        phase("preview")
        assert file.read_bytes() == before
        valid = plan.read_bytes()
        invalid = dict(json.loads(valid), command=SENTINEL)
        plan.write_text(json.dumps(invalid), encoding="utf-8")
        phase("refuse")
        assert file.read_bytes() == before
        plan.write_bytes(valid)
        phase("missing")
        phase("summary-missing")
        assert json.loads(file.read_bytes())["revision"] == 2
        phase("plan")
        phase("present")
        phase("summary-present")
        after = file.read_bytes()
        assert json.loads(after)["revision"] == 3
        phase("refuse")
        assert file.read_bytes() == after
        # These routes use the production process, not the injected dispatcher.
        run("src/Orthonis.Cli", "report", str(directory), expected=2)
        run("src/Orthonis.Cli", "example-plan", str(directory), expected=2)
        assert "historical findings: 1" in run("src/Orthonis.Cli", "summary", str(directory))
        assert file.read_bytes() == after
    print("PASS OFC3 controlled CLI: create, fresh reload, preview/refusal unchanged, missing inspection, fresh reload, contradictory present inspection, history, replay, export gates")


def native_read() -> None:
    if os.name != "nt":
        print("SKIP actual Windows Run collection: non-Windows host; no synthetic fallback")
        return
    with tempfile.TemporaryDirectory(prefix="orthonis-ofc3-private-") as temp:
        directory = Path(temp) / "case"
        output = run("src/Orthonis.Cli", "start-windows", str(directory))
        assert "LOCAL / PRIVATE" in output and "SYNTHETIC DEMONSTRATION" not in output
        file = directory / "case.json"
        saved = json.loads(file.read_bytes())
        coverage = [e for e in saved["evidence"] if e["detailSchema"] == "startup-coverage.v2"]
        assert len(coverage) == 2
        assert {e["coverage"]["portion"] for e in coverage} == {"currentUserRun", "localMachineRun"}
        assert all(e["coverage"]["returned"] <= 32 for e in coverage)
        targets = [e for e in saved["evidence"] if e["detailSchema"] == "startup-run.v2"]
        assert len(targets) <= 64
        run("src/Orthonis.Cli", "summary", str(directory))
        run("src/Orthonis.Cli", "report", str(directory), expected=2)
        run("src/Orthonis.Cli", "example-plan", str(directory), expected=2)
        plan_args = ["local-plan", str(directory)]
        if targets:
            plan_args.append(targets[0]["targetId"])
        plan = Path(temp) / "plan.json"
        plan.write_text(run("src/Orthonis.Cli", *plan_args), encoding="utf-8")
        before = file.read_bytes()
        run("src/Orthonis.Cli", "apply", str(directory), str(plan))
        assert file.read_bytes() == before
        run("src/Orthonis.Cli", "apply", str(directory), str(plan), "--approve")
        assert json.loads(file.read_bytes())["revision"] == 2
        run("src/Orthonis.Cli", "summary", str(directory))
        outcomes = {status: sum(e["status"] == status for e in coverage)
                    for status in ("observed", "empty", "permissionDenied", "limited", "failed", "unavailable", "unsupported")}
        print("PASS actual bounded Windows Run read and fresh-process local case loop; counts only:",
              "registrations=", len(targets), "targeted_checks=", int(bool(targets)), "key_outcomes=", outcomes)
    run("tests/Orthonis.Tests", "--ofc3-file-probe")
    print("PASS native local existing/missing file-attribute checks; no target execution, autorun writes or data artifacts")


if __name__ == "__main__":
    fixture_round_trip()
    native_read()
