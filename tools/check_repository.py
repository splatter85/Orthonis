#!/usr/bin/env python3
"""Read-only checks for Orthonis's repository-file profile, not native runtime certification."""
from __future__ import annotations
import argparse
import json
import posixpath
import re
import sys
from pathlib import Path, PurePosixPath
from urllib.parse import unquote, urlsplit

class CheckError(ValueError):
    """A repository record violates the supported structural contract."""

def _pairs(pairs: list[tuple[str, object]]) -> dict:
    result = {}
    for key, value in pairs:
        if key in result:
            raise CheckError(f"duplicate JSON key: {key}")
        result[key] = value
    return result

def _constant(value: str) -> None:
    raise CheckError(f"non-finite JSON value: {value}")

def _json(text: str) -> dict:
    try:
        result = json.loads(text, object_pairs_hook=_pairs, parse_constant=_constant)
    except (ValueError, RecursionError) as exc:
        raise CheckError(f"invalid JSON: {exc}") from exc
    if not isinstance(result, dict):
        raise CheckError("JSON record must be an object")
    return result

def _text(value: object, label: str) -> str:
    if not isinstance(value, str) or not value.strip():
        raise CheckError(f"{label} must be a nonempty string")
    return value

def _list(value: object, label: str) -> list:
    if not isinstance(value, list):
        raise CheckError(f"{label} must be an array")
    return value

def _relative(value: object) -> str:
    value = _text(value, "path")
    if (value.startswith("/") or "\\" in value or ":" in value
            or any(ord(c) < 32 for c in value)
            or any(p in ("", ".", "..") for p in value.split("/"))):
        raise CheckError(f"unsafe or noncanonical relative path: {value!r}")
    return value

def _file(root: Path, value: object, excluded: list[str]) -> Path:
    name = _relative(value)
    if any(name == p or name.startswith(p + "/") for p in excluded):
        raise CheckError(f"path is excluded: {name}")
    path = root
    for part in PurePosixPath(name).parts:
        # Check exact spelling even on case-insensitive filesystems.
        if not path.is_dir() or part not in {p.name for p in path.iterdir()}:
            raise CheckError(f"missing or case-mismatched source: {name}")
        path = path / part
        if path.is_symlink():
            raise CheckError(f"symlink is not an admitted source: {name}")
    if not path.is_file() or not path.resolve().is_relative_to(root):
        raise CheckError(f"not a contained regular file: {name}")
    return path

def _read(path: Path) -> str:
    try:
        return path.read_text(encoding="utf-8")
    except (OSError, UnicodeError) as exc:
        raise CheckError(f"cannot read UTF-8 source {path.name}: {exc}") from exc

def _ref(value: object, catalog: dict, bindings: dict) -> str:
    if not isinstance(value, dict) or set(value) != {"scope_id", "namespace", "resource_id"}:
        raise CheckError("reference requires scope_id, namespace, and resource_id only")
    if value["scope_id"] != catalog["scope_id"] or value["namespace"] != catalog["namespace"]:
        raise CheckError("reference scope/namespace does not match the catalog")
    rid = _text(value["resource_id"], "resource_id")
    if rid not in bindings:
        raise CheckError(f"unresolved stable reference: {rid}")
    return rid

def _anchors(text: str) -> set[str]:
    anchors = set(re.findall(r'<a\s+id=["\']([^"\']+)["\']', text))
    counts: dict[str, int] = {}
    text = re.sub(r"(?ms)^```[^\n]*\n.*?^```[ \t]*$", "", text)
    for heading in re.findall(r"(?m)^#{1,6}[ \t]+(.+?)[ \t]*#*[ \t]*$", text):
        slug = re.sub(r"[^\w\- ]", "", heading.lower()).replace(" ", "-")
        count = counts.get(slug, 0)
        counts[slug] = count + 1
        anchors.add(slug if count == 0 else f"{slug}-{count}")
    return anchors

def _links(root: Path, source: str, text: str, excluded: list[str]) -> None:
    # Inline links/images and ATX anchors used here, not a complete Markdown parser.
    for target in re.findall(r"!?\[[^\]\n]*\]\(([^)\s]+)\)", text):
        parts = urlsplit(target)
        if parts.scheme in {"https", "http", "mailto"}:
            continue
        if parts.scheme or parts.netloc or parts.query:
            raise CheckError(f"unsupported local link in {source}: {target}")
        relative = unquote(parts.path)
        if relative.startswith("/") or "\\" in relative:
            raise CheckError(f"non-relative local link in {source}: {target}")
        name = posixpath.normpath(posixpath.join(posixpath.dirname(source), relative)) if relative else source
        path = _file(root, name, excluded)
        if parts.fragment and unquote(parts.fragment) not in _anchors(_read(path)):
            raise CheckError(f"missing local anchor in {source}: {target}")

