# AGENTS.md — SASD PIMS

## Purpose

This file contains durable repository instructions for coding agents working on **SASD PIMS**.

SASD PIMS is already past the general architecture-planning phase. The agent's default job is to implement the **currently active milestone** in small, verifiable steps while respecting the approved requirements, architecture, data, UI and test baselines.

Do not redesign the product merely because another design is possible.

---

## 1. Governing project documents

Before changing product code, read the documents relevant to the requested work.

Primary sources:

1. `docs/requirements/lastenheft-v0.1/documents/SASD-PIMS-Lastenheft-v0.1.md`
   - functional intent, priorities, acceptance intent and product boundaries;
2. `docs/specification/pflichtenheft-v0.1/documents/SASD-PIMS-Pflichtenheft-v0.1.md`
   - technical interpretation and implementation constraints;
3. `docs/architecture/baseline-v1.0/documents/SASD-PIMS-Software-Architecture-Document-v1.0.md`
   - approved architecture, dependency rules and quality gates;
4. `docs/baseline/BASELINE.md`
   - implementation approval, reconciliations and open-decision rules;
5. `docs/database/SASD-PIMS-Datenbankspezifikation-v0.1.md`
   - persistence baseline for the current early releases;
6. `docs/ui/SASD-PIMS-UI-UX-Spezifikation-v0.1.md`
   - UI behaviour and accessibility/DPI baseline;
7. `docs/testing/SASD-PIMS-Test-und-Abnahmespezifikation-v0.1.md`
   - verification and acceptance rules;
8. `docs/ROADMAP.md`
   - compact release sequencing;
9. `docs/planning/roadmap-v0.1/documents/SASD-PIMS-Entwicklungsroadmap-und-Dokumentationsplan-v0.1.md`
   - detailed release roadmap;
10. `docs/implementation/VERTICAL-SLICE-0.0.1.md`
   - current vertical-slice contract while `0.0.1-internal` is active.

Use `docs/README.md` as the documentation index.

### Precedence and conflicts

- The Lastenheft governs **what** the product must achieve.
- The Pflichtenheft governs the technical interpretation unless it reduces or contradicts the Lastenheft.
- Architecture v1.0 governs system boundaries, dependency directions and architectural quality rules.
- `docs/baseline/BASELINE.md` records explicit reconciliations.
- The roadmap governs **sequencing**, not the priority of requirements.
- Implementation-near database/UI/test specifications govern the current slice where they do not contradict higher-level sources.
- Code does not silently override a governing document.

If two governing sources materially conflict and the baseline does not resolve the conflict:
1. stop the affected implementation;
2. report the exact conflict and impacted files/requirements;
3. propose the smallest decision needed;
4. do not invent a silent compromise.

---

## 2. Current active milestone

**Active milestone: `0.1.0 — Practical project catalog`**

`0.0.1-internal — Vertical architecture slice` has been implemented and technically verified. Its implementation evidence is recorded under:

```text
docs/releases/0.0.1-internal/EVIDENCE.md
```

Do not reopen or refactor the accepted `0.0.1-internal` implementation merely for stylistic reasons. Change it only when a `0.1.0` requirement, defect, migration need, security/recovery issue, or clearly justified simplification requires the change.

Until the user explicitly changes the active milestone or the repository baseline is updated, implement only work required for `0.1.0`.

The purpose of `0.1.0` is to make SASD PIMS **practically usable as a local project catalog**.

The user must be able to:

- start PIMS reliably;
- see existing projects in a useful project list;
- create a project;
- view and edit project master data;
- save changes;
- close and reopen the application without losing data;
- identify and select projects clearly;
- receive understandable validation feedback;
- use reversible archive/deactivate and reactivation behaviour once the corresponding domain rule is finalized for this milestone;
- continue to use the proven backup/restore path after the `0.1.0` schema migration;
- use the core catalog workflow by keyboard and on the required DPI baseline.

`0.1.0` must also prove the first real schema evolution from the accepted `0.0.1-internal` database.

Not in `0.1.0`:

- Requirement management;
- Product lifecycle/product pipeline;
- Risk, Decision, Milestone or Release management;
- status-review/blocker functionality planned for `0.2.0`;
- dashboards or charts;
- GitHub/provider APIs;
- cloud synchronization;
- plugin framework;
- generic task management;
- complex reporting;
- installer or automatic updater unless explicitly requested for a focused validation task.

