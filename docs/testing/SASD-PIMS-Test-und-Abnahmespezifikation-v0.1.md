# SASD PIMS — Test- und Abnahmespezifikation v0.1

**Status:** Ready for `0.0.1-internal`; basis for `0.1.0`  
**Principle:** Architecture is accepted by document, implementation is accepted by evidence.

## 1. Scope

This specification defines the minimum verification needed before the vertical slice can be called successful.

It focuses on:

- Project domain rules;
- Application use cases;
- real SQLite persistence;
- migration;
- JSON export;
- backup/restore;
- fault paths;
- architecture boundaries;
- minimal WinForms usability/accessibility/DPI;
- reproducible build/package evidence.

It does not attempt to define every MVP test before the corresponding feature exists.

## 2. Test layers

| Layer | Purpose | Typical technology |
| --- | --- | --- |
| Domain unit | Invariants/value behaviour | xUnit |
| Application unit | Use-case orchestration and result semantics | xUnit + small fakes |
| SQLite integration | Real persistence/migration/concurrency | xUnit + temporary SQLite files |
| Recovery/system | Backup, restore, corruption/failure cases | real files + SQLite |
| Architecture | Dependency rules | automated assembly/project test |
| UI smoke/manual | DPI, keyboard, accessible names, visible errors | manual first; automation later |
| Build/package | Clean restore/build/test/publish and smoke start | scripts/CI |

**Rule:** EF Core InMemory is not a substitute for SQLite integration tests.

## 3. Test isolation

Every test run that touches files must use a fresh synthetic root, for example:

```text
<temp>/SASD-PIMS/tests/<run-id>/
├── data/
├── logs/
├── backups/
└── exports/
```

Tests must not read/write:

- `%LOCALAPPDATA%\SASD\PIMS` from the developer profile;
- real backups;
- real project documents;
- credentials/tokens.

## 4. Domain tests

### P1-DOM-001 — valid project

Given a valid key and name, Project creation succeeds with stable ID, UTC timestamps and revision 1.

### P1-DOM-002 — blank key

Blank/whitespace key is rejected.

### P1-DOM-003 — blank name

Blank/whitespace name is rejected.

### P1-DOM-004 — immutable key

After creation/persistence, normal mutation API cannot change the Project key.

### P1-DOM-005 — revision/timestamp mutation

A valid mutable-field change increments revision and moves `ModifiedAtUtc` forward without changing `CreatedAtUtc`.

## 5. Application tests

### P1-APP-001 — create valid Project

A valid create request returns success and asks the persistence port to store exactly one aggregate.

### P1-APP-002 — invalid create request

Invalid request returns validation result and performs no write.

### P1-APP-003 — duplicate key

Repository uniqueness conflict is translated into a domain/application conflict result suitable for UI presentation.

### P1-APP-004 — load existing Project

Existing stable ID returns the expected neutral result DTO/model.

### P1-APP-005 — load unknown Project

Unknown ID returns `NotFound`; it is not converted into an unhandled exception.

## 6. SQLite integration tests

### P1-DB-001 — migrate clean database

Create new SQLite file and apply all migrations. Schema reaches current version.

### P1-DB-002 — real round trip

```text
create database
→ migrate
→ create Project
→ save
→ dispose DbContext
→ dispose/rebuild service provider
→ load Project
→ compare persisted values
```

The test must use the real SQLite provider and a real temporary file.

### P1-DB-003 — duplicate key

Two Projects with the same effective key cannot both be persisted.

### P1-DB-004 — transaction rollback

Injected failure during a write leaves no partial persisted result.

### P1-DB-005 — optimistic concurrency

Two stale revisions attempt an update. The stale update is detected and does not silently overwrite the current record.

### P1-DB-006 — current migration no-op safety

Opening an already-current test database does not perform destructive schema work.

## 7. Export tests

### P1-EXP-001 — version exists

Export contains an explicit format version.

### P1-EXP-002 — stable identity preserved

Export includes stable Project ID and key.

### P1-EXP-003 — valid JSON

Export parses as valid JSON and satisfies the current internal schema/contract assertions.

### P1-EXP-004 — deterministic semantics

Equivalent input produces semantically equivalent output; unstable incidental values are either documented or isolated.

## 8. Backup tests

### P1-BAK-001 — manifest

Backup ZIP contains `pims.db` and `manifest.json`.

### P1-BAK-002 — checksum

Manifest SHA-256 matches the actual database payload.

### P1-BAK-003 — consistent source

A backup made from the defined procedure reopens successfully and contains the expected Project.

## 9. Restore tests

### P1-RST-001 — valid restore

Valid backup:

```text
validate → stage → integrity check → rollback copy → replace → reopen
```

Expected project is readable after restore.

### P1-RST-002 — invalid checksum

Tampered database/hash mismatch is rejected before active data is changed.

### P1-RST-003 — missing database

Backup without required payload is rejected.

### P1-RST-004 — incompatible manifest/schema

Unsupported format/schema is reported and active database remains untouched.

### P1-RST-005 — integrity failure

A staged SQLite database failing `PRAGMA integrity_check` is never promoted.

### P1-RST-006 — replacement failure

Inject failure around active-file replacement. Previous usable state/rollback evidence remains available.

### P1-RST-007 — rollback backup

Before a valid restore replaces an existing active data set, rollback backup/copy is created and verifiable.

## 10. Logging/error tests

### P1-LOG-001 — correlation

Unexpected/infrastructure failure gets a correlation/error identifier visible to the user and in technical diagnostics.

### P1-LOG-002 — no secrets/full project text by default

Defined secret patterns and full project descriptions are not copied into routine logs.

