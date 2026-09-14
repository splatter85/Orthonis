"""Repository-validator regressions, not Windows product tests."""
from __future__ import annotations
import importlib.util
import json
import shutil
import tempfile
import unittest
from pathlib import Path

SOURCE = Path(__file__).resolve().parents[1]
SPEC = importlib.util.spec_from_file_location("orthonis_check", SOURCE / "tools/check_repository.py")
assert SPEC is not None and SPEC.loader is not None
CHECKER = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(CHECKER)

class RepositoryChecks(unittest.TestCase):
    def setUp(self) -> None:
        temp = tempfile.TemporaryDirectory()
        self.addCleanup(temp.cleanup)
        self.root = Path(temp.name) / "repo"
        self.root.mkdir()
        catalog = json.loads((SOURCE / "eutonos.read.json").read_text(encoding="utf-8"))
        for name in ["eutonos.read.json", *catalog["source_paths"]]:
            dest = self.root / name
            dest.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(SOURCE / name, dest)

    def catalog(self) -> dict:
        return json.loads((self.root / "eutonos.read.json").read_text(encoding="utf-8"))

    def save(self, value: dict) -> None:
        (self.root / "eutonos.read.json").write_text(json.dumps(value), encoding="utf-8")

    def reject(self) -> None:
        with self.assertRaises(CHECKER.CheckError):
            CHECKER.check(self.root)

    def select(self, **changes: object) -> None:
        work = {"profile": "eutonos.tokenslang.work.v1", "work_id": "test.work", "goal": "Check routing only", "status": "selected", "required": [{"scope_id": "orthonis.repo", "namespace": "orthonis.native", "resource_id": "orthonis.doc.agents"}]}
        work.update(changes)
        text = '# Current task\n\nDocument ID: `orthonis.doc.current-task`.\n\n```tokenslang-work\n' + json.dumps(work) + '\n```\n'
        (self.root / "docs/CURRENT_TASK.md").write_text(text, encoding="utf-8")

    def test_real_repository(self) -> None:
        self.assertGreaterEqual(CHECKER.check(SOURCE)["bindings"], 14)

    def test_selected_work(self) -> None:
        self.select()
        self.assertEqual(CHECKER.check(self.root)["work"], "selected")

    def test_idle_has_no_fake_selection(self) -> None:
        (self.root / "docs/CURRENT_TASK.md").write_text('# Current task\n\nDocument ID: `orthonis.doc.current-task`.\n\nNo active work selected.\n', encoding="utf-8")
        self.assertEqual(CHECKER.check(self.root)["work"], "idle")

    def test_duplicate_json_key(self) -> None:
        p = self.root / "eutonos.read.json"
        p.write_text('{"profile":"bad",' + p.read_text(encoding="utf-8")[1:], encoding="utf-8")
        self.reject()

    def test_nonfinite_json(self) -> None:
        p = self.root / "eutonos.read.json"
        p.write_text(p.read_text(encoding="utf-8").replace('"orthonis.repo"', 'NaN', 1), encoding="utf-8")
        self.reject()

    def test_unsupported_profile(self) -> None:
        c = self.catalog(); c["profile"] = "future.v999"; self.save(c); self.reject()

    def test_duplicate_id(self) -> None:
        c = self.catalog(); c["bindings"][1]["resource_id"] = c["bindings"][0]["resource_id"]; self.save(c); self.reject()

    def test_case_collision(self) -> None:
        c = self.catalog(); c["bindings"][1]["path"] = "readme.md"; self.save(c); self.reject()

    def test_missing_binding_file(self) -> None:
        (self.root / "docs/PROJECT.md").unlink(); self.reject()

    def test_escape_path(self) -> None:
        c = self.catalog(); c["bindings"][0]["path"] = "../outside.md"; self.save(c); self.reject()

    def test_absolute_path(self) -> None:
        c = self.catalog(); c["bindings"][0]["path"] = "/tmp/outside.md"; self.save(c); self.reject()

    def test_excluded_source(self) -> None:
        c = self.catalog(); c["excluded_roots"].append("docs"); self.save(c); self.reject()

    def test_private_binding(self) -> None:
        c = self.catalog(); c["bindings"][0]["visibility"] = "private"; self.save(c); self.reject()

    def test_missing_owner(self) -> None:
        c = self.catalog(); c["owners"] = {}; self.save(c); self.reject()

    def test_wrong_reference_scope(self) -> None:
        c = self.catalog(); c["owners"]["current_task"]["scope_id"] = "different.repo"; self.save(c); self.reject()

    def test_missing_required_work_ref(self) -> None:
        self.select(required=[{"scope_id": "orthonis.repo", "namespace": "orthonis.native", "resource_id": "does.not.exist"}]); self.reject()

    def test_completed_work_not_selected(self) -> None:
        self.select(status="completed"); self.reject()

    def test_competing_work_blocks(self) -> None:
        self.select()
        p = self.root / "docs/CURRENT_TASK.md"; p.write_text(p.read_text(encoding="utf-8") * 2, encoding="utf-8"); self.reject()

    def test_unclosed_work_block(self) -> None:
        self.select()
        p = self.root / "docs/CURRENT_TASK.md"; p.write_text(p.read_text(encoding="utf-8").rsplit('```', 1)[0], encoding="utf-8"); self.reject()

    def test_stale_heading_link(self) -> None:
        p = self.root / "README.md"; p.write_text(p.read_text(encoding="utf-8") + '\n[Missing](docs/PROJECT.md#not-present)\n', encoding="utf-8"); self.reject()

    def test_missing_local_link(self) -> None:
        p = self.root / "README.md"; p.write_text(p.read_text(encoding="utf-8") + '\n[Missing](missing.md)\n', encoding="utf-8"); self.reject()

    def test_document_id_mismatch(self) -> None:
        p = self.root / "AGENTS.md"; p.write_text(p.read_text(encoding="utf-8").replace('orthonis.doc.agents', 'wrong.id'), encoding="utf-8"); self.reject()

    def test_symlink_source(self) -> None:
        p = self.root / "docs/PROJECT.md"
        other = self.root / "docs/COPY.md"
        p.rename(other)
        try:
            p.symlink_to(other.name)
        except (OSError, NotImplementedError):
            self.skipTest("symlink creation unavailable on this host")
        self.reject()

    def test_duplicate_source_inventory(self) -> None:
        c = self.catalog(); c["source_paths"].append(c["source_paths"][0]); self.save(c); self.reject()

if __name__ == "__main__":
    unittest.main()
