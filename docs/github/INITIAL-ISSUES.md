# Initial Issues

The following issues are intended as the first implementation backlog. Titles are ready for GitHub; acceptance criteria should be copied into each issue.

## Milestone `0.0.1-internal — Vertical Slice`

### 1. `chore(repo): scaffold .NET 10 solution and project structure`

**Labels:** `type:chore`, `area:build-release`, `priority:p1`, `status:ready`

Acceptance:

- solution builds on a clean supported Windows development environment;
- Domain, Application, Infrastructure and WinForms responsibilities are represented;
- projects exist only where they already have a concrete purpose;
- `dotnet restore`, `dotnet build` and `dotnet test` have documented entry points.

### 2. `build(repo): add global SDK and central build/package configuration`

**Labels:** `type:chore`, `area:build-release`, `priority:p1`

Acceptance:

- .NET 10 SDK requirement is explicit;
- nullable reference types are enabled for authored C# code;
- warnings policy is documented;
- package versions have one authoritative location.

### 3. `test(architecture): enforce project dependency rules`

**Labels:** `type:test`, `type:architecture`, `priority:p1`

Acceptance:

- Domain cannot reference WinForms, EF Core or Infrastructure;
- Application cannot depend on WinForms;
- prohibited dependencies fail an automated test.

### 4. `feat(domain): implement minimal Project aggregate`

**Labels:** `type:feature`, `area:domain`, `priority:p1`

Acceptance:

- stable identity exists;
- minimum project master data for the vertical slice is represented;
- invariants are enforced in domain/application logic, not WinForms event handlers;
- domain tests cover valid and invalid creation.

### 5. `feat(application): implement CreateProject and LoadProject use cases`

**Labels:** `type:feature`, `area:domain`, `priority:p1`

Acceptance:

- UI-independent request/response models exist;
- validation failures are returned deliberately;
- use cases do not know EF Core or WinForms types.

### 6. `feat(persistence): add SQLite/EF Core project persistence`

**Labels:** `type:feature`, `area:persistence`, `priority:p1`

Acceptance:

- database can be created from a defined migration/baseline;
- project can be persisted and loaded;
- transaction boundaries are explicit;
- synthetic integration test verifies round-trip persistence.

### 7. `feat(ui): create minimal WinForms project editor for vertical slice`

**Labels:** `type:feature`, `area:ui`, `priority:p1`

Acceptance:

- create/save workflow is usable with keyboard;
- validation is visible without relying on colour alone;
- UI does not access DbContext directly;
- no external UI framework is introduced.

### 8. `feat(app): compose startup, database initialization and controlled shutdown`

**Labels:** `type:feature`, `type:architecture`, `priority:p1`

Acceptance:

- one composition root wires application services;
- startup failures are handled without silent corruption;
- shutdown disposes owned resources deterministically.

### 9. `feat(logging): add structured local logging and error correlation`

**Labels:** `type:feature`, `priority:p1`

Acceptance:

- user-visible failures have a correlation/error identifier where appropriate;
- secrets and project content are not logged by default;
- log location and retention baseline are documented.

### 10. `feat(export): implement minimal versioned project export`

**Labels:** `type:feature`, `area:import-export`, `priority:p1`

Acceptance:

- export contains an explicit format version;
- output is deterministic enough for test comparison where practical;
- unsupported/newer format behaviour is defined.

### 11. `feat(backup): implement verified backup and staged restore PoC`

**Labels:** `type:feature`, `area:backup-recovery`, `priority:p0`

Acceptance:

- backup is created from a consistent state;
- integrity/checksum metadata is recorded;
- restore validates a candidate before replacing current data;
- restore creates or preserves a rollback path;
- failure-path integration tests exist.

### 12. `test(persistence): add create-close-reopen round-trip scenario`

**Labels:** `type:test`, `area:persistence`, `priority:p1`

Acceptance:

- test uses real SQLite, not only mocks;
- application data survives process/service reconstruction;
- cleanup is isolated to synthetic test directories.

### 13. `test(recovery): add fault-injection cases for backup/restore`

**Labels:** `type:test`, `area:backup-recovery`, `priority:p0`

Acceptance:

- invalid checksum is rejected;
- missing file is handled;
- interrupted/failed restore leaves a recoverable current state;
- test evidence is reproducible.

### 14. `test(ui): establish DPI, keyboard and accessibility baseline`

**Labels:** `type:test`, `area:ui`, `priority:p1`

Acceptance:

- vertical-slice screens checked at 100%, 125%, 150% and 200% where available;
- tab order is usable;
- controls have meaningful accessible names where required;
- no clipped mandatory content is accepted.

### 15. `ci(quality): add Windows build and test quality gate`

**Labels:** `type:chore`, `area:build-release`, `priority:p1`

Acceptance:

- clean checkout builds on CI;
- automated tests run;
- failed tests fail the workflow;
- release packaging is not yet silently published.

### 16. `build(release): produce first internal self-contained win-x64 artifact`

**Labels:** `type:chore`, `area:build-release`, `priority:p1`

Acceptance:

- artifact can be produced reproducibly from a tag/commit;
- version is visible;
- SHA-256 checksum is generated;
- no real user database or secret is packaged;
- release is marked prerelease/internal.

## Milestone `0.1.0 — Project Catalog`

### 17. `feat(projects): implement project list and selection`

**Labels:** `type:feature`, `area:ui`, `priority:p1`

Acceptance:

- projects can be listed and selected;
- empty state is understandable;
- list does not load unbounded unnecessary data;
- sorting/filtering requirements are documented rather than guessed.

### 18. `feat(projects): implement project edit workflow`

**Labels:** `type:feature`, `area:ui`, `area:domain`, `priority:p1`

Acceptance:

- existing project can be edited and saved;
- validation and conflict behaviour are explicit;
- persistence and UI tests cover the main path.

### 19. `feat(projects): define archive/deactivate behaviour`

**Labels:** `type:feature`, `area:domain`, `status:needs-decision`, `priority:p1`

Before implementation confirm whether project deletion, archival or deactivation is required by the requirements baseline.

Acceptance after decision:

- lifecycle rule is documented;
- irreversible deletion is not introduced accidentally;
- UI and persistence semantics match the decision.

### 20. `docs(0.1): create first user quick-start and release checklist`

**Labels:** `type:documentation`, `area:build-release`, `priority:p1`

Acceptance:

- installation/run instructions match the actual artifact;
- screenshots use synthetic data;
- known limitations are explicit;
- backup/recovery expectations are documented.
