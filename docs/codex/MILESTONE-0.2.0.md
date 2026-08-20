# Codex autonomous milestone task — 0.2.0

Implement the complete milestone:

`0.2.0 — Status, reviews and blockers`

Work autonomously until this milestone is fully implemented and technically verified.

Do **not** begin `0.3.0`.

## 1. Governing instructions

Before changing code:

1. read `/AGENTS.md`;
2. read `docs/implementation/DECISION-0.2.0-PROJECT-STEERING-SEMANTICS.md`;
3. read the relevant Lastenheft, Pflichtenheft, architecture, database, UI, testing and roadmap documents referenced by `AGENTS.md`;
4. inspect the current `main` state, Git history and `v0.1.0` evidence/release state.

If the newly approved decision note conflicts with an older implementation-near status statement, treat the decision note and updated `AGENTS.md` as the approved `0.2.0` clarification unless a higher-level requirement would be weakened. In that case stop and report the conflict.

## 2. Git start state

Start from clean, current `main`.

Verify:

```bash
git status -sb
git fetch
git pull --ff-only
```

Create and use:

```text
codex/0.2.0
```

Do not force-push or rewrite shared history.

You are authorized for this milestone to:

- create the milestone branch;
- implement autonomous milestone work;
- create logically coherent commits after their relevant gates pass;
- push `codex/0.2.0` to origin;
- use `gh` for read-only verification of GitHub workflow state.

Do not merge to `main`, create the final release/tag, or start `0.3.0`. Stop at the `0.2.0` milestone boundary for user review.

## 3. Development priority

Optimize for **practical usability and fast delivery**, not theoretical perfection.

The code must be correct, understandable, maintainable, recoverable and tested, but it does not need to be maximally optimized.

Do not spend milestone time on:

- micro-optimization;
- speculative caching;
- generic workflow frameworks;
- premature parallelism;
- broad refactoring without functional benefit;
- performance work without measured evidence.

Record material optimization opportunities as technical debt and continue.

A deliberate optimization/refactoring loop is planned after `0.8.0`.

## 4. Source readability

Follow the `AGENTS.md` XML/commenting policy.

In particular:

- use English identifiers;
- use English XML documentation for public domain/application contracts where semantics matter;
- use `//` comments to explain non-obvious **why**;
- comment derived review logic, due-date logic, migration decisions, archive/activity independence, concurrency and blocker-resolution behaviour;
- do not fill trivial methods with comments that merely restate the C# syntax.

The source should remain teachable and understandable to a developer who did not write it.

## 5. Required `0.2.0` capability

Implement the approved project-steering model without collapsing concepts into a generic `Status`.

### 5.1 Project phase

Controlled values only:

- Idea / Idee
- Preparation / Vorbereitung
- Execution / Durchführung
- Validation / Validierung
- Closure / Abschluss

A version-controlled JSON resource may be used for display labels/help/sort order if it simplifies the implementation, but users must not be able to add arbitrary values.

Do not create a generic runtime-extensible status system.

### 5.2 Activity state

Controlled values only:

- NotStarted / Noch nicht begonnen
- Active / Aktiv
- Paused / Pausiert
- Completed / Abgeschlossen
- Cancelled / Abgebrochen

Keep this independent from Project Phase and Archive.

Do not automatically archive Completed projects.

Do not silently change activity state when an archived project is reactivated.

### 5.3 Review

Persist:

- `LastReviewedAtUtc?`
- `NextReviewDueAtUtc?`

Derive readable review freshness from facts and current time.

Expected derived values:

- NotScheduled
- NotReviewed
- Current
- DueToday
- Overdue

Do not persist an authoritative review-status snapshot.

Provide a practical action such as **Als geprüft markieren** that updates the relevant facts safely.

Do not build a full review-history subsystem.

### 5.4 Blockers

Implement blocker details, not just a Boolean.

Minimum:

- Id
- ProjectId
- Summary
- Details?
- CreatedAtUtc
- ResolvedAtUtc?
- ResolutionNote?

The UI must allow users to:

- see open blockers for a project;
- inspect blocker details;
- add a blocker;
- resolve a blocker with an optional resolution note;
- retain resolved blocker history sufficiently for the current project context.

Do not turn blockers into tasks.

### 5.5 Target date and due-date indication

Add optional `TargetDate` where it belongs in the Project model/specification.

Derive the due-date indication:

- no date → neutral;
- >14 calendar days remaining → green + `im Plan`;
- 1–14 days remaining → yellow + `bald fällig`;
- today → yellow + `heute fällig`;
- past target → red + `überfällig`;
- Completed/Cancelled → neutral.

Colour/icon must never be the only information.

Do not persist the traffic-light state.

Do not claim that green means overall project health or measured progress.

### 5.6 Needs attention

Derive `NeedsAttention` from current facts.

For `0.2.0`, attention is required when:

