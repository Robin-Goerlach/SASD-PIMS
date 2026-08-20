# Implementation evidence — 0.4.0

**Evidence date:** 2026-08-20

**Packaged source commit:** `43f1d26`

**Branch:** `codex/0.4.0`

**Status:** local automated milestone gates passed; GitHub Quality is recorded at final handoff after the branch push

## Delivered behavior

- local global and Project-scoped search for Projects, Requirements, Blockers and ExternalReferences;
- server-side filtering through bounded read models, stable sorting, type and Project filters, controlled Requirement/reference filters, and a visible further-results indication;
- combinable Project filters for activity, phase, review freshness, type, area and tags, with a prominent reset action;
- Project traceability projected only from existing Project, Requirement, AcceptanceCriterion, VerificationReference, SourceReference, ExternalReference and Blocker relationships;
- direct navigation from search and traceability results to the relevant native WinForms workspace;
- append-only selective ChangeEvents for the approved Project, Requirement, Blocker and ExternalReference whitelist, without Event Sourcing or sensitive full-text values;
- atomic, deterministic complete JSON exchange export with `formatId` `sasd-pims-exchange` and `formatVersion` `1.0`;
- versioned JSON Schema at `docs/exchange/sasd-pims-exchange-1.0.schema.json`;
- deterministic Markdown Project profile export;
- local absolute file/directory references remain unchanged and are explicitly marked machine-local;
- no import, merge framework, Requirement relationships, generic relationship engine, external search service, FTS5, embedded files, provider integration, synchronization or report archive.

## Database, migration and recovery evidence

Current schema: `202608200004_SearchTraceabilityAndExchange`.

The additive migration creates only `ChangeEvents` and targeted search/audit indexes. It creates no search-result,
traceability, export or FTS table. The migration test constructs the exact 0.3 physical boundary, including its
migration-history row and representative Project, Requirement and reference state, upgrades through the production
`DatabaseMigrator`, verifies preservation, and reopens the upgraded database through fresh services.

Real SQLite tests passed for migration, reopen, pre-migration backup, manifest/checksum/schema/integrity validation,
staged restore, rollback and recovery failure paths. Search, traceability and exchange integration tests also use
real isolated SQLite files rather than EF Core InMemory.

## Automated release gate

`scripts/build-0.4.0.ps1` completed from packaged source commit `43f1d26`:

- clean and restore: passed;
- Release build: passed with 0 warnings and 0 errors;
- tests: 126 passed, 0 failed, 0 skipped;
  - Domain: 47;
  - Application: 30;
  - real SQLite integration/migration/recovery/performance: 34;
  - Architecture: 6;
  - WinForms baseline: 9;
- architecture dependency and WinForms `DbContext` ownership rules: passed;
- vulnerable packages, including transitives: none reported for all nine projects;
- self-contained `win-x64` restore and publish: passed;
- package scan: no database, log, backup, `.env`, token or secret file found;
- `git diff --check`: passed.

## Search performance baseline

The checked-in reproducible xUnit baseline seeds and queries an isolated real SQLite database.

- environment: Microsoft Windows NT 10.0.26200.0, 8 logical CPUs, .NET 10.0.11, x64;
- corpus: 500 Projects, 10,000 Requirements, 20,000 ExternalReferences, 2,000 Blockers and 10,000 ChangeEvents;
- operations: 20 warmed representative global/Project-scoped searches and combined filters;
- seed: 6,637.1 ms;
- minimum: 8.5 ms;
- median: 36.8 ms;
- P95: 90.0 ms;
- maximum: 110.8 ms;
- target: P95 below 2,000 ms — passed.

The implementation therefore remains on parameterized EF Core/SQLite substring queries and targeted indexes.
There is no measurement justification for FTS5. The ChangeEvent volume is intentionally independent of the searched
entities; the fixture establishes the additive audit shape and can be increased toward 100,000 events without changing
the search workload.

## Exchange and security evidence

Tests verify stable relationship IDs, canonical language-neutral controlled values, deterministic collection order,
format ID/version, JSON Schema parsing and representative schema conformance, local-reference markers, unchanged local
targets, Markdown determinism, atomic replacement, and failure cleanup. Exports do not read or embed referenced local
files. ChangeEvent tests verify whitelist generation and that descriptions, reasons and complete reference targets do
not enter old/new audit values.

## Release artifact

- `artifacts/release/0.4.0/SASD-PIMS-0.4.0-win-x64.zip`;
- size: 53,300,432 bytes;
- SHA-256: `C156C77ED8F0736E85A92357F60C43C5E35480B88435E46F3488687AF367F644`;
- self-contained `win-x64`; manifest, schema identifier and checksum were generated and independently rechecked.

## Windows and UI evidence

Automated WinForms checks cover global-search and traceability menu access, Ctrl+F, F1/help and glossary text,
accessible names/descriptions, keyboard controls, native contained layouts and the existing simulated 100%, 125%,
150% and 200% scaling baseline. Release publish/package completed on Windows x64.

A visible packaged start/save/navigation/close walkthrough was not automated because the application resolves the real
Windows Known Folder database path; launching it under the build identity would risk touching existing user data.
Physical multi-monitor Per-Monitor V2 DPI inspection, Save-dialog interaction, screen-reader behavior and visual focus
order remain explicit manual acceptance checks.

## Dependencies, limitations and debt

- No new direct NuGet dependency was added.
- Global search executes one bounded server-side query per selected object type and merges only those projections for
  final cross-type ordering; it does not load complete domain collections. A SQL union or FTS remains unjustified while
  the measured baseline is comfortably inside target.
- Search returns at most 200 results in the UI (application hard limit 500) and clearly indicates additional matches;
  interactive page traversal is deferred until measured usage requires it.
- The benchmark holds 10,000 ChangeEvents rather than 100,000 because audit rows are not search inputs in 0.4.0; a
  larger audit-only endurance run remains a future scale check, not a hidden relaxation of the measured search target.
- JSON import, preview, merge and conflict intake remain explicitly planned outside 0.4.0.
- No known data-loss, recovery, security, performance-target or architecture defect remains open.

0.5.0 was not started. No merge, `v0.4.0` tag or GitHub Release was created.
