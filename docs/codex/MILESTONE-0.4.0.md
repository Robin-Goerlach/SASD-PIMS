# Codex milestone contract — 0.4.0

## Goal

Make accumulated PIMS information locally searchable, navigable through existing traceability, selectively auditable and portable through deterministic JSON and Markdown exports.

## Binding semantics

Read `docs/implementation/DECISION-0.4.0-SEARCH-TRACEABILITY-AND-EXCHANGE.md` before changing 0.4.0 code. The export-only boundary, search fields, relationship projection, audit whitelist/redaction and exchange 1.0 contract are binding. The 0.2.0 and 0.3.0 decisions remain binding for existing domain semantics.

## Required vertical scope

- server-side global and Project-scoped search with combinable filters, sorting and visible result limiting;
- native WinForms search/navigation plus Project traceability tree;
- additive ChangeEvents persistence written by normal use cases;
- deterministic atomic full JSON exchange 1.0 with versioned JSON Schema;
- deterministic Markdown Project profile;
- additive `0.3.0 → 0.4.0` migration and targeted indices;
- real-SQLite upgrade/reopen/recovery evidence;
- reproducible 500/10,000/20,000 synthetic performance baseline;
- menu, Ctrl+F, F1/help, accessibility, keyboard and DPI baseline;
- reproducible self-contained `win-x64` ZIP and SHA-256.

## Scope boundary

0.4.0 is export-only. Do not implement import/preview/merge, Requirement relationships, a generic relationship/audit platform, FTS5 without failed measured evidence, external services/providers, copied files, report snapshots, event sourcing or any 0.5.0 feature.

## Completion boundary

Push a clean `codex/0.4.0` branch and verify GitHub Quality. Do not merge to `main`, create `v0.4.0`, publish a GitHub Release or begin `0.5.0` without explicit approval.
