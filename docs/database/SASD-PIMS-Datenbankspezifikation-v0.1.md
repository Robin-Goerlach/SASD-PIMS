# SASD PIMS — Datenbankspezifikation v0.1

**Status:** Ready for implementation of `0.0.1-internal`; controlled expansion for `0.1.0`  
**Database:** SQLite via Entity Framework Core  
**Platform:** .NET 10 / Windows 11 x64  
**Sources:** Lastenheft v0.1, Pflichtenheft v0.1, Software Architecture Documentation v1.0, implementation baseline

> This specification intentionally does not model the entire 1.0 domain. The purpose of the first schema is to prove persistence, migration and recovery with the smallest useful domain slice.

## 1. Goals

The database design must:

- persist authoritative local PIMS data without a server;
- preserve stable identities across rename/export/restart;
- enforce essential integrity independently of UI behaviour;
- support controlled EF Core migrations;
- remain testable with real temporary SQLite files;
- permit consistent backup and staged restore;
- evolve incrementally instead of pre-creating tables for future roadmap stages.

## 2. Explicit non-goals for the first schema

Not physically modelled in `0.0.1-internal`:

- Product;
- Requirement;
- Blocker;
- Milestone;
- Decision;
- Risk;
- Release;
- external references;
- tags/reference-data administration;
- audit history beyond technical timestamps/revision;
- multi-user ownership/permissions;
- cloud/synchronisation metadata.

Their absence from P1 is a release-staging decision, not removal from later requirements.

## 3. Database location and ownership

Default logical location:

```text
%LOCALAPPDATA%\SASD\PIMS\data\pims.db
```

The application resolves the path through the Windows known-folder API (`Environment.SpecialFolder` or an equivalent centrally tested abstraction). No production database is stored beside the executable.

Other operational roots:

```text
Configuration: %APPDATA%\SASD\PIMS
Logs:          %LOCALAPPDATA%\SASD\PIMS\logs
Backups:       user-selectable, suggested under Documents\SASD-PIMS-Backups
Exports:       user-selectable
```

Tests use isolated temporary directories and never reuse the user's database.

## 4. Context lifetime and transaction rules

- `DbContext` is short-lived and scoped to one use case / clearly bounded unit of work.
- WinForms Forms/UserControls never hold or create `DbContext` directly.
- A Project save is one transaction.
- Parallel writes in the single-user MVP are serialised by the application write coordinator.
- A failed save must not leave a partially persisted aggregate.
- Reads should use projections/no-tracking where mutation is not required.

## 5. Schema P1 — `0.0.1-internal`

### 5.1 Table `Projects`

| Column | Logical type | Null | Rule | Purpose |
| --- | --- | --- | --- | --- |
| `Id` | GUID | no | Primary key, immutable | Stable technical identity |
| `Key` | string | no | Unique, immutable after creation | Human-readable stable project identifier |
| `Name` | string | no | Trimmed, non-empty | Display name |
| `ShortDescription` | string | yes | Trimmed when present | Minimal explanatory text |
| `CreatedAtUtc` | UTC timestamp | no | Set once | Technical creation evidence |
| `ModifiedAtUtc` | UTC timestamp | no | Updated on successful mutation | Technical modification evidence |
| `Revision` | integer | no | Starts at 1, increments on mutation | Explicit optimistic-concurrency token |

### 5.2 P1 invariants

The first implementation must enforce at least:

- `Id` exists and never changes;
- `Key` is not blank;
- `Key` is unique;
- `Key` is immutable after persistence;
- `Name` is not blank;
- timestamps are recorded in UTC;
- `CreatedAtUtc <= ModifiedAtUtc`;
- `Revision >= 1`;
- validation is performed in Domain/Application and essential uniqueness/nullability is also protected by persistence.

### 5.3 Project-key syntax

The baseline requires a stable, non-empty, unique human-readable key but does not yet contain a sufficiently proven universal syntax.

Therefore:

- P1 must not hide key generation inside persistence;
- trim leading/trailing whitespace;
- compare uniqueness using a documented normalisation strategy;
- do not allow a later rename of `Key`;
- record the final allowed-character/normalisation rule before the Project aggregate is implemented.

Recommended PoC examples may use values such as `SASD-PIMS`; examples are not the final grammar.

P1 decision (`OD-P1-001`):

