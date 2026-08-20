# SASD PIMS 0.1.0 — Release Notes

`0.1.0` is the first practically usable local project-catalog release for Windows 11 x64.

Available functions:

- create, list, select, view and edit project master records;
- stable project keys and explicit revision-conflict handling;
- search by key or name;
- type/area codes and tags without an unproven fixed vocabulary;
- reversible archive/reactivation; no project hard delete;
- persistent SQLite reopen and additive migration from `0.0.1-internal`;
- versioned JSON export;
- verified backup and staged restore with rollback protection;
- German native WinForms UI with keyboard, accessibility and DPI baseline.

Not included: status/reviews/blockers, requirements, risks, decisions, milestones, releases,
products, dashboards, cloud synchronisation, plugins or an installer.

Known limitations:

- project type/area are validated technical codes; controlled reference-data administration is deferred;
- tags can be entered and displayed; dedicated tag/type/area catalog filters are available in the
  Application layer but the initial UI exposes only the practical key/name and archive filters;
- visual DPI checks on multiple physical monitors and a four-hour endurance test remain manual gates.