def check(root: Path) -> dict[str, object]:
    root = root.resolve(strict=True)
    catalog = _json(_read(_file(root, "eutonos.read.json", [])))
    expected = {"profile", "repository_id", "scope_id", "namespace", "owners", "excluded_roots", "source_paths", "bindings"}
    if set(catalog) != expected or catalog.get("profile") != "eutonos.tokenslang.catalog.v1":
        raise CheckError("unsupported catalog shape/profile for this deployment")
    for field in ("repository_id", "scope_id", "namespace"):
        _text(catalog[field], field)
    excluded = [_relative(p) for p in _list(catalog["excluded_roots"], "excluded_roots")]
    bindings: dict[str, dict] = {}
    paths: dict[str, str] = {}
    for entry in _list(catalog["bindings"], "bindings"):
        fields = {"resource_id", "path", "kind", "label", "visibility", "state"}
        if not isinstance(entry, dict) or set(entry) != fields:
            raise CheckError("unsupported binding shape; review profile changes explicitly")
        rid = _text(entry["resource_id"], "resource_id")
        if rid in bindings:
            raise CheckError(f"duplicate stable ID: {rid}")
        name = _relative(entry["path"])
        if name.casefold() in paths:
            raise CheckError(f"duplicate/case-colliding binding path: {name}")
        paths[name.casefold()] = name
        if entry["kind"] not in {"docs", "source"} or entry["visibility"] != "public" or entry["state"] != "active":
            raise CheckError(f"unsupported kind or visibility/state for {rid}")
        _text(entry["label"], "label")
        _file(root, name, excluded)
        bindings[rid] = entry
    if not bindings:
        raise CheckError("catalog has no bindings")
    source_paths = _list(catalog["source_paths"], "source_paths")
    if any(not isinstance(p, str) for p in source_paths):
        raise CheckError("source_paths must contain strings")
    if len(set(source_paths)) != len(source_paths):
        raise CheckError("duplicate source path")
    for name in source_paths:
        _file(root, name, excluded)
    if set(source_paths) != set(paths.values()):
        raise CheckError("source inventory and binding paths differ")
    owners = catalog["owners"]
    if not isinstance(owners, dict) or set(owners) != {"current_task"}:
        raise CheckError("one current_task owner is required")
    owner_id = _ref(owners["current_task"], catalog, bindings)
    if bindings[owner_id]["kind"] != "docs":
        raise CheckError("current_task must refer to a Docs owner")
    owner_text = _read(_file(root, bindings[owner_id]["path"], excluded))
    blocks = re.findall(r"(?ms)^```tokenslang-work[ \t]*\n(.*?)^```[ \t]*$", owner_text)
    openings = len(re.findall(r"(?m)^```tokenslang-work(?:\s|$)", owner_text))
    if openings != len(blocks) or len(blocks) > 1:
        raise CheckError("malformed or competing selected work blocks")
    state = "idle"
    if blocks:
        work = _json(blocks[0])
        required = {"profile", "work_id", "goal", "status", "required"}
        if set(work) - (required | {"optional"}) or not required <= set(work):
            raise CheckError("unsupported selected work shape")
        if work["profile"] != "eutonos.tokenslang.work.v1" or work["status"] != "selected":
            raise CheckError("unsupported work profile/status; omit completed selections")
        _text(work["work_id"], "work_id")
        _text(work["goal"], "goal")
        refs = _list(work["required"], "required work references")
        if not refs:
            raise CheckError("selected work has no required material")
        for ref in refs:
            _ref(ref, catalog, bindings)
        for hint in _list(work.get("optional", []), "optional work hints"):
            if not isinstance(hint, dict) or set(hint) != {"target", "why"}:
                raise CheckError("invalid optional work hint")
            _ref(hint["target"], catalog, bindings)
            _text(hint["why"], "optional hint reason")
        state = "selected"
    markdown = 0
    for rid, entry in bindings.items():
        if entry["path"].endswith(".md"):
            markdown += 1
            text = _read(_file(root, entry["path"], excluded))
            if entry["path"] != "README.md" and f"Document ID: `{rid}`." not in text:
                raise CheckError(f"document identity differs from binding: {entry['path']}")
            _links(root, entry["path"], text, excluded)
    return {"bindings": len(bindings), "markdown": markdown, "work": state}

def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        result = check(args.root)
    except (CheckError, OSError, RuntimeError, TypeError) as exc:
        print(f"FAIL: {exc}", file=sys.stderr)
        return 1
    print(f"PASS: {result['bindings']} bindings; {result['markdown']} Markdown owners; work={result['work']}")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
