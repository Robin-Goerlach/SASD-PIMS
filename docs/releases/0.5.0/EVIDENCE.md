# Implementation evidence — 0.5.0 release candidate

**Evidence date:** 2026-08-21  
**Packaged source commit:** `a2a56060dc6b716ace242e342dc4c3d3f9d7f233`  
**Branch:** `codex/0.5.0`  
**Status:** automated technical gates passed; mandatory manual acceptance remains PENDING

## Delivered and conflicts resolved

- LF-BLK-001: blocker Impact and open NextAction are required; Cause/AffectedObject are optional; optional external task must be an existing same-Project `ExternalTask`. No Task subsystem was added.
- Migration `202608210005_FullMustMvp` preserves every 0.4 blocker. New fields are nullable only for honest legacy history; no artificial facts are backfilled. Three SQLite triggers enforce the external-task invariant and deletion protection.
- LF-AUD-001: Project menu exposes a read-only ChangeEvent view with timestamp, entity, readable identifier, event and stored redacted values. No edit/delete/undo/event sourcing exists.
- LF-SEC-003: Help exposes read-only database, log, application and recovery locations and explains user-selected backup/export destinations.
- LN-BED-003: damaged import is not applicable to this MVP; import remains Release 1.1. Implemented validation, reference, export, recovery, SQLite and concurrency negative paths were verified.

## Automated gates — passed

- clean, restore and Release build: passed, 0 warnings / 0 errors;
- 131 tests passed, 0 failed, 0 skipped: Domain 49, Application 31, real SQLite Integration 35, Architecture 6, WinForms 10;
- real 0.4 → 0.5 migration, legacy blocker preservation, current-schema reopen, backup/restore, corruption/checksum/schema/integrity rejection, rollback/fault injection and stale writes: passed;
- audit whitelist/redaction/read projection, blocker DB/application invariants, reference schema/secret/missing-target negatives and atomic export: passed;
- architecture boundaries and automated German keyboard/accessibility/DPI structure: passed;
- direct/transitive vulnerability scan across all nine projects: no vulnerable packages reported by configured NuGet sources;
- self-contained `win-x64` restore/publish, package hygiene, SPDX SBOM generation and independent ZIP checksum verification: passed;
- `git diff --check`: passed.

The release packaging run used `-SkipTests -SkipVulnerabilityCheck` after its clean Release build because both gates had already been executed separately with full output in the same source state. No gate is claimed from a skipped command.

## Performance

Environment: Windows NT 10.0.26200.0, 8 logical CPUs, .NET 10.0.11. Real SQLite fixture: 500 Projects, 10,000 Requirements, 20,000 ExternalReferences, 2,000 Blockers and 10,000 ChangeEvents. Seed: 15,741.7 ms. Across warmed typical interactions: median 32.0 ms, P95 126.1 ms, maximum 129.5 ms.

| Interaction | Median | P95 | Maximum |
| --- | ---: | ---: | ---: |
| Global search | 39.8 ms | 126.1 ms | 129.5 ms |
| Project list | 30.3 ms | 44.0 ms | 44.0 ms |
| Combined Project filter | 27.3 ms | 35.8 ms | 35.8 ms |
| Project-context load | 6.2 ms | 8.4 ms | 8.4 ms |

Target P95 below 2,000 ms passed. No FTS5, paging redesign or speculative optimization is justified.

## RC artifact, dependencies and SBOM

- ZIP: `artifacts/release/0.5.0/SASD-PIMS-0.5.0-win-x64.zip`;
- size: 53,316,016 bytes;
- SHA-256: `C6E2FD520681AF2D455485BEF9778B2850084B5A57EB8852FF32807B4BA4B946`;
- package manifest source: `a2a56060dc6b716ace242e342dc4c3d3f9d7f233`;
- SPDX 2.3 SBOM: `SASD-PIMS-0.5.0.spdx.json`, 37 unique package/version entries;
- dependency inventory: nine projects with direct/transitive graph; licence summary in `DEPENDENCIES.md`;
- no runtime dependency was added in 0.5.0.

## MUST matrix

`MUST-TRACEABILITY.md` covers every Lastenheft MUST from requirement to implementation, tests/evidence and status. There are zero `SpecificationConflict`, `PartiallyImplemented` or `NotImplemented` MUST entries. Automated requirements are `ImplementedAndVerified`; requirements needing physical/visible acceptance are `ImplementedButManualVerificationOutstanding`. Import is `NotApplicableToMvp` by Decision 0.5.

## Manual gates — PENDING

All numbered cases in `MANUAL-ACCEPTANCE.md` remain PENDING: complete keyboard and visible Project/steering/Requirement/reference/search/traceability/audit/export/backup/restore flows; physical 100/125/150/200% DPI and monitor move; contrast/focus; real shell opening; visible start/shutdown; offline run; empty-profile restore; clean-system portable first start/removal; two Windows 11 x64 configurations; four-hour endurance; ≥80% usability after ≤30-minute introduction; and complete visible German consistency. Codex has not marked any of these as passed.

## GitHub Quality

Pending final pushed-branch run; this section will be updated only from an observed completed workflow.

## Technical debt

Release blockers: the mandatory manual gates above. No known automated data-loss, recovery, security, architecture or performance blocker remains.

Non-blockers: search shows at most 200 results without interactive follow-up pages; broader SHOULD/MAY machine-readable traceability remains beyond the completed MUST matrix; physical acceptance and optional screen-reader spot check remain manual. The four-hour helper is prepared but was not run by holding an interactive Codex session.

No merge to `main`, `v0.5.0` tag or GitHub Release was created. Milestone 0.6.0 was not begun.
