# SASD PIMS — Decision Note 0.2.0: Project steering semantics

**Status:** Approved for implementation  
**Milestone:** `0.2.0 — Status, reviews and blockers`  
**Date:** 2026-08-20

## Purpose

This decision fixes the business semantics required before implementing `0.2.0`.

PIMS must distinguish lifecycle, activity, review freshness, blockers, due-date proximity and archive state instead of collapsing them into a generic project `Status` or a vague `Health` field.

The decisions below are binding for `0.2.0` unless a later explicit project decision changes them.

## 1. Project phase

UI term: **Projektphase**

Canonical values:

| ID | German label | Meaning |
|---|---|---|
| `Idea` | Idee | A project idea is being explored or clarified. |
| `Preparation` | Vorbereitung | Preconditions, scope and setup are being prepared. |
| `Execution` | Durchführung | The substantive project work is being carried out. |
| `Validation` | Validierung | Results are being checked, tested or accepted. |
| `Closure` | Abschluss | Finalization, handover and closing work is taking place. |

The phase answers:

> Where is the project in its lifecycle?

It does not answer whether somebody is currently working on it.

The vocabulary is finite and controlled. A version-controlled JSON resource may provide labels, help descriptions, order or localization, but users must not add arbitrary phase values.

## 2. Activity state

UI term: **Aktivitätszustand**

Canonical values:

| ID | German label |
|---|---|
| `NotStarted` | Noch nicht begonnen |
| `Active` | Aktiv |
| `Paused` | Pausiert |
| `Completed` | Abgeschlossen |
| `Cancelled` | Abgebrochen |

The activity state answers:

> Is work currently taking place, and in what broad activity condition?

Project phase and activity state are independent.

Examples:

- `Closure + Active` is valid while closing work continues.
- `Execution + Paused` is valid.
- `Completed` does not automatically archive the project.
- reactivating an archived project does not silently change its activity state.

No generic configurable workflow/state-machine framework is required for `0.2.0`.

## 3. Archive state

Archive remains independent.

Archive means that the project is removed from the normal active catalog view while retained for later access.

Therefore:

- `Archived != Completed`
- `Archived != Paused`
- `Archived != Cancelled`

No hard delete is introduced.

## 4. Review freshness

Persist facts:

- `LastReviewedAtUtc?`
- `NextReviewDueAtUtc?`

Do not persist an authoritative `ReviewStatus`.

A derived code concept such as `ReviewFreshness` is encouraged for readability.

Expected derived values:

- `NotScheduled`
- `NotReviewed`
- `Current`
- `DueToday`
- `Overdue`

The value is recalculated from persisted dates and the current time.

Reason:

A persisted value such as `Current` could silently become stale after the due date passes without any write to the database.

`0.2.0` does not require a complete review-history subsystem.

## 5. Blockers

A blocker is a concrete obstacle that materially impedes project work.

Minimum information:

| Field | Required |
|---|---|
| stable blocker identity | yes |
| Project identity | yes |
| Summary | yes |
| Details | no |
| CreatedAtUtc | yes |
| ResolvedAtUtc | no |
| ResolutionNote | no |

A blocker is open when `ResolvedAtUtc` is null.

The UI must allow the user to see **what the project is blocked by**, not merely `HasBlocker = true`.

Out of scope for `0.2.0`:

- assignee;
- priority matrix;
- subtasks;
- Kanban workflow;
- comments;
- attachments;
- task-management features.

## 6. Target date and due-date indication

A Project may have an optional **Zieldatum / TargetDate**.

PIMS derives a **Terminlage**.

Default rules:

| Condition | Presentation |
|---|---|
| no target date | neutral |
| more than 14 calendar days remaining | green + text `im Plan` |
| 1–14 calendar days remaining | yellow + text `bald fällig` |
| due today | yellow + text `heute fällig` |
| target date passed | red + text `überfällig` |
| activity state Completed or Cancelled | neutral |

The colour is never shown without explanatory text or equivalent accessible semantics.

The due-date indication means **date proximity only**. It must not be presented as an overall project-health score or as evidence that execution progress is on schedule.

Do not persist colour or derived due-state.

The 14-day threshold is deliberately fixed for `0.2.0`. Do not create a configuration subsystem yet.

## 7. No generic project health field

Do not introduce:

```text
Health = Green / Yellow / Red
```

A generic health colour is too ambiguous.

If a user wants qualitative project-health commentary, that belongs in project documentation/notes rather than an unexplained persisted traffic-light value.

## 8. Needs attention

A derived `NeedsAttention` projection is allowed.

For `0.2.0`, a project needs attention if at least one condition is true:

- review is overdue;
- one or more blockers are open;
- target date is overdue.

The UI should expose the concrete reason(s).

## 9. Menu and local help

PIMS should provide a conventional menu so that major functions have a predictable place.

Suggested initial structure:

```text
Datei
├─ Neues Projekt
├─ Exportieren
├─ Sicherung erstellen
├─ Sicherung wiederherstellen
└─ Beenden

Projekt
├─ Bearbeiten
├─ Archivieren
├─ Reaktivieren
└─ Als geprüft markieren

Ansicht
├─ Aktualisieren
├─ Archivierte Projekte anzeigen
└─ Aufmerksamkeit erforderlich

Hilfe
├─ PIMS-Hilfe (F1)
├─ Glossar
└─ Über SASD PIMS
```

Exact placement may be adjusted for normal WinForms usability, but every major function must remain discoverable via menu or an equally obvious primary UI control.

PIMS remains local-first/offline-first. Core help must ship locally with the application.

Use three levels:

1. tooltip;
2. short contextual field help where useful;
3. local help/glossary.

The help must explicitly explain the distinction between:

- Project Phase;
- Activity State;
- Review Freshness;
- Blocker;
- Archive.

## 10. Documentation and source readability

The existing repository commenting policy remains binding.

For this milestone, comments are particularly valuable around:

- derived review freshness;
- due-date calculation;
- archive/activity independence;
- concurrency when editing steering information;
- blocker resolution;
- migration from `0.1.0`.

Prefer XML documentation for public domain/application APIs and `//` comments that explain why non-obvious logic exists.

Do not over-comment trivial C# syntax.

## 11. Scope boundary

`0.2.0` must not introduce:

- generic task management;
- Requirements;
- Product lifecycle;
- Risks;
- Decisions;
- Milestones/Releases as managed domain modules;
- broad dashboards/charts;
- GitHub/provider integration;
- cloud sync;
- plugin framework;
- configurable workflow engine.

The goal is a useful lightweight project-steering layer on top of the accepted `0.1.0` catalog.
