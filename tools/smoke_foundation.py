#!/usr/bin/env python3
"""Exercise the real CLI in separate processes using synthetic evidence only."""
from __future__ import annotations
import json
import subprocess
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
COMMAND = ["dotnet", "run", "--project", str(ROOT / "src/Orthonis.Cli"), "--configuration", "Release", "--no-build", "--"]


def run(*args: str, expected: int = 0) -> str:
    result = subprocess.run([*COMMAND, *args], cwd=ROOT, capture_output=True,
                            text=True, encoding="utf-8", timeout=45, check=False)
    if result.returncode != expected:
        raise RuntimeError(f"Command {args[0]} returned {result.returncode}, expected {expected}: {result.stderr}")
    return result.stdout.lstrip("\ufeff")


def main() -> None:
    with tempfile.TemporaryDirectory(prefix="orthonis-smoke-") as temp:
        case = Path(temp) / "case"
        first = run("start", str(case), "missing")
        assert "SYNTHETIC DEMONSTRATION" in first
        case_file = case / "case.json"
        before = case_file.read_bytes()
        plan = json.loads(run("example-plan", str(case)))
        path = Path(temp) / "plan.json"
        path.write_text(json.dumps(plan), encoding="utf-8")
        preview = run("apply", str(case), str(path))
        assert "PREVIEW ONLY" in preview and case_file.read_bytes() == before
        invalid = dict(plan, command="NOT-AN-EXECUTABLE-REQUEST")
        bad = Path(temp) / "bad.json"
        bad.write_text(json.dumps(invalid), encoding="utf-8")
        run("apply", str(case), str(bad), "--approve", expected=2)
        assert case_file.read_bytes() == before, "Invalid plan changed persisted state"
        output = run("apply", str(case), str(path), "--approve")
        assert "missing target" in output and "synthetic hang records" in output
        after = case_file.read_bytes()
        current = json.loads(after)
        assert current["revision"] == 2 and current["appliedPlanIds"] == [plan["planId"]]
        run("apply", str(case), str(path), "--approve", expected=2)
        assert case_file.read_bytes() == after, "Replay changed persisted state"
        assert "revision: 2" in run("report", str(case))
        print("PASS CLI: start, export, preview/no mutation, invalid-plan/no mutation, approved follow-up, replay/no mutation, fresh-process report")
        print("Synthetic observations only; no Windows system collectors or AI service called.")


if __name__ == "__main__":
    main()