- trim leading/trailing whitespace and normalise with invariant upper casing before storage and comparison;
- allow 1 to 64 characters from `A-Z`, `0-9` and `-`;
- require the first and last character to be alphanumeric;
- reject repeated or leading/trailing hyphens;
- persist the normalised key so the unique database index and Domain/Application comparison use identical semantics.

### 5.4 Indexes

Required for P1:

- primary-key index on `Id`;
- unique index on normalised/stored `Key`;
- optional index on `Name` only if the first project-list query demonstrates need.

Do not add speculative indexes for future entities.

## 6. EF Core mapping rules

- Persistence mappings live in Infrastructure.
- Domain types do not carry EF Core attributes merely for convenience.
- Use fluent configuration for schema/constraints.
- Migration source files are version controlled.
- Generated migration changes must be reviewed before commit.
- Schema creation in application runtime is migration-driven; `EnsureCreated()` is not the production migration strategy.
- Tests may build a database from migrations and must prove that a fresh database reaches the current schema.

## 7. Initial migration

Suggested name:

```text
InitialVerticalSlice
```

It creates only the schema required by P1 plus EF migration history.

Acceptance:

- a clean database migrates successfully;
- running against an already-current database performs no destructive work;
- failure is reported clearly;
- the application's current schema version can be diagnosed.

## 8. Evolution to `0.1.0`

`0.1.0` expands Project toward the Lastenheft project master record. The exact physical migration is approved after P1 evidence, but the target information includes:

- `Goal`;
- `Benefit`;
- project type;
- project area;
- responsibility;
- lifecycle/status information;
- tags/classification needed by the 0.1 project catalog.

### 8.1 Reference data

Project type and area should be represented as stable, language-neutral values/identities rather than persisting display strings as domain truth.

Reference-data administration itself is not required in 0.1; a small seeded catalog is acceptable until later controlled administration is justified.

### 8.2 Tags

If tags are required in 0.1, model a normalised many-to-many relation rather than a delimiter-separated text column.

Do not create tag infrastructure in P1 unless it is actually used.

## 9. Deletion and archival

Hard delete is not a default domain operation.

For P1, no destructive project delete workflow is required.

Before implementing `0.1.0` lifecycle behaviour, resolve:

- active/paused/discontinued/archived semantics;
- whether "archive" is a lifecycle status or separate flag;
- reactivation rules;
- what, if anything, may be physically deleted.

Until that decision, test cleanup may delete synthetic databases, but production code must not introduce irreversible project deletion as a convenience feature.

## 10. Concurrency

SQLite does not provide SQL Server-style rowversion semantics. PIMS therefore uses an explicit integer `Revision` for aggregate concurrency.

On update:

1. expected revision is supplied by the application;
2. persistence updates only the matching row;
3. revision is incremented atomically with the update;
4. zero affected rows are treated as a conflict, not an automatic overwrite.

This is intentionally modest for a single-process MVP but makes stale edits observable and testable.

## 11. SQLite operating policy

Mandatory:

- foreign-key enforcement enabled whenever foreign keys exist;
- transactions used for domain writes;
- busy/locked errors translated into a controlled infrastructure error;
- database connections are not shared across unrelated long-lived UI objects.

Journal mode (including WAL) is **not** hard-coded as an architectural requirement in P1. If WAL is enabled, backup/checkpoint/recovery tests must prove the chosen procedure with WAL/SHM state.

## 12. Backup format

P1 backup package:

```text
backup-<timestamp>.zip
├── pims.db
└── manifest.json
```

`manifest.json` must contain at least:

- backup-format version;
- application/schema version;
- creation timestamp UTC;
- database file name;
- database SHA-256;
- optional application version / commit for diagnostics.

The backup must come from a consistent database state. Prefer SQLite's supported backup mechanism or a controlled copy only when no write transaction can invalidate the result.

## 13. Restore

Restore is never "unzip directly over the active database".

Required sequence:

1. choose candidate backup;
2. extract to a fresh staging directory;
3. validate manifest version and required files;
4. verify SHA-256;
5. open staged SQLite database;
6. run schema/version compatibility check;
7. run `PRAGMA integrity_check`;
8. close staged database;
9. create rollback backup/copy of the current data;
10. replace the active database using a controlled operation;
11. reopen and run a smoke read;
12. on failure, preserve evidence and restore the previous state.

