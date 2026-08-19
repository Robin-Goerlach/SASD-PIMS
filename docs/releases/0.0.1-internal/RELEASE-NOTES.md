# SASD PIMS 0.0.1-internal

This is an internal architecture proof for Windows 11 x64. It is not a supported production release.

## Included

- create and centrally validate one minimal Project;
- persist to and reopen from real SQLite through EF Core migrations;
- list and open saved Projects in a minimal native WinForms UI;
- write correlated, data-minimised JSON Lines diagnostics;
- export one Project as versioned JSON;
- create a checksum-verified backup ZIP;
- stage, validate and safely restore a backup with rollback evidence.

## Run

Extract the self-contained ZIP and run `Sasd.Pims.WinForms.exe`. No separate .NET runtime is required.

Runtime data remains outside the program directory:

- database: `%LOCALAPPDATA%\SASD\PIMS\data\pims.db`;
- logs: `%LOCALAPPDATA%\SASD\PIMS\logs`;
- automatic pre-migration and restore rollback backups: `%LOCALAPPDATA%\SASD\PIMS\backups`.

User-selected exports and backups are written only to the selected path.

## Migration and recovery

The first start creates the P1 schema through EF Core migration `202608190001_InitialProject`.
Before a later migration touches an existing database, the application creates a verified
pre-migration backup. Restore candidates are validated in staging before active data changes.

## Known limitations

- only the `0.0.1-internal` vertical-slice workflow is implemented;
- Project editing and the broader project catalog belong to `0.1.0`;
- the ZIP and executable are not code-signed;
- clean-environment and multi-configuration Windows smoke evidence is still recorded separately;
- no installer or automatic updater is included.