### Milestone boundary rule

Never pull a later milestone feature forward merely because its design already exists in the documentation.

If a later feature is technically useful but not necessary for the active milestone, defer it.

### Default autonomy inside the active milestone

When the user authorizes autonomous milestone work, Codex may:

- determine the next open implementation step inside `0.1.0`;
- implement, build, test and debug it;
- make small reversible technical decisions that do not change product semantics or approved architecture;
- add regression tests;
- perform small local refactorings needed for the current change;
- commit logically complete changes after their relevant quality gates pass.

Codex must stop and ask only when a decision would materially change:

- product/domain semantics;
- approved architecture or technology baseline;
- data-loss/recovery guarantees;
- security posture;
- licence obligations;
- a public/portable exchange contract;
- supported platform;
- milestone scope.

Do not transition to `0.2.0` without explicit user approval.

---

## 3. Full roadmap — all planned milestones

The following milestones are the approved staged path. They are **scope boundaries**, not permission to implement ahead.

### `0.0.1-internal — Vertical architecture slice` — completed

Goal: prove that the approved architecture supports one complete, testable workflow.

Target evidence:

- application starts and shuts down cleanly;
- one minimal Project can be created and validated;
- SQLite persistence and reopen work;
- logging and controlled error handling work;
- minimal export works;
- backup and staged restore work on synthetic data;
- unit/integration/architecture tests run;
- reproducible internal artifact can be built.

### `0.1.0 — Practical project catalog`

Goal: first practically useful local project-master-data release.

Planned:

- project list;
- create/edit/view Project;
- stable Project identity;
- basic validation;
- approved archive/deactivate semantics;
- reliable restart/reopen;
- basic backup/recovery access;
- usable keyboard and DPI behaviour.

Not yet:

- full requirements model;
- broad portfolio/dashboard;
- product lifecycle;
- complex reporting.

### `0.2.0 — Status, reviews and blockers`

Goal: turn the project catalog into a lightweight project-steering instrument without becoming a task manager.

Planned:

- project status;
- review/health information;
- blockers/attention indicators;
- focused status views.

### `0.3.0 — Requirements and typed references`

Goal: establish requirement traceability and structured provenance.

Planned:

- structured requirements;
- acceptance information;
- typed references to repositories, chats, files, documents and URLs;
- safe validation/opening of referenced targets;
- manual references remain the default integration model.

No mandatory online provider API.

### `0.4.0 — Search, traceability and portable exchange`

Goal: make the growing information base reliably searchable, navigable and portable.

Planned:

- local search/filtering;
- relationship navigation;
- versioned export;
- import preview and validation;
- supported conflict handling.

### `0.5.0 — MVP`

Goal: complete and verify the mandatory MVP scope.

Focus:

- all mandatory MVP requirements implemented;
- recovery and migration confidence;
- accessibility/DPI baseline;
- realistic synthetic-data performance measurements;
- release documentation.

Important: `0.1.0` is not the MVP. The full MUST-scope is reached here.

### `0.6.0 — Governance information`

Goal: extend project governance beyond status and requirements where still justified.

Planned additions:

- milestones;
- decisions;
- risks;
- supporting views;
- traceability for those concepts.

### `0.7.0 — Minimal product relationship`

Goal: introduce the minimal domain relationship between Project and Product without turning PIMS into a full product-management suite.

Keep Project and Product as distinct domain concepts.

### `0.8.0 — Feature-complete beta`

Goal: freeze the planned 1.0 feature set.

After feature freeze, priority moves to:

- defects;
- usability;
- migration;
- documentation;
- accessibility;
- performance;
- recovery;
- upgradeability.

Do not introduce new planned 1.0 features after the freeze without explicit roadmap change.

After feature freeze, run a deliberate optimization/refactoring loop based on actual measurements and accumulated technical debt before the release-candidate phase. Optimize measured bottlenecks and unnecessarily complex code; do not perform speculative rewrites.

### `0.9.0 — Release candidate`

Goal: release qualification only.

Rules:

- no planned new features;
- installer/package/upgrade/recovery qualification as applicable;
- acceptance evidence;
- regression fixing;
- release notes and operational documentation.

### `1.0.0 — Stable release`

Goal: promote a successfully accepted release candidate to the stable first release.

Do not slip unvalidated late features into 1.0.

### Optional post-1.0 candidates — not committed milestones

Only after measured need and explicit decision:

