# SASD PIMS Roadmap

This roadmap is intentionally incremental. It must not be read as a promise that all later features are already implemented.

## Detailed planning baseline

The detailed planning document is versioned at [`planning/roadmap-v0.1/documents/SASD-PIMS-Entwicklungsroadmap-und-Dokumentationsplan-v0.1.md`](planning/roadmap-v0.1/documents/SASD-PIMS-Entwicklungsroadmap-und-Dokumentationsplan-v0.1.md). The implementation baseline and P1 specifications in `baseline/`, `database/`, `ui/`, `testing/` and `implementation/` govern the immediate coding start.

## 0.0.1-internal — Vertical architecture slice

**Goal:** prove that the architecture can support one complete, testable workflow.

Target evidence:

- application starts and shuts down cleanly;
- one minimal project can be created and validated;
- data is persisted to SQLite and can be reopened;
- logging and user-visible failure handling exist;
- minimal export works;
- backup and staged restore work on synthetic test data;
- automated unit/integration/architecture tests run;
- a reproducible release artifact can be built.

Not in scope:

- broad project catalog UX;
- requirements management;
- advanced search;
- product pipeline;
- dashboards;
- external API integration.

## 0.1.0 — Practical project catalog

**Goal:** first version that is useful for maintaining local project master data.

Planned:

- project list;
- create/edit/view project;
- stable project identity;
- basic validation;
- archive/deactivate semantics if required by the requirements baseline;
- reliable reopen/restart behaviour;
- basic backup/recovery access;
- usable keyboard and DPI behaviour.

Not planned:

- complete requirement model;
- full portfolio/dashboard;
- product lifecycle;
- complex reporting.

## 0.2.0 — Status, reviews and blockers

**Goal:** support lightweight project steering without becoming a task manager.

Planned:

- project status;
- review/health information;
- blockers/attention indicators;
- focused status views.

## 0.3.0 — Requirements and typed references

**Goal:** create the traceability foundation.

Planned:

- structured requirements;
- acceptance information;
- typed references to repositories, chats, files, documents and URLs;
- safe opening/validation of reference targets;
- manual references as the default integration model.

No mandatory online provider API.

## 0.4.0 — Search, traceability and portable exchange

**Goal:** make accumulated information reliably retrievable and portable.

Planned:

- local search/filtering;
- relationship navigation;
- versioned export;
- import preview and validation;
- conflict handling for supported import cases.

## 0.5.0 — MVP

**Goal:** complete and verify the mandatory scope defined for the MVP.

Focus:

- functional completeness of mandatory MVP requirements;
- recovery and migration confidence;
- accessibility/DPI baseline;
- performance measurements with realistic synthetic data;
- release documentation.

## 0.6.0 — Governance information

Planned additions where still justified:

- milestones;
- decisions;
- risks;
- supporting views and traceability.

## 0.7.0 — Minimal product relationship

Introduce the minimal project/product relationship required by the domain without turning PIMS into a full product-management suite.

## 0.8.0 — Feature-complete beta

Feature freeze for the planned 1.0 scope. Focus shifts to defects, usability, migration, documentation, accessibility, performance and recovery.

## 0.9.0 — Release candidate

No new planned features. Release qualification only.

## 1.0.0 — Stable release

Promote a successfully accepted release candidate without slipping unvalidated features into 1.0.

## Optional post-1.0 candidates

Only after measured need and explicit decision:

- richer reporting;
- charts;
- Markdown preview;
- provider metadata adapters such as GitHub;
- enhanced import/export formats;
- additional reusable SASD UI components;
- update automation.

Cloud synchronisation, team collaboration, generic Kanban, arbitrary plugin systems and broad task management are not implicit post-1.0 commitments.