- review is overdue; or
- at least one blocker is open; or
- target date is overdue.

Expose the concrete reason(s).

### 5.7 Archive

Keep the accepted reversible archive/reactivation behaviour separate from all new status concepts.

No hard delete.

## 6. Menu and help

Add a simple, professional native WinForms application menu.

Every major user function must be reachable through the menu or an equally obvious primary UI control.

Use the approved decision note as the semantic guide for menu content.

At minimum cover suitable access to:

- new/edit project;
- archive/reactivate;
- mark reviewed;
- export;
- backup;
- restore;
- refresh/view options;
- attention view/filter;
- help/glossary/about;
- exit.

Add local/offline help:

- tooltips for potentially confusing fields/actions;
- F1 help;
- concise local glossary/help pages;
- explanations for Project Phase vs Activity State, Review Freshness, Blocker and Archive.

Do not add a WebView/help-server framework merely for this milestone. Prefer the smallest robust local solution.

## 7. UI usability

`0.2.0` must remain a usable Windows Forms application, not only a backend/domain change.

Use native controls first.

Preserve:

- keyboard usability;
- logical tab order;
- accessible names/descriptions;
- text in addition to colour;
- required DPI behaviour;
- German user-facing terminology.

Avoid Ribbon, charts, elaborate custom themes and UI framework dependencies.

## 8. Database evolution

Treat the accepted `0.1.0` database as real user data.

Implement an additive EF Core migration.

Required migration evidence:

```text
0.1.0 database
→ 0.2.0 migration
→ existing projects preserved
→ phase/activity/review/target-date/blocker functionality usable
→ save
→ full application/service reconstruction
→ reopen
→ existing and new data preserved
→ backup/restore still valid
```

Do not delete/recreate the database as an upgrade strategy.

Keep migration/recovery tests using real SQLite.

## 9. Tests

Add tests according to risk, not test-count vanity.

Cover at least as applicable:

- finite Project Phase values;
- finite Activity State values;
- phase/activity independence;
- archive/activity independence;
- review-freshness derivation;
- review due-today/overdue boundaries;
- target-date 14-day boundary;
- completed/cancelled neutral due state;
- NeedsAttention reasons;
- blocker creation/resolution/history;
- invalid blocker data;
- concurrency/revision behaviour when steering data changes;
- `0.1.0 → 0.2.0` migration;
- reopen/round-trip;
- backup/restore after migration;
- architecture boundaries;
- menu/help-related UI contracts where meaningfully automatable.

For date-dependent rules, make time controllable/testable. Do not make tests depend on the real wall clock.

## 10. Work method

Break the milestone into the smallest sensible sequence yourself.

For every logical step:

1. inspect relevant source/specifications;
2. implement only current milestone scope;
3. run focused tests;
4. debug normal implementation/compiler/test failures autonomously;
5. run the relevant wider gate;
6. inspect the diff;
7. commit only after the step is coherent and verified.

Do not interrupt the user for:

- class names;
- private method structure;
- straightforward UI layout choices;
- small reversible refactorings;
- test organization;
- normal compiler/test failures.

Stop only for the strategic blockers defined in `AGENTS.md`.

## 11. Quality gates

Before declaring the milestone complete, run at minimum:

```bash
dotnet clean Sasd.Pims.slnx -c Release
dotnet restore Sasd.Pims.slnx
dotnet build Sasd.Pims.slnx -c Release --no-restore
dotnet test Sasd.Pims.slnx -c Release --no-build
git diff --check
git status -sb
```

Also run:

- NuGet vulnerability check including transitive packages;
- relevant architecture tests;
- migration tests with real SQLite;
- backup/restore/recovery tests;
- release packaging and SHA-256 verification;
- a Windows smoke test where safely automatable;
- GitHub Quality workflow after pushing the milestone branch.

If a gate fails, diagnose and repair it autonomously unless the failure requires a strategic decision.

## 12. Documentation and evidence

Update only documentation made stale by actual implementation.

Record:

- schema/migration facts;
- direct dependency changes;
- material technical decisions;
- technical debt deliberately deferred;
- release/evidence results.

Do not write another general architecture volume.

Create/update milestone evidence under the existing release/evidence convention.

## 13. Milestone completion

When `0.2.0` is complete:

- push the clean `codex/0.2.0` branch;
- verify local and remote branch heads match;
- verify the GitHub Quality workflow;
- leave the working tree clean;
- do not merge to `main`;
- do not create `v0.2.0`;
- do not delete the milestone branch;
- do not begin `0.3.0`.

Report:

- all commits;
- actual user-visible capabilities;
- schema/migration state;
- build/test totals;
- GitHub CI result;
- release artifact path, size and SHA-256;
- direct dependency changes;
- vulnerability result;
- consciously deferred optimization/refactoring items;
- technical debt;
- known defects;
- manual checks still required;
- any decisions needed before `0.3.0`.

Then stop and wait for user review.