- richer reporting;
- charts;
- Markdown preview;
- provider metadata adapters such as GitHub;
- enhanced import/export formats;
- additional reusable SASD UI components;
- update automation.

The following are **not implicit commitments**:

- cloud synchronization;
- team collaboration platform;
- generic Kanban;
- arbitrary plugin system;
- broad task-management replacement.

---

## 4. Product and architectural boundaries

PIMS is the authoritative local project-information system.

It must not silently become:

- a general task manager;
- a Jira replacement;
- a Prompt Manager replacement;
- a generic notes application;
- a mandatory cloud/multi-user system;
- a generic Kanban product.

The Product Pipeline is a PIMS view/projection, not a separate product unless a future approved decision says otherwise.

Keep these domain concepts distinct:

- Project;
- Product;
- Requirement;
- Task;
- Milestone;
- Release;
- Decision;
- Risk.

Do not merge concepts merely to simplify a database schema.

---

## 5. Technology baseline

Unless a governing document is explicitly changed:

- language: C#;
- target runtime: .NET 10;
- desktop UI: Windows Forms;
- supported product platform baseline: Windows 11 x64;
- persistence: SQLite;
- ORM: EF Core;
- architecture: small modular monolith;
- operation: local-first / offline-first.

Do not replace these technologies during routine implementation.

A proposal to replace a baseline technology is a strategy/architecture decision and must be escalated before implementation.

---

## 6. Production project structure and dependency rules

Approved production projects:

```text
src/
├── Sasd.Pims.Domain/
├── Sasd.Pims.Application/
├── Sasd.Pims.Infrastructure/
└── Sasd.Pims.WinForms/
```

Do not add another production project unless it has an immediate, concrete responsibility that cannot reasonably live in these four projects.

### Responsibilities

#### `Sasd.Pims.Domain`

Contains:

- domain entities;
- value objects;
- domain invariants;
- domain behaviour;
- domain-specific result/error concepts where appropriate.

Must not depend on:

- WinForms;
- EF Core;
- SQLite;
- filesystem APIs;
- concrete external providers;
- `Sasd.Pims.Infrastructure`.

Keep the Domain free of persistence/UI attributes used only for framework convenience.

#### `Sasd.Pims.Application`

Contains:

- use cases;
- commands/queries or equivalent application operations;
- application ports/interfaces;
- orchestration;
- validation coordination;
- neutral application result models.

Must not depend on WinForms.

Application code should not know concrete SQLite/EF Core implementation details.

#### `Sasd.Pims.Infrastructure`

Contains:

- EF Core persistence;
- SQLite configuration;
- repository/port implementations;
- filesystem adapters;
- export/backup/restore implementations;
- concrete external-provider adapters when a future milestone explicitly requires them.

Infrastructure may depend on Domain/Application as required by their contracts.

#### `Sasd.Pims.WinForms`

Contains:

- Forms/UserControls;
- presenters/controllers/view models where chosen by the approved UI design;
- Windows application lifetime;
- Composition Root / dependency wiring where appropriate.

Forms/UserControls must not:

- execute SQL;
- own a long-lived `DbContext`;
- implement persistence logic;
- bypass Application use cases to mutate domain state.

A WinForms reference to Infrastructure is acceptable only where required for composition/startup wiring. Keep concrete infrastructure use out of normal UI behaviour.

### Dependency direction

Conceptually:

```text
WinForms ───► Application ───► Domain
   │
   └───────► Infrastructure ─► Application/Domain contracts
```

No cyclic project references.

---

## 7. Solution and test-project scope

At scaffold time, prefer the smallest test-project set that has real responsibilities.

Expected categories over time:

- Domain/Application unit tests;
- real SQLite integration tests;
- architecture/dependency tests;
- later UI automation only when the UI shell is stable enough to justify it.

Do not create empty test projects merely to match a theoretical template.

If separate projects are created, conventional names are:

```text
tests/
├── Sasd.Pims.Domain.Tests/
├── Sasd.Pims.Application.Tests/
├── Sasd.Pims.IntegrationTests/
└── Sasd.Pims.Architecture.Tests/
```

The exact split may remain smaller during the first scaffold if the governing implementation task justifies it.

---

## 8. Database and persistence rules

For current early milestones:

