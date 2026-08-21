# MUST traceability — 0.5.0

Status vocabulary: `ImplementedAndVerified`, `ImplementedButManualVerificationOutstanding`, `NotApplicableToMvp`. The Decision Note resolves LF-BLK-001 and LN-BED-003; no `SpecificationConflict`, `PartiallyImplemented` or `NotImplemented` entry remains.

| Requirement | Intent | Implementation | Tests / evidence | Status |
| --- | --- | --- | --- | --- |
| LF-PRO-001..004 | Project master data, ownership, lifecycle, classification | `Project`, Project use cases, SQLite repositories, catalog UI | Project/Application/SQLite tests; 0.1–0.4 evidence | ImplementedAndVerified |
| LF-STA-001..002 | Independent phase/activity and derived review freshness | `ProjectSteering`, catalog filters/UI | `ProjectSteeringTests`, use-case and UI tests; 0.2 evidence | ImplementedAndVerified |
| LF-REQ-001..004 | Requirements, sources, decisions and criteria | Requirement aggregate/use cases/repository/UI | `RequirementTests`, requirement persistence tests; 0.3 evidence | ImplementedAndVerified |
| LF-BLK-001 | Complete blocker facts and optional external task | extended `ProjectBlocker`, application validation, DB triggers, blocker UI, migration 005 | blocker domain/application/migration/reopen tests; 0.5 evidence | ImplementedAndVerified |
| LF-REF-001 | Typed Project/Requirement references | ExternalReference aggregate, validator, opener and UI | validator/persistence/UI tests; 0.3 evidence | ImplementedAndVerified |
| LF-SRH-001..002 | Global search and combinable catalog filters | SQLite read model, SearchForm, catalog filters | search and performance tests; 0.4/0.5 evidence | ImplementedAndVerified |
| LF-AUD-001 | Immutable readable business change history | append-only writers, `SqliteChangeEventReader`, `ProjectHistoryForm` | audit/redaction/projection/UI tests; 0.5 evidence | ImplementedAndVerified |
| LF-LOG-001 | Data-minimal correlated diagnostics | logging abstraction, JSONL provider, error IDs | logging/application tests; 0.0.1–0.4 evidence | ImplementedAndVerified |
| LF-BAK-001..002 | Versioned backup and controlled restore | recovery ports/service and UI | real-SQLite recovery/fault tests; manual visible restore M-10 | ImplementedButManualVerificationOutstanding |
| LF-EXP-001..002 | Complete JSON and readable Markdown | portable exchange 1.0 and atomic writers | portable exchange/schema tests; manual visible export M-08 | ImplementedButManualVerificationOutstanding |
| LF-UI-001 | Master-detail navigation | `MainForm` and Project workspaces | WinForms structure tests; M-01..M-08 | ImplementedButManualVerificationOutstanding |
| LF-VAL-001 | Central validation | Domain invariants and application results | Domain/application negative tests | ImplementedAndVerified |
| LF-SEC-001..002 | Safe targets and no secrets | centralized validator/revalidation/opener | `ReferenceTargetValidatorTests`, persistence negatives; M-09 | ImplementedButManualVerificationOutstanding |
| LF-SEC-003 | Visible local paths and selectable outputs | `OperatingPathsInfo`, read-only operating-information UI | WinForms structure test; M-11 | ImplementedButManualVerificationOutstanding |
| LN-BED-001..002 | Learnability and consistent interaction | native German UI, menu/F1/help | automated structure tests; M-15 usability/language | ImplementedButManualVerificationOutstanding |
| LN-BED-003 | Error tolerance on implemented MVP inputs | validation, safe open, atomic export, staged recovery, stale-write handling | negative suites; damaged import moved to 1.1 by Decision 0.5 | ImplementedAndVerified |
| LN-A11Y-001..002 | Keyboard, DPI and contrast | mnemonics, tab order, accessible metadata, DPI layouts | simulated structure tests; M-01/M-12 physical checks | ImplementedButManualVerificationOutstanding |
| LN-SEC-001 | Data minimization | bounded fields, audit redaction, no secret storage | security/audit tests and inventory | ImplementedAndVerified |
| LN-SEC-002 | Local data sovereignty | local SQLite/filesystem, no provider/server dependency | architecture/package review; M-13 offline | ImplementedButManualVerificationOutstanding |
| LN-SEC-003 | Transactional integrity | SQLite transactions, migrations, atomic export, staged restore | migration/recovery/fault tests | ImplementedAndVerified |
| LN-REL-001 | Robust operation | controlled errors and recovery | automated negatives; M-14 four-hour test | ImplementedButManualVerificationOutstanding |
| LN-REL-002 | Documented recovery | recovery service and runbook | automated restore; M-10 empty-profile restore | ImplementedButManualVerificationOutstanding |
| LN-PERF-001 | P95 under two seconds | measured Project list/filter/context/search paths | `SearchPerformanceBaselineTests`; 0.5 evidence | ImplementedAndVerified |
| LN-MNT-001..002 | Boundaries and repeatable verification | four production projects, architecture suite, this matrix | full suite and GitHub Quality | ImplementedAndVerified |
| LN-PORT-001 | Portable user data | exchange 1.0 JSON and Markdown | exchange tests/evidence | ImplementedAndVerified |
| LN-LOC-001 | German UI | German forms/messages/help | structure checks; M-15 complete visible review | ImplementedButManualVerificationOutstanding |
| LN-OPS-001 | Portable Windows deployment | self-contained win-x64 ZIP, docs, checksum, SBOM | packaging gate; M-16 clean-system install/remove | ImplementedButManualVerificationOutstanding |
| LN-OPS-002 | Offline operation | no server/cloud dependency | architecture/package checks; M-13 | ImplementedButManualVerificationOutstanding |
| LN-COMP-001 | Windows 11 x64 | win-x64 target and PerMonitorV2 | build/smoke; M-17 two configurations | ImplementedButManualVerificationOutstanding |

Import and the damaged-import example are `NotApplicableToMvp`; import remains Release 1.1. Detailed automated results are recorded in `EVIDENCE.md`; every manual outstanding item maps to `MANUAL-ACCEPTANCE.md`.
