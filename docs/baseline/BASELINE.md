# SASD PIMS — Implementation Baseline

**Baseline ID:** PIMS-IMPLEMENTATION-BASELINE-2026-08  
**Status:** Approved for `0.0.1-internal` and preparation of `0.1.0`  
**Date:** 7 August 2026  
**Target:** Controlled transition from planning to implementation

## 1. Purpose

This record establishes the documents that may be used as the implementation basis for the first vertical slice and the subsequent `0.1.0` project-catalog release.

The source documents keep their original document status and version text. This baseline does not rewrite their history; it records that SASD accepts them as the working basis for the stated implementation scope, subject to the explicit reconciliations below.

## 2. Normative source set

| Priority | Artifact | Version | Role |
| ---: | --- | --- | --- |
| 1 | Lastenheft | 0.1 | Functional intent, requirement priority, acceptance intent and scope boundaries |
| 2 | Pflichtenheft | 0.1 | Technical interpretation, test IDs and implementation constraints |
| 3 | Software Architecture Documentation | 1.0 | System boundaries, dependency rules, quality gates and architecture decisions |
| 4 | Development Roadmap | 0.1 | Release staging and sequencing; does not alter requirement priority |
| 5 | Database/UI/Test specifications | 0.1 | Implementation-near definition for `0.0.1-internal` and `0.1.0` |

Repository paths are indexed in `docs/README.md`.

## 3. Baseline rules

1. The Lastenheft remains authoritative for *what* must be achieved.
2. The Pflichtenheft may refine *how*, but may not reduce a Lastenheft MUST requirement.
3. Architecture v1.0 is the accepted target architecture. A documented architecture decision is not automatically proven by code.
4. The roadmap controls sequencing only. A feature assigned to a later pre-release does not lose its original MUST/SHOULD/MAY priority.
5. P0/P1 specifications intentionally model only the first slice. Absence from the P1 physical schema does not remove an MVP requirement.
6. If implementation contradicts a baseline artifact, either implementation is corrected or the baseline is changed explicitly.
7. No new framework, database, cloud dependency, plugin system or UI suite may be introduced without demonstrated need and dependency/ADR review.

## 4. Explicit reconciliations

### BL-R-001 — Supported platform

**Decision:** The implementation baseline is **Windows 11 x64**.

The repository README previously mentioned Windows 10/11. That text is corrected by this changeset. Windows 10 compatibility may be investigated later but is not a supported product promise.

### BL-R-002 — Licence

**Decision:** The repository currently publishes SASD PIMS under **Apache License 2.0**. The README must state the licence as an actual repository fact, not as a future decision.

Third-party packages still require individual licence and maintenance review.

### BL-R-003 — Performance reference count for external references

The Lastenheft acceptance quantity requires at least **20,000 references**, while the Pflichtenheft technical reference set previously mentioned 10,000.

**Resolution:** Performance testing must use **at least 20,000 references** so the technical test does not undercut the functional acceptance baseline.

This does not block the vertical slice because requirements/references are not yet physically implemented there.

### BL-R-004 — Publish model

Framework-dependent versus self-contained `win-x64` publish remains an evidence-based decision for `0.0.1-internal`.

**Resolution:** compare both; record size, prerequisite and operation trade-offs; choose the `0.1.0` default only after measurement. Portable ZIP remains the first packaging form. An installer does not block P1.

### BL-R-005 — Logging provider

The application-level contract should use `Microsoft.Extensions.Logging`.

**Resolution:** the concrete local file provider remains a P1 decision. Serilog may be adopted only after licence/activity/dependency review. Application/domain code must not depend on Serilog types.

## 5. Intentional open decisions that do not block scaffolding

| ID | Decision | Latest decision point |
| --- | --- | --- |
| OD-P1-003 | Framework-dependent vs self-contained default package | End of `0.0.1-internal` |
| OD-01-001 | Final project lifecycle/archive/deletion semantics | Before `0.1.0` lifecycle feature |
| OD-01-002 | Exact controlled vocabularies for project type/area | Before `0.1.0` reference data seed |

These decisions must not be guessed inside UI event handlers or persistence mappings.

### Resolved P1 implementation decision

`OD-P1-001` is resolved before implementation of the Project aggregate. P1 project keys are trimmed, normalised to
invariant upper case, limited to 1-64 characters from `A-Z`, `0-9` and `-`, start and end with an alphanumeric
character, and do not contain repeated hyphens. The normalised value is stored and compared for uniqueness. The
database specification contains the persistence-level rule.

`OD-P1-002` is resolved for P1 with a small local JSON Lines provider implemented behind
`Microsoft.Extensions.Logging.ILoggerProvider`. It writes daily files with a 14-day retention window and deliberately
records no project payload or full local paths. Serilog is not introduced because the P1 logging needs do not justify
the additional dependency; the application contract remains provider-neutral.

`OD-P1-003` is resolved at the end of P1 in favour of a self-contained `win-x64` ZIP as the
default package for `0.1.0`. The measured P1 publish was 125.10 MiB self-contained versus
8.20 MiB framework-dependent. The larger package is accepted for the internal/local-first
distribution because it removes the separate .NET Desktop Runtime prerequisite. The
framework-dependent publish remains a supported evaluation output, not the default artifact.

### Resolved 0.1 implementation decisions

`OD-01-001` is resolved for `0.1.0` as a separate reversible archive flag. Archiving removes a
project from the normal active catalog without deleting its master data; archived projects remain
findable and can be reactivated. The normal UI exposes no physical project deletion. Broader phase,
pause and discontinuation semantics remain deferred to the status-focused `0.2.0` milestone.

`OD-01-002` is resolved without seeding an unproven organisation-wide vocabulary. `0.1.0` stores
optional, language-neutral project-type and project-area codes and validates their stable technical
syntax. Users can classify and filter projects by those codes; controlled reference-data catalogs and
their administration remain deferred until real catalog usage supplies the vocabulary. Simple tags are
stored as a normalised relation rather than delimiter-separated project data.

## 6. Approved first implementation scope

`0.0.1-internal` must prove:

1. controlled startup and data paths;
2. creation of one minimal Project;
3. central validation;
4. transactional persistence in real SQLite;
5. clean shutdown;
6. re-open/reload after complete service reconstruction;
7. minimal versioned JSON export;
8. verified backup and staged restore;
9. negative recovery tests;
10. structured diagnostics/error correlation;
11. DPI/keyboard/accessibility baseline;
12. reproducible build/test/package evidence.

Everything else remains outside the vertical-slice scope unless required to prove one of these points.

## 7. Change control

Any change to the following requires an explicit review and normally an ADR or baseline amendment:

- project/product/requirement/task/milestone/release semantics;
- database technology or ownership;
- project dependency rules;
- persistence/migration/backup semantics;
- public exchange-format compatibility;
- security/trust boundaries;
- supported platform;
- release/update model;
- mandatory external dependencies.

## 8. Baseline exit

After `0.0.1-internal`, record:

- which architecture decisions were actually proven;
- which assumptions failed;
- schema/migration findings;
- UI usability findings;
- restore/fault-injection findings;
- packaging decision;
- technical debt accepted into `0.1.0`.

The result is evidence for implementation, not a reason to create another general architecture volume.
