# SASD PIMS 0.4.0 — Release Notes

`0.4.0` makes the accumulated local information searchable, navigable and portable.

- bounded server-side SQLite search across Projects, Requirements, Blockers and ExternalReferences;
- global and Project context, sorting and controlled filters;
- native traceability tree projected from existing source, verification, reference and blocker relations;
- selective append-only ChangeEvents with sensitive-value redaction;
- deterministic atomic `sasd-pims-exchange` JSON 1.0 plus versioned JSON Schema;
- deterministic Markdown Project profile;
- additive 0.3-to-0.4 migration and realistic synthetic performance evidence.

There is no import, merge framework, Requirement relationship model, generic relationship/audit platform,
external search service, FTS5, provider integration, embedded file, report snapshot or event sourcing.
