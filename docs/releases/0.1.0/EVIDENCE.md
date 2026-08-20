# Implementation evidence — 0.1.0

**Evidence date:** 2026-08-20  
**Release source commit:** `f6f92c9`  
**Branch:** `codex/0.1.0`  
**Status:** automated milestone gates passed; manual visual checks noted below

## Delivered behavior

- German master-detail project catalog with list, selection, key/name search and archive visibility;
- create, view and edit complete 0.1 project master records;
- immutable normalised project key and stable GUID identity;
- goal, benefit, type/area codes, responsibility and normalised tags;
- explicit integer-revision conflict detection; stale writes do not overwrite newer data;
- reversible archive/reactivation with no normal hard-delete command;
- persistent SQLite reopen, versioned JSON export and verified backup/staged restore.

## Database and migration evidence

Current schema: `202608200001_ProjectCatalog`.

The additive migration retains the `Projects` table and adds nullable/defaulted master-data columns
plus the normalised `ProjectTags` relation. The automated evolution test recreates the accepted
`202608190001_InitialProject` schema, inserts an existing Project, creates a verified pre-migration
backup, migrates, verifies the old Project, adds new 0.1 data, reconstructs the complete persistence
stack and verifies both records after reopen. No database recreation is used as migration behavior.

Recovery tests run against the new schema and retain checksum, manifest, compatibility, SQLite
integrity, staged promotion and rollback protection, including negative replacement paths.

## Automated release gate

`scripts/build-0.1.0.ps1` completed from `f6f92c9`:

```powershell
dotnet clean Sasd.Pims.slnx -c Release
dotnet restore Sasd.Pims.slnx
dotnet build Sasd.Pims.slnx -c Release --no-restore
dotnet test Sasd.Pims.slnx -c Release --no-build
dotnet package list --vulnerable --include-transitive
dotnet publish src/Sasd.Pims.WinForms/Sasd.Pims.WinForms.csproj -c Release -r win-x64 --self-contained true
```

Results:

- clean/restore: passed;
- Release build: passed with 0 warnings and 0 errors;
- tests: 61 passed, 0 failed, 0 skipped;
  - Domain: 22;
  - Application: 9;
  - Integration/export/recovery: 18;
  - Architecture: 6;
  - WinForms baseline: 6;
- vulnerable packages: none reported for all nine projects and their transitive dependencies;
- package scan: no database, log, backup, `.env`, token or secret file found.

The architecture suite continues to enforce production project-reference direction, no cycles,
Domain isolation from WinForms/EF Core/Infrastructure and absence of concrete `DbContext` ownership
in normal WinForms code.

## Release artifact

- `artifacts/release/0.1.0/SASD-PIMS-0.1.0-win-x64.zip`
- size: 53,153,544 bytes;
- SHA-256: `1D5E76958463D4755C82341F2012D6382AA631FB4B0BDF219605D710A1DA4517`;
- checksum generated and independently re-read by the release script;
- publish model: self-contained `win-x64` ZIP.

## Windows smoke evidence

The packaged executable started as a responsive Windows process. The structured production log
recorded `ApplicationStarted` for the package, after which the smoke process was controlledly
stopped. Automated UI tests cover keyboard order, German text validation, accessible names and
contained layouts at simulated 100%, 125%, 150% and 200% DPI scaling.

Not automated in this environment: visual inspection of the live window on physical 125/150/200%
monitors, a full manual create/edit/archive/restore walkthrough and the four-hour endurance test.

## Dependencies and small decisions

No new direct NuGet dependency was added. The 0.0.1 package/license set remains unchanged.

- Archiving is a separate reversible flag; phase/pause/discontinuation remain in the 0.2 status scope.
- Type and area use optional language-neutral validated codes without seeding an unproven vocabulary.
- Tags are a normalised relation; reference-data administration is deferred.
- Catalog filtering is intentionally in-memory for the early local catalog. Server-side querying or
  paging is deferred until measured data volume shows a need.

## Known limitations and debt

- Type/area/tag-specific filters exist in the Application query but the initial UI exposes only the
  most useful key/name and active/archive filters.
- UI strings are centralisable but not yet moved to `.resx`; this should be done when localization is
  scheduled, without delaying practical early use.
- No dedicated performance dataset was added; current simple local queries are adequate for the
  milestone and should be measured before indexing/caching work.
- No known data-loss, recovery, security or architecture defect remains open for 0.1.0.

Before `0.2.0`, decide the exact independent lifecycle phase and activity-state vocabularies and the
scope of review freshness/blocker information. Do not infer those semantics from the archive flag.
