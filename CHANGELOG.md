# Changelog

All notable changes to SASD PIMS will be documented in this file.

The project intends to follow [Semantic Versioning](https://semver.org/) once release artifacts exist. Before 1.0, minor versions may still contain deliberate compatibility changes, but such changes must be called out in release notes and migration documentation.

## [Unreleased]

## [0.3.0] - 2026-08-20

### Added
- Project-bound Requirements with immutable project-local keys, controlled priority, decision state and source.
- Persistently ordered acceptance criteria with optional verification references.
- Project and Requirement references with controlled semantic types and same-Project ownership rules.
- Central HTTPS/local/GitHub target validation, target-string secret rejection and safe OS opening.
- Native German Requirement/reference workspace, menu entry, F1 glossary and accessibility metadata.
- Additive real-SQLite migration from the released 0.2 schema.

### Changed
- Current database schema is `202608200003_RequirementsAndTypedReferences`.

### Security
- Reference targets are allowlisted by type and revalidated before opening; PIMS performs no network probe.

## [0.2.0] - 2026-08-20

### Added
- Independent controlled project phase and activity state.
- Derived review freshness, 14-day target-date indication and concrete attention reasons.
- Project blocker creation, resolution and retained resolution history.
- German steering workspace, attention filter and local F1 glossary.
- Additive SQLite migration from the accepted 0.1 schema.

### Changed
- Current database schema is `202608200002_ProjectSteering`.

### Security
- Pre-migration backup, staged restore and integrity verification cover the expanded schema.

## [0.1.0] - 2026-08-20

### Added
- Practical German master-detail project catalog with create, view, edit, search and selection.
- Goal, benefit, project-type/area codes, responsibility and normalised tags.
- Reversible archive/reactivation without a normal hard-delete operation.
- Optimistic revision conflict detection for project updates.
- Additive SQLite migration from the accepted 0.0.1 schema with verified pre-migration backup.
- Complete project-data JSON export schema `1.1-internal`.

### Changed
- Self-contained `win-x64` ZIP is the default local package.
- Current database schema is `202608200001_ProjectCatalog`.

### Security
- Migration, backup and restore preserve checksum, schema, integrity and rollback validation.

## [0.0.1-internal] - 2026-08-19

Internal architecture proof; not a supported production release.

### Added
- Minimal Project creation, validation, SQLite persistence and reload.
- Native WinForms project editor with keyboard/accessibility metadata and Per-Monitor V2 DPI mode.
- Structured local JSON Lines diagnostics with safe error correlation.
- Versioned JSON Project export.
- Verified SQLite backup packages and staged restore with rollback protection.
- Domain, application, integration, recovery and architecture tests.
- Windows build/test workflow and reproducible `win-x64` ZIP packaging script.

### Changed
- Select self-contained `win-x64` ZIP as the default package for `0.1.0` from measured evidence.

### Fixed

### Security
- Reject backup checksum, format, schema and SQLite-integrity failures before active data replacement.
