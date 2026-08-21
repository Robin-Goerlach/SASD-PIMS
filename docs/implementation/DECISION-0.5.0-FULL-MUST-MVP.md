# SASD PIMS — Decision Note 0.5.0: Full MUST MVP

**Status:** Approved for implementation  
**Milestone:** `0.5.0 — Full MUST MVP`  
**Date:** 2026-08-21

## Purpose

This decision closes the remaining MUST-MVP gaps without adding a new broad feature area. The decisions for 0.2.0 through 0.4.0 remain binding unless explicitly reconciled here.

## Blocker reconciliation (LF-BLK-001)

`ProjectBlocker` is extended additively with optional `Cause`, required `Impact`, optional free-text `AffectedObject`, required `NextAction` while open, and optional `ExternalTaskReferenceId`. The reference may point only to an existing `ExternalReference` of type `ExternalTask` in the same Project. It creates no internal Task.

Existing 0.2–0.4 rows are preserved. The migration adds the new columns as nullable because SQLite cannot derive missing historical business facts. For legacy rows, the application displays and exports an explicit legacy-information marker for missing `Impact` and, while open, `NextAction`. New and edited open blockers must supply both values. Resolved legacy rows may retain null values because resolution preserves the historical facts as they were recorded. No artificial cause such as `unknown` is stored.

Resolution sets `ResolvedAtUtc`; the optional `ResolutionNote` and all existing blocker facts remain. Assignees, priorities, subtasks, comments, Kanban, workflow and a Task subsystem remain out of scope.

## Error tolerance and import (LN-BED-003)

JSON/CSV import is not part of the 0.5.0 MUST MVP and remains planned for Release 1.1. The damaged-import acceptance example is therefore `NotApplicableToMvp` and moves with import to 1.1. 0.5.0 verifies error tolerance through the implemented inputs and failure paths: validation, reference target validation and opening, secret detection, export, backup, staged restore, SQLite/recovery and stale writes.

No import, preview, merge or conflict framework is introduced.

## Change history (LF-AUD-001)

Existing append-only `ChangeEvent` records receive a Project-scoped read-only application projection and native user view. It shows occurrence time, entity type, a readable object identifier where available, event type and only the already stored redacted old/new values. It cannot edit, delete, undo or reconstruct prohibited sensitive text and is not event sourcing or a generic audit platform.

## Local operating paths (LF-SEC-003)

A read-only operating-information view shows the active SQLite file, log directory, application directory and fixed recovery directory. Backup and export destinations are described as user-selected because no fixed destination exists. The view does not configure or mutate paths.

## Qualification

The complete current MUST traceability matrix is versioned with implementation, tests, evidence and acceptance status. Performance measurements extend the 0.4 synthetic fixture to Project list, combined filters, Project-context loading and global search. P95 for typical measured interactions must remain below two seconds. Manual UI, DPI, physical-system, offline and endurance checks remain explicitly `PENDING` until actually performed.

## Explicit non-goals

No import, Requirement relationships, generic relationship/audit engine, internal Task management, Product Pipeline, Products, Milestones, Decisions, Risks, portfolio dashboard, provider APIs, automatic link checking, synchronization, multi-user support, event sourcing, document management, embedded files, unmeasured FTS5, UI redesign or 0.6+ functionality is introduced.
