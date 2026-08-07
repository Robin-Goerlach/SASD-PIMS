# SASD PIMS Documentation

This directory contains the version-controlled sources that govern implementation.

## Source hierarchy

1. `requirements/` — functional requirements / Lastenheft.
2. `specification/` — technical specification / Pflichtenheft.
3. `architecture/` — approved software architecture and ADR-related registers.
4. `baseline/BASELINE.md` — implementation approval record and conflict-resolution rules.
5. `database/`, `ui/`, `testing/` — implementation-near specifications.
6. `implementation/` — vertical-slice scope, Ready-to-Code state and initial execution plan.
7. `planning/roadmap-v0.1/` and `ROADMAP.md` — staged delivery planning.

If documents disagree:

- the Lastenheft remains authoritative for functional intent and priority;
- the Pflichtenheft defines the implementation interpretation unless it contradicts the Lastenheft;
- the architecture baseline defines system boundaries and dependency/quality rules;
- `baseline/BASELINE.md` records explicit reconciliations and implementation approvals;
- code does not silently override a governing document: a contradiction is a defect or requires an explicit change/ADR.

## Editing rules

- Keep editable Markdown and diagram sources in Git.
- Do not commit generated PDF/DOCX copies merely because they exist.
- Do not rewrite approved source documents silently. Record amendments, errata or superseding versions.
- Architecture acceptance and implementation evidence are separate states.
- Update traceability when a requirement, schema, format or architecture decision changes.
