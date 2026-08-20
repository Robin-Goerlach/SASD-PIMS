# Implementation evidence — 0.2.0

**Evidence date:** 2026-08-20

**Release source commit:** `9090d50`

**Branch:** `codex/0.2.0`

**Status:** automated milestone gates passed; manual visual and endurance checks noted below

## Delivered behavior

- controlled Project Phase and Activity State remain independent from each other and from Archive;
- review facts are persisted while freshness is derived from controlled UTC time;
- optional target dates use the fixed 14-calendar-day boundary and completed/cancelled neutrality;
- NeedsAttention exposes review-overdue, open-blocker and target-overdue reasons;
- blockers can be added, inspected and conditionally resolved while retaining resolution history;
- German catalog indicators, attention filter, steering workspace and local F1 glossary;
- no Requirement, typed-reference or later-milestone feature was introduced.

## Database, migration and recovery evidence

Current schema: `202608200002_ProjectSteering`.

The additive migration preserves the accepted 0.1 `Projects` and `ProjectTags` data, adds conservative
`Idea`/`NotStarted` defaults and nullable review/target facts, then creates `ProjectBlockers`. The
real-SQLite evolution test constructs the exact 0.1 physical schema and migration history, inserts an
existing Project, creates a verified pre-migration backup, migrates, verifies old data, adds 0.2 data,
reconstructs services and verifies the reopened result. Blocker resolution uses a conditional database
update so two editors cannot silently replace retained resolution evidence.

The recovery suite passed for schema `202608200002_ProjectSteering`, including manifest/checksum/schema
compatibility, SQLite integrity, staged promotion, rollback and negative replacement paths.

## Automated release gate

`scripts/build-0.2.0.ps1` completed from `9090d50`:

- clean and restore: passed;
- Release build: passed with 0 warnings and 0 errors;
- tests: 86 passed, 0 failed, 0 skipped;
  - Domain: 41;
  - Application: 12;
  - Integration/export/migration/recovery: 20;
  - Architecture: 6;
  - WinForms baseline: 7;
- vulnerable packages: none reported for all nine projects including transitive dependencies;
- package scan: no database, log, backup, `.env`, token or secret file found.

Architecture tests retain project-reference direction, cycle prevention, Domain isolation and the
prohibition on normal WinForms `DbContext` ownership.

## Release artifact

- `artifacts/release/0.2.0/SASD-PIMS-0.2.0-win-x64.zip`;
- size: 53,185,986 bytes;
- SHA-256: `DBD2251391E5D20E3922682539F6B9D28068EFAF5293E0E27237EA402261F40B`;
- self-contained `win-x64`; checksum generated and independently re-read by the build script.

## Windows smoke evidence

The packaged executable started against an isolated synthetic `LOCALAPPDATA`, reached input-idle and
reported a responsive window process. `CloseMainWindow` did not close the hidden smoke window within
ten seconds, so that synthetic process was explicitly terminated. Normal visible File/Close-window
shutdown remains a manual release check; no production user data was accessed.

Automated UI checks cover keyboard controls, accessible names, German validation/help semantics and
contained layouts at simulated 100%, 125%, 150% and 200% scaling. Physical-monitor visual checks, a
full manual create/steer/review/blocker/archive/restore walkthrough and the endurance test remain manual.

## Dependencies and decisions

No new direct NuGet dependency was added. Review freshness, due indication and attention remain derived
instead of persisted. Catalog filtering stays in-memory for the early local catalog. SQLite blocker
history ordering is performed after the small per-project result is loaded because SQLite does not
translate `DateTimeOffset` ordering. No caching, paging or generic workflow framework was introduced.

The JSON export remains schema `1.1-internal`; adding 0.2 fields without a separately approved exchange
format revision would create an undocumented compatibility change.

## Known limitations and debt

- Physical multi-monitor DPI inspection, full manual workflow and endurance testing are not automated.
- The hidden-window smoke harness cannot prove normal visible shutdown; startup responsiveness is proven.
- In-memory catalog filtering and client-side blocker-history ordering should be measured before any
  server-side query/paging optimization.
- UI strings can move to `.resx` when localization is scheduled; this is not required for German 0.2.
- No known data-loss, recovery, security or architecture defect remains open.

Before 0.3.0, approve the typed-reference and Requirement semantics defined by that milestone. Do not
extend the 0.2 blocker model into tasks or begin 0.3 implementation before explicit acceptance.