- use real SQLite for persistence integration tests;
- do not use EF Core InMemory as a substitute for SQLite behaviour;
- use short-lived `DbContext` instances scoped to a use case/unit of work;
- use EF Core migrations for production schema evolution;
- do not use `EnsureCreated()` as the production migration strategy;
- keep persistence mappings in Infrastructure;
- preserve stable Project identity;
- apply a transaction per domain save where specified;
- make stale-write/concurrency conflicts observable rather than silently overwriting.

Current default data path:

```text
%LOCALAPPDATA%\SASD\PIMS\data\pims.db
```

Tests must use isolated temporary paths.

### Current `0.1.0` Project persistence

Preserve the accepted `0.0.1-internal` database as the migration source and evolve it through real EF Core migrations.

For `0.1.0`:

- extend Project only with fields and relations required by the approved project-catalog scope;
- preserve stable Project identity and Project Key semantics;
- preserve existing `0.0.1-internal` Project data during migration;
- test migration from a real `0.0.1-internal` database to the current `0.1.0` schema;
- verify that migrated data can be edited, saved, closed and reopened;
- verify that backup/restore still works after the schema change;
- do not delete/recreate the user database as a substitute for migration.

Do not pre-create future tables for Requirements, Products, Risks, Milestones, Decisions, Releases or external references.

If tags or small controlled reference data are required by the approved `0.1.0` baseline, implement only the minimal normalized model needed for actual catalog use.

### Backup/restore

Restore must be staged and verified.

A candidate backup must not replace the active database until:

- package/manifest checks pass;
- SHA-256 checks pass;
- schema/version compatibility is acceptable;
- SQLite integrity validation passes.

A failed candidate restore must leave active data untouched.

---

## 9. UI rules

Use native WinForms controls first.

Do not add a full UI toolkit, Ribbon framework, docking framework, chart library or WebView merely for visual polish.

For `0.1.0`, build a simple but genuinely usable project catalog with native WinForms controls.

Minimum practical UI scope:

- MainForm with useful project list/catalog;
- clear project selection/opening;
- create/view/edit Project;
- explicit Save/Cancel behaviour;
- understandable validation;
- safe error feedback;
- reversible archive/deactivate/reactivate behaviour when the domain rule is finalized;
- access to the already proven backup/restore path;
- reliable restart/reopen behaviour.

The repository dashboard screenshot is a **design concept**, not proof of implemented functionality and not a requirement to reproduce the full dashboard in `0.1.0`.

Prioritize usability and clarity over visual sophistication. Do not add Ribbon, charts, complex themes, docking or custom-drawing infrastructure just to make the early release look more elaborate.

### Accessibility and DPI

For relevant UI work:

- keyboard operation must be possible for the core workflow;
- define logical tab/focus order;
- do not communicate status/error only through colour;
- use meaningful accessible names/descriptions where native labelling is insufficient;
- avoid brittle pixel-perfect layouts;
- verify 100%, 125% or 150%, and 200% scaling for the P1 smoke baseline;
- prefer Per-Monitor V2 DPI awareness.

---

## 10. Logging and diagnostics

Application code should target the `Microsoft.Extensions.Logging` abstraction where logging is required.

Do not hard-wire Serilog types into Domain/Application.

The accepted `0.0.1-internal` implementation uses a small custom `ILoggerProvider` for local structured diagnostics.

Keep it unless a concrete defect or requirement justifies replacement. Do not introduce Serilog or another logging framework merely for stylistic preference.

Routine logs must not contain:

- passwords;
- tokens;
- secrets;
- full user/project content unless explicitly necessary and approved;
- real database contents.

Unexpected/infrastructure errors should be correlatable between safe user-visible messages and technical diagnostics.

---

### Accepted SQLite native dependency decision

The `0.0.1-internal` implementation explicitly pinned the patched native SQLite dependency used by the accepted vertical slice to version `2.1.12`.

Do not change this pin casually. Any update must pass:

- licence/dependency review;
- vulnerability review;
- real SQLite integration tests;
- migration tests;
- backup/restore and recovery tests.

---

## 11. External dependencies

Do not add a new direct package casually.

Before adding one, check:

- immediate need for the active milestone;
- licence compatibility;
- maintenance/activity;
- platform compatibility;
- deployment impact;
- security implications;
- lock-in/replacement path.

Prefer the .NET platform and already-approved dependencies when adequate.

Do not reimplement complex infrastructure solely to avoid a small, established dependency; raise the trade-off if a package is clearly safer/smaller than custom code.

