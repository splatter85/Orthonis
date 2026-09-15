#!/usr/bin/env python3
"""OFC4 fresh-process controlled loop and no-dump Windows native read. No live artifact upload."""
import hashlib
import json
import os
from pathlib import Path
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parents[1]
BASE = ["dotnet", "run", "--project", "tests/Orthonis.Tests", "--configuration", "Release", "--no-build", "--"]

def call(*args, expected=0):
    result = subprocess.run(BASE + list(args), cwd=ROOT, capture_output=True, text=True, encoding="utf-8", timeout=30)
    if result.returncode != expected:
        raise RuntimeError("OFC4 subprocess failed; output withheld (possible private evidence)")
    if any(marker in result.stdout + result.stderr for marker in ["PRIVATE-OFC4-PROVIDER", "PRIVATE-PAYLOAD", "PRIVATE-COMPUTER"]):
        raise RuntimeError("OFC4 privacy sentinel leaked")
    return result.stdout

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

with tempfile.TemporaryDirectory(prefix="orthonis-ofc4-") as temp:
    directory = Path(temp) / "case"
    call("--ofc4-cli", "start-reliability", str(directory))
    case = directory / "case.json"
    before = digest(case)
    call("--ofc4-cli", "report", str(directory), expected=2)
    call("--ofc4-cli", "example-plan", str(directory), expected=2)
    plan = Path(temp) / "plan.json"
    plan.write_text(call("--ofc4-cli", "local-plan", str(directory)), encoding="utf-8")
    call("--ofc4-cli", "apply", str(directory), str(plan))
    assert digest(case) == before, "Preview/export refusal mutated the case"
    call("--ofc4-cli", "apply", str(directory), str(plan), "--approve")
    saved = json.loads(case.read_text(encoding="utf-8"))
    assert saved["revision"] == 2 and len(saved["appliedPlanIds"]) == 1
    after = digest(case)
    call("--ofc4-cli", "apply", str(directory), str(plan), "--approve", expected=2)
    assert digest(case) == after, "Replay mutated the case"
    summary = call("--ofc4-cli", "summary", str(directory))
    assert "Distinct retained event records across history: 1." in summary
print("PASS: OFC4 fresh-process create, preview, approval, reload, deduplication, privacy and replay.")
if os.name == "nt":
    call("--ofc4-native")
    print("PASS: bounded native Windows Application read; no log details or artifacts uploaded.")
else:
    with tempfile.TemporaryDirectory(prefix="orthonis-ofc4-unsupported-") as temp:
        result = subprocess.run(["dotnet", "run", "--project", "src/Orthonis.Cli", "--configuration", "Release", "--no-build", "--",
                                 "start-reliability", str(Path(temp) / "case")], cwd=ROOT, capture_output=True, timeout=30)
        assert result.returncode == 2 and not (Path(temp) / "case" / "case.json").exists()
    print("PASS: unsupported host refused without fixture fallback; native read skipped.")