### P1-LOG-003 — controlled infrastructure messages

SQLite locked/I/O errors are mapped to controlled application errors, not raw stack traces in normal UI.

## 11. Architecture tests

### P1-ARC-001

`Sasd.Pims.Domain` has no reference to:

- WinForms;
- EF Core;
- Infrastructure.

### P1-ARC-002

`Sasd.Pims.Application` has no reference to WinForms.

### P1-ARC-003

WinForms production code does not depend directly on concrete `DbContext`.

### P1-ARC-004

Composition Root is the only place expected to know all concrete implementation assemblies for wiring.

### P1-ARC-005

External provider packages are absent from the first slice unless explicitly approved.

## 12. UI/manual tests

### P1-UI-001 — keyboard

Create → validate → save → reopen can be executed without mouse.

### P1-UI-002 — focus order

Tab order follows visual/logical reading order.

### P1-UI-003 — validation accessibility

Errors are textually available and not represented only by colour.

### P1-UI-004 — DPI

Check 100%, 125% or 150%, and 200%. No mandatory information/control is clipped.

### P1-UI-005 — accessible metadata

Input controls/actions have meaningful accessible names when native labelling is insufficient.

### P1-UI-006 — failure preservation

A save failure does not unexpectedly erase user-entered values.

## 13. Build and package tests

### P1-BLD-001 — clean restore/build/test

From clean checkout:

```powershell
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

succeeds without developer-local prerequisites beyond documented SDK/tooling.

### P1-BLD-002 — publish comparison

Produce and record both where practical:

- framework-dependent `win-x64`;
- self-contained `win-x64`.

Record:

- artifact size;
- runtime prerequisite;
- startup/smoke result;
- operational simplicity;
- selected default for 0.1.

### P1-BLD-003 — package hygiene

Artifact contains no:

- real database;
- logs;
- backups;
- `.env`/tokens;
- developer-local configuration.

### P1-BLD-004 — checksum

Release ZIP has a generated and verified SHA-256.

## 14. Evidence record

Each release/PoC evidence package should identify:

- commit SHA;
- .NET SDK version;
- target/runtime;
- direct package versions;
- schema/migration version;
- test run result;
- manual UI/DPI environment;
- publish model;
- artifact name/size/SHA-256;
- known failures/deviations.

## 15. Quality gates for `0.0.1-internal`

| Gate | Blocking | Pass condition |
| --- | --- | --- |
| QG-P1-01 Build | yes | clean restore/build succeeds |
| QG-P1-02 Unit | yes | all P1 domain/application tests pass |
| QG-P1-03 Architecture | yes | no prohibited dependency |
| QG-P1-04 SQLite | yes | real migration/round-trip/concurrency passes |
| QG-P1-05 Recovery | yes | valid restore + negative hash/integrity paths pass |
| QG-P1-06 UI baseline | yes | keyboard/DPI/validation smoke accepted |
| QG-P1-07 Package | yes | reproducible internal artifact + checksum |
| QG-P1-08 Dependency review | yes | direct packages documented/licence-reviewed |
| QG-P1-09 Documentation | yes | deviations and implementation evidence recorded |

## 16. Vertical-slice acceptance

`0.0.1-internal` is accepted only when a reviewer can reproduce this story:

1. start from the documented build;
2. launch the app;
3. create a valid synthetic Project;
4. verify invalid input is rejected;
5. save;
6. close;
7. restart;
8. load the exact saved Project;
9. export it;
10. create backup;
11. restore a known backup safely;
12. verify a tampered backup is rejected without harming active data;
13. inspect safe diagnostic/error evidence;
14. confirm build/test/package reports.

If this fails, broader feature development pauses until the failing architecture assumption is corrected or deliberately revised.

## 17. `0.1.0` extension

Add tests only as the 0.1 fields/features are introduced:

- full project master record;
- project list/projection;
- edit workflow;
- classification/filtering actually included in 0.1;
- approved lifecycle/archive semantics;
- migration from P1 database to 0.1 without Project loss;
- restart/recovery using migrated data.

Do not pre-write hundreds of future test cases for entities not yet implemented.

## 15. 0.4 verification increment

Automated 0.4 coverage includes all agreed search fields, Project scope, object/controlled filters, sorting,
empty and bounded high-result sets; traceability source/verification/reference/blocker projection; stable IDs;
ChangeEvent whitelist, append-only persistence and sensitive-value redaction; exchange format/version/schema,
canonical values, deterministic collection order, local-reference marker and atomic replacement; deterministic
Markdown; real 0.3-to-0.4 migration and data preservation; reopen, backup, restore and fault-injection recovery;
architecture boundaries; and native WinForms accessibility/DPI structure.

The performance gate seeds 500 Projects, 10,000 Requirements, 20,000 ExternalReferences, 2,000 Blockers and
10,000 ChangeEvents. Twenty representative warmed search/filter operations are measured and the observed P95 is
compared with the two-second target. No FTS5 decision may be made without this evidence. Physical multi-monitor
DPI, visible end-to-end navigation and user-observed Save-dialog behavior remain manual evidence.
## 0.5 MVP qualification increment

Automated qualification adds blocker required/optional rules, same-Project ExternalTask validation and database
triggers, real 0.4-to-0.5 preservation, audit read projection, read-only operating-path/UI structure and Project
list/combined-filter/context-load performance measurements. Import and damaged-import testing are not applicable to
the MVP and remain with Release 1.1. Physical keyboard/DPI/monitor, shell, offline, clean-profile restore/install,
endurance, two-configuration and usability/language checks are `PENDING` in
`docs/releases/0.5.0/MANUAL-ACCEPTANCE.md` until actually executed.