A failed candidate validation must leave the current database untouched.

## 14. Migration safety

Before a migration touches a user-valued database:

- create/verify a pre-migration backup;
- apply migration;
- verify open/schema/integrity;
- record outcome.

Downgrade to a binary that cannot understand the newer schema is not silently allowed.

## 15. Export separation

Export DTOs/contracts are not EF entities.

The first JSON export must contain:

- explicit format version;
- stable project `Id`;
- stable project `Key`;
- implemented P1 fields;
- deterministic field semantics.

No schema is considered a public forever-contract merely because it appeared in an internal PoC. Compatibility obligations begin only when a release explicitly declares them.

## 16. Test data

Use synthetic projects only, for example:

```text
Key: SASD-PIMS
Name: SASD PIMS
ShortDescription: Synthetic test fixture for the vertical slice
```

Tests must never copy a real user database into the repository or CI artifacts.

## 17. P1 database acceptance

The database part of `0.0.1-internal` is accepted only when:

- fresh migration succeeds;
- valid Project round-trip succeeds in real SQLite;
- duplicate key is rejected;
- context/service-provider reconstruction still reloads the same data;
- concurrency conflict is observable;
- backup hash is verifiable;
- invalid backup hash is rejected without modifying active data;
- valid staged restore reopens successfully;
- all test databases are isolated and disposable.

## 18. Deferred design work

Defer until the corresponding roadmap stage:

- requirements and references schema;
- audit-history physical model;
- FTS5;
- product/release/milestone schema;
- generic reference-data administration;
- multi-user locks/permissions;
- remote/provider metadata;
- plugin-extensible persistence.

## 19. Schema evolution for 0.2.0

Migration `202608200002_ProjectSteering` extends `Projects` additively with `Phase`,
`ActivityState`, `TargetDate`, `LastReviewedAtUtc` and `NextReviewDueAtUtc`. Existing 0.1 rows
receive the conservative defaults `Idea` and `NotStarted`; nullable scheduling facts remain unset.

`ProjectBlockers` stores `Id`, `ProjectId`, `Summary`, optional `Details`, `CreatedAtUtc`, optional
`ResolvedAtUtc` and optional `ResolutionNote`. Resolution updates an open row conditionally and
never deletes its history. The foreign key uses the established Project identity. The normal UI
continues to provide no Project hard-delete operation.

Review freshness, target-date indication and NeedsAttention are derived from persisted facts and
current time. They are deliberately not stored as authoritative snapshots.

That deferral is intentional scope control.

## 20. Schema evolution for 0.3.0

Migration `202608200003_RequirementsAndTypedReferences` is additive to the released 0.2 schema.
It creates `Requirements`, `AcceptanceCriteria` and `ExternalReferences` without modifying existing
Project, tag, steering or blocker rows.

`Requirements` has a required Project foreign key, immutable GUID and project-local `Key`, controlled
priority/decision/source codes, optional source facts/reference and an explicit concurrency revision.
`(ProjectId, Key)` is unique. `AcceptanceCriteria` preserves order through the unique
`(RequirementId, Sequence)` index and may point to a verification reference. `ExternalReferences`
always has ProjectId and may additionally identify a Requirement in that same Project. Application
validation enforces the cross-row same-Project invariant before persistence; relational foreign keys
protect the referenced identities.

No reachability, health, provider metadata, attachment content or task state is stored. Existing 0.2
databases receive a verified pre-migration backup and remain editable after migration and reopen.

## 21. Schema evolution for 0.4.0

Migration `202608200004_SearchTraceabilityAndExchange` is additive to the released 0.3 schema. It creates
only `ChangeEvents` plus targeted normal indices for Project steering/classification, Requirement controlled
values and ExternalReference type/Project filtering. It creates no search-result, traceability, export or FTS5
table. Existing 0.3 Projects, Requirements, AcceptanceCriteria, Blockers and ExternalReferences remain unchanged.

`ChangeEvents` stores stable `Id`, `ProjectId`, controlled technical `EntityType`/`EventType`, `EntityId`,
`OccurredAtUtc` and optional redacted `OldValue`/`NewValue` strings limited to 256 characters. Normal application
writes append whitelisted events in the same persistence transaction as the business change. There is no normal
update/delete path and the table is not an event-sourcing store.