No external UI suite is approved as the default foundation.

---

## 12. Build and package rules

Keep builds reproducible from a clean checkout.

Use central build/package configuration only when it serves multiple projects and matches the approved baseline.

Do not create speculative build infrastructure.

The accepted `0.0.1-internal` evidence selected **self-contained `win-x64`** as the default packaging model for `0.1.0`.

For `0.1.0`:

- keep the self-contained `win-x64` default unless new evidence justifies a change;
- keep the portable ZIP as the practical early-release artifact;
- generate and verify SHA-256;
- keep real databases, logs, backups, `.env` files, tokens and secrets out of artifacts;
- do not introduce an installer, code-signing workflow or auto-updater merely to complete `0.1.0`.

---

## 13. Development priority: usability before premature optimization

The primary goal of the early milestones is to make PIMS useful quickly while preserving correctness, data integrity, recovery, security and architectural boundaries.

Code must be:

- correct;
- understandable;
- maintainable;
- adequately tested;
- safe with user data.

Code does **not** need to be maximally optimized during early feature milestones.

Avoid unless there is a measured or clearly demonstrated need:

- micro-optimizations;
- speculative caching;
- premature parallelism;
- complex pooling;
- generic frameworks for hypothetical future features;
- large refactorings with no current functional benefit;
- performance tuning without measurements;
- replacing simple working code with a theoretically more elegant but substantially more complex design.

Prefer:

- the simplest correct implementation;
- clear control flow;
- small cohesive classes/methods;
- reversible decisions;
- early practical functionality.

When a potentially useful optimization is discovered but is not necessary for the active milestone:

1. do not implement it automatically;
2. record it as technical debt/improvement when it is material;
3. continue toward usable milestone functionality.

A deliberate optimization/refactoring phase is expected after `0.8.0` feature freeze and before `0.9.0`, using real measurements, profiling and accumulated evidence.

Small obvious improvements that reduce defects or complexity without distracting from the milestone may still be made immediately.

---

## 14. Code comments and XML documentation

The source should be understandable to a developer who did not make the original implementation decision.

Use **English identifiers, English technical comments and English XML documentation** in C# source. User-facing UI text remains German according to the product baseline.

### XML documentation

Use `///` XML documentation where it adds durable value, especially for:

- public domain types;
- public application use cases;
- public ports/interfaces;
- public DTOs/result types with non-trivial semantics;
- backup/restore/export APIs;
- public methods with important invariants, side effects, conflict semantics or lifecycle rules.

Useful XML documentation should explain:

- purpose;
- important parameter semantics;
- return/result semantics;
- invariants or preconditions;
- important failure/conflict behaviour;
- relevant side effects.

Do not add verbose XML comments to trivial private helpers merely to increase comment volume.

### Inline comments

Use `//` comments to explain **why**, not to translate obvious C# syntax.

Comment especially:

- non-obvious domain decisions;
- state transitions;
- transaction boundaries;
- concurrency/revision handling;
- migration compatibility logic;
- recovery/rollback steps;
- atomic file/database replacement;
- security or integrity checks;
- platform/framework workarounds;
- intentionally unusual ordering of operations;
- places where a simpler-looking implementation is deliberately avoided for correctness.

Good example:

```csharp
// Increment the revision only after the domain mutation has been accepted.
// Persistence compares this value to detect stale editors instead of silently
// overwriting a newer project state.
revision++;
```

Avoid comments such as:

```csharp
// Increment revision.
revision++;
```

Comments must stay correct when code changes. Remove or update stale comments during related refactoring.

The goal is **readable, teachable code**, not maximum comment density.

---

## 15. Verification rules

Before claiming a task complete:

1. inspect the relevant source/docs;
2. build the affected project/solution;
3. run relevant automated tests;
4. use real SQLite where persistence behaviour is under test;
5. inspect the final Git diff/status;
6. report exactly what was and was not verified.

Once a solution exists, the normal escalation is:

```bash
dotnet restore
dotnet build
dotnet test
```

Use narrower builds/tests during iteration when appropriate, then run the required wider gate before milestone/release acceptance.

Do not claim:

- "works";
- "tested";
- "verified";
- "ready";

unless the corresponding action actually ran successfully.

For a bug fix, add a regression test when reasonably automatable.

---

## 16. Architecture tests

Automate dependency rules as soon as the scaffold makes that practical.

At minimum verify:

