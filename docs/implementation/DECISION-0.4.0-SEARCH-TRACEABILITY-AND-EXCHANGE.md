# SASD PIMS — Decision Note 0.4.0: Search, traceability and portable exchange

**Status:** Approved for implementation  
**Milestone:** `0.4.0 — Search, traceability and portable exchange`  
**Date:** 2026-08-20

## Purpose

This decision fixes the smallest public search, traceability, audit and export contracts for 0.4.0. It supersedes the roadmap's import-preview sequencing for this milestone: 0.4.0 is export-only; import remains planned for Release 1.1.

## Search

Search is local and implemented through parameterized EF Core/SQLite read-model queries. It covers Projects, Requirements, ProjectBlockers and ExternalReferences globally and within one Project. Project key, name, short description, goal, benefit, responsibility and tags; Requirement key, title, description, rationale and source summary; the existing Blocker summary, details and resolution note; and ExternalReference title and target are searchable.

Object type and Project are global filters. Existing Project filters remain combinable. Results have stable entity/project identities, object type, project context, key/title and a concise match hint. Sorting and result limiting/paging happen in the query contract. WinForms must not load complete aggregates to filter them. Start with LIKE and targeted normal indices; FTS5 requires benchmark evidence showing that this is insufficient.

## Traceability

Traceability is a read-only projection of existing relationships only:

- Project → Requirement → AcceptanceCriterion → VerificationReference;
- Requirement → SourceReference;
- Requirement → ExternalReferences;
- Project → ExternalReferences;
- Project → Blockers.

There is no Requirement relationship, generic relationship table, graph engine or dependency vocabulary. The UI provides a native hierarchical view and navigation to represented business objects.

## Selective change events

`ChangeEvent` is additive, append-only from normal user operations and is not event sourcing. It contains stable Id, ProjectId, EntityType, EntityId, EventType, OccurredAtUtc and optional short redacted old/new values.

The whitelist is limited to Project phase, activity state, review-control facts, relevant target-date changes and archive/reactivate; Requirement priority, decision status and decision reason; Blocker creation/resolution; and relevant ExternalReference metadata changes. Full descriptions, rationale, targets, document/chat content and other sensitive full text are never stored as old/new values. ExternalReference target changes may record that the target changed, but not either target. Users cannot edit or delete change events.

## Portable exchange 1.0

The public envelope uses `formatId = "sasd-pims-exchange"` and `formatVersion = "1.0"`. Product, database-schema and exchange versions are independent. The full export contains portable Projects, tags, steering/review/target facts, Requirements, ordered AcceptanceCriteria, Blockers and resolution facts, ExternalReferences, source/verification relationships and ChangeEvents. Relationships use stable IDs; readable keys may supplement but never replace IDs. Controlled values are canonical language-neutral values and every collection is deterministically sorted.

The JSON contract is accompanied by a versioned JSON Schema. Writing uses a sibling temporary file, complete serialization and validation, flush, then atomic replace/rename. Failure must not expose a partial target. Secrets and prohibited data are excluded.

Absolute local file/directory targets remain unchanged and carry an explicit machine-local marker. Export does not read, copy, embed, relativize or transform referenced files. Relative workspace paths remain out of scope.

## Markdown project profile

The deterministic human-readable Project export contains identity/master data, phase/activity, review and due-date facts, tags, Requirements with priority/decision state and acceptance criteria, open/resolved Blockers, and Project/Requirement references. It is a generated export, not stored report history or an in-app Markdown preview.

## Performance baseline

The reproducible synthetic fixture contains approximately 500 Projects, 10,000 Requirements, 20,000 ExternalReferences, representative Blockers and is extensible toward 100,000 ChangeEvents. The target is that 95% of typical search/filter operations complete under two seconds on the documented environment. A miss is reported honestly and investigated before adopting FTS5 or another specialized optimization.

## Explicit non-goals

No JSON/CSV import, preview, merge/conflict framework, Requirement relationships, generic relationship engine, external search service, unmeasured FTS5, DMS/embedded files, provider integration, automatic link checking, synchronization, multi-user conflict resolution, portfolio dashboard, Product Pipeline, report snapshots or event sourcing is introduced in 0.4.0.
