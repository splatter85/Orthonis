# Project health and verification

Document ID: `orthonis.doc.health`.

## Repeatable checks

Python 3.12 or later is the selected baseline for this repository's standard-library-only check tooling. From the repository root:

```sh
python tools/check_repository.py
python -m unittest discover -s tests -v
```

An alternate root can be checked without modifying it:

```sh
python tools/check_repository.py --root /path/to/Orthonis
```

The checker reads the explicit catalog and Markdown owners. It checks the repository-local catalog shape, unique stable IDs, case-distinct paths, source/owner routing, supported work-block shape, exact scoped references, local link targets/anchors, and exclusion/path boundaries. It does not fetch external URLs, execute imported data, or repair files.

The tests exercise valid and idle pickup plus deliberately malformed, ambiguous, missing, escaping, symlinked, and stale-reference cases. A successful check establishes these structural properties only. This independently authored checker is not the upstream EUTONOS runtime validator, a security sandbox, a secret scanner, or proof of all TokenSlang profiles.

## Publication verification

Inspect the proposed paths/diff before writing. After publication, fetch the exact committed tree and compare blob identities with the tested bytes, then read boot/task/adoption/checkpoint owners through the connector. Record the resulting source commit in the evidence owner. A later documentation-only closeout must also be checked and read back.

The containing Git commit identifies the version of an evidence record. Do not embed an invented future commit into itself. A historical checkpoint may pin the prior verified source commit; Git history supplies the record's own publication identity.

## Current evidence owner

[EUTONOS adoption](EUTONOS_ADOPTION.md) records OED1's observed environment, commands/results, publication/readback boundary, and remaining limits. Do not duplicate that mutable ledger here. Re-run the commands after relevant changes instead of assuming the initial result covers later work.

## Acceptance boundaries

No .NET build, C# compiler, Windows integration test, application UI, installer, privileged operation, model connection, EUTONOS native installation, RNAVL sweep, or native RAM capture is established by these checks. There is no GitHub Actions workflow in OED1, and publication does not trigger one created by this setup.

A future application slice must select its own test/toolchain commands. Report separately: portable-core tests, Windows-runner checks, and actual Windows PC verification. Synthetic fixtures must be labeled and must not be treated as real-system observations. An unrun or unsupported check remains unrun or unsupported, not healthy.

## Review priorities

Review owner authority, actual versus planned capabilities, source provenance, and public-data safety as well as mechanical links. Confirm that an idle board does not silently select the next product task. For future repair work, include negative and interruption tests before claiming safe execution or recovery.
