# Changelog

All notable changes to SASD PIMS will be documented in this file.

The project intends to follow [Semantic Versioning](https://semver.org/) once release artifacts exist. Before 1.0, minor versions may still contain deliberate compatibility changes, but such changes must be called out in release notes and migration documentation.

## [Unreleased]

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
