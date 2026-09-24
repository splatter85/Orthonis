# Current task

Document ID: `orthonis.doc.current-task`.

## State: idle after OFC5 publication

The OFC5 Windows pilot is complete at its bounded owner-PC boundary. Its documentation-only branch was merged through [PR #2](https://github.com/splatter85/Orthonis/pull/2) into `main` at `079bc1bb3b3d4dd4ccb5388c49d021c4e548188e`, read back from GitHub. There is no selected work block on `main`. The owner also authorized the recommended synthetic EXP-002 evaluation on 2026-09-24; it needs a separate scoped selection before execution.

## Publication closeout

PR #2's checked head was `c5fe7e05715bc68d984c9135009f72651e8f2942`. GitHub Actions run `35966218291` passed Ubuntu job `107525264970` and Windows job `107525265153` at that head. Local repository consistency passed with 50 bindings and 15 Markdown owners; all 24 Python repository tests and `git diff --check` passed. The public diff was reviewed for home paths, private case paths and private case fields. The merge did not change product source or release/deploy the application.

## Completed boundary and evidence

The selected host reported Windows 25H2 build `26200.9457`, x64, with the repository-pinned .NET SDK `10.0.401`. `dotnet build Orthonis.slnx --configuration Release` passed with zero warnings and zero errors. The executable harness reported 53 foundation checks, 110 cumulative foundation/OFC3 checks and 68 OFC4 controlled checks passed. The synthetic CLI smoke passed. Native OFC3 and OFC4 smoke lanes passed on this PC without Run/event-log writes, target execution or live artifact upload.

Two distinct ignored private cases were created. Startup observed complete bounded reads for 17 current-user and four local-machine Run registrations, retained 21 opaque target rows and produced no attention finding. One approved opaque target inspection revalidated the registration and returned `Unsupported` rather than guessing or executing it. Reliability remained honestly `Limited`/`Partial`: it examined 65 records, retained 64, recorded six unknown envelopes and zero unparsed records. A second bounded read advanced the case while cross-query deduplication kept 64 distinct retained event records; these are not incident or crash counts.

Both local plans previewed with exit 0 and no byte change, then approved with exit 0 and advanced their separate cases from revision 1 to revision 2 with one applied-plan ID each. Replaying either plan was refused with exit 2 and preserved case bytes. Fresh-process reloads preserved both revision-2 cases. Controlled source-drift, timeout and cancellation regressions passed; no Windows state was changed merely to manufacture a live drift or cancellation condition, so an actual owner-PC drift/cancellation event remains unobserved.

## Privacy/export decision

Live export remains blocked. `report` and `example-plan` were each refused with exit 2 for both live cases. The pilot exposed incomplete Reliability coverage and local summaries that remain working views rather than reviewed minimized exports; no field-level projection or leakage-test suite was selected. No model/provider call, upload, dump access, raw event message, provider string, registry command, path or live case artifact was published.

The private case and plan files remain under ignored `.local/` paths on this PC. They are local working evidence, not a health certificate or public handoff. Empty, complete-looking or finding-free bounded observations do not establish PC health.

## Delivery and restart

OFC5 changed documentation only; product code did not change. The private cases and plans remain ignored on the selected PC. EXP-002, UI, minimized live export, broader collectors, repairs, release and deployment each require their own selected scope; the OFC5 merge grants none of them.