- Domain does not reference WinForms;
- Domain does not reference EF Core;
- Domain does not reference Infrastructure;
- Application does not reference WinForms;
- normal WinForms code does not own/use concrete `DbContext`;
- no prohibited cyclic dependency exists.

If an architecture test fails, do not weaken the test simply to make the build green. First determine whether the implementation or the approved architecture must change.

---

## 17. Git workflow and safety

Before modifying files:

```bash
git status -sb
```

After a meaningful step:

```bash
git status -sb
git diff --check
git diff
```

Rules:

- keep commits small and conceptually coherent;
- use Conventional Commits;
- do not mix unrelated formatting churn with behaviour;
- never commit secrets, real databases, backups, logs or user data;
- do not rewrite shared history;
- do not force-push;
- do not reset/discard unrelated user changes;
- when the user has authorized autonomous milestone work, commit each logically complete change after its relevant quality gates pass;
- outside such an authorization, do not commit unless the user explicitly asks;
- do not push unless the user has explicitly authorized pushing for the current task or milestone;
- never force-push or rewrite shared history.

Suggested commit families:

- `chore(solution): ...`
- `build(repo): ...`
- `feat(projects): ...`
- `feat(persistence): ...`
- `feat(ui): ...`
- `feat(recovery): ...`
- `fix(...): ...`
- `test(...): ...`
- `refactor(...): ...`
- `docs(...): ...`
- `ci(...): ...`

---

## 18. Change discipline

Do not edit approved requirements/architecture merely to make implementation easier.

Update documentation only when:

- the task explicitly requires a baseline change;
- implementation evidence disproves an assumption;
- a new migration/exchange/dependency decision must be recorded;
- a deliberate deviation has been approved.

If implementation reveals a strategic question, report it rather than deciding it as a side effect of a coding task.

Examples of strategic questions:

- replacing SQLite;
- changing Project/Product semantics;
- merging Requirement and Task;
- changing milestone scope;
- adding mandatory cloud/service dependencies;
- changing the supported platform;
- introducing a plugin architecture.

---

## 19. Working method for Codex

For each task:

1. read this `AGENTS.md`;
2. identify the active milestone and governing documents;
3. inspect the current working tree;
4. state or internally maintain the smallest implementation plan;
5. modify only files needed for the task;
6. run the narrowest useful checks while iterating;
7. run the required build/tests before declaring completion;
8. inspect the diff;
9. summarize:
   - files changed;
   - behaviour implemented;
   - tests/builds actually run;
   - failures/deviations;
   - any architectural question requiring escalation;
10. stop at the requested task boundary, unless the user has explicitly authorized autonomous work for the whole active milestone.

During an autonomous milestone run, continue from one verified, committed step to the next **within the same active milestone**. Never continue into the next milestone automatically.

---

## 20. Definition of Done for a normal implementation task

As applicable:

- [ ] requested behaviour implemented;
- [ ] active milestone boundary respected;
- [ ] architecture boundaries respected;
- [ ] failure paths considered;
- [ ] build successful;
- [ ] relevant tests successful;
- [ ] regression test added for bug fix where practical;
- [ ] real SQLite used for persistence integration behaviour;
- [ ] no secrets/local user data added;
- [ ] documentation updated only where justified;
- [ ] `git diff --check` clean;
- [ ] final diff reviewed for unrelated changes;
- [ ] verification limitations explicitly reported;
- [ ] commit behaviour followed the current user authorization (manual task vs. autonomous milestone run).

---

## 21. Stop rule against overengineering

When the approved documents determine the next step sufficiently:

**implement it.**

Do not add abstractions, extension points, projects, generic repositories, plugin hooks, provider frameworks, UI frameworks or future-domain tables merely because they may be useful someday.

The preferred question is:

> What is the smallest next verifiable change that advances the active milestone and proves a relevant assumption?

If a small working experiment can resolve a technical uncertainty more reliably than additional general planning, prefer the experiment and record the evidence.

---

## 22. Milestone transition rule

When a milestone is accepted:

1. do not silently start the next milestone;
2. produce/confirm the required evidence and release/PoC notes;
3. record open defects, technical debt and architecture deviations;
4. confirm the next milestone explicitly with the user;
5. update the **Current active milestone** section of this `AGENTS.md`;
6. only then begin work scoped to the next milestone.

The full roadmap section above remains unchanged unless the project roadmap itself is formally revised.
