# SASD PIMS — UI-/UX-Spezifikation v0.1

**Status:** Ready for `0.0.1-internal`; target detail for `0.1.0`  
**UI technology:** Windows Forms / .NET 10  
**Supported baseline:** Windows 11 x64  
**Sources:** Lastenheft v0.1, Pflichtenheft v0.1, Architecture v1.0

> The generated dashboard image in `docs/assets/screenshots/` is a design concept, not proof of functionality and not the mandatory layout for the vertical slice.

## 1. UI goals

The initial UI should prove:

- a clear project context;
- predictable create/edit/save/cancel flows;
- consistent validation;
- keyboard operability;
- DPI-safe layout;
- separation from persistence/framework internals;
- useful error feedback without exposing technical details.

It must not attempt to implement the full dashboard shown in the concept image.

## 2. Component strategy

For P1 and 0.1:

- use standard WinForms controls first;
- no Ribbon framework;
- no full external UI suite;
- no docking framework;
- no charting library;
- no WebView/Markdown preview;
- compose small reusable UserControls only after a repeated pattern actually exists.

Candidate external components require separate need/licence/maintenance review.

## 3. `0.0.1-internal` screen set

Only two functional surfaces are required.

### 3.1 MainForm

Purpose:

- host application lifetime;
- display minimal project list/current state;
- start "New project";
- open an existing project;
- expose a small diagnostics/status surface needed for the PoC.

Minimal conceptual layout:

```text
┌───────────────────────────────────────────────────────────────┐
│ SASD PIMS                                                    │
├───────────────────────────────────────────────────────────────┤
│ [New project]                                                │
│                                                              │
│ Projects                                                     │
│ ┌───────────────────────────────────────────────────────────┐ │
│ │ SASD-PIMS   SASD PIMS                                   │ │
│ │ DEMO       Synthetic demo project                        │ │
│ └───────────────────────────────────────────────────────────┘ │
├───────────────────────────────────────────────────────────────┤
│ Ready / operation result                                    │
└───────────────────────────────────────────────────────────────┘
```

No dashboard cards, charts, requirements panels or product pipeline.

### 3.2 ProjectEditor

Purpose:

- create the minimal P1 Project;
- edit mutable P1 fields where applicable;
- show validation errors;
- save or cancel explicitly.

P1 controls:

| Field/control | P1 | Behaviour |
| --- | --- | --- |
| Project key | required | editable on create, read-only after persisted |
| Name | required | text input |
| Short description | optional | multiline |
| Validation summary | required | text/list, not colour-only |
| Save | required | initiates one application use case |
| Cancel | required | closes without persistence mutation |

Concept:

```text
Project

Key:               [____________________________]
Name:              [____________________________]

Short description:
[________________________________________________]
[________________________________________________]

Validation:
- shown here when required

                           [Save] [Cancel]
```

## 4. UI states

The Presenter/Controller must make state explicit rather than relying on ad-hoc event flags.

Minimum states:

- `Empty`;
- `Loading`;
- `Ready`;
- `EditingNew`;
- `EditingExisting`;
- `Saving`;
- `ValidationError`;
- `InfrastructureError`;
- `Closing`.

Longer operations later add `RunningOperation` / cancellable progress state.

## 5. Behaviour rules

### New project

1. User invokes New Project.
2. Editor opens in `EditingNew`.
3. Key receives initial focus.
4. Save validates Application/Domain rules.
5. Invalid fields remain editable; no database write occurs.
6. Valid data is sent to the create use case.
7. On success, editor closes or transitions to read/edit state and MainForm refreshes its projection.
8. On infrastructure failure, entered values remain available where safely possible.

### Open project

1. User selects a row.
2. User activates by Enter/double click/Open action.
3. Application loads Project by stable ID.
4. Editor displays persisted data.
5. Stable key is read-only after persistence.

### Cancel

Cancel does not persist.

If mutable values differ from the loaded state, a discard confirmation is allowed. Do not show a confirmation when nothing changed.

## 6. Validation presentation

Required:

- invalid input cannot be saved;
- field-level indicator via `ErrorProvider` or equivalent native pattern;
- one readable validation summary;
- errors identify the field and actionable problem;
- no error conveyed only through colour;
- focus can be moved to the first invalid field;
- same invalid value receives the same semantic judgement across screens.

P1 validation must include:

- blank key;
- blank name;
- duplicate key (persistence/application conflict);
- unexpected infrastructure error.

## 7. Error model in UI

Separate:

1. validation errors — user can correct input;
2. conflict/not-found — state changed or requested object unavailable;
3. infrastructure/recovery errors — operation failed, data must not be assumed written;
4. unexpected defect — show safe generic message plus correlation/error ID.

Do not display stack traces or secrets in normal dialogs.

## 8. Command enabling

Avoid "click and then discover obvious invalid state" where state can be known beforehand.

Examples:

- Open disabled when no project selected.
- Save remains available when editable, but failed validation is clear; alternatively disable only for structurally incomplete states if accessibility/usability tests support that choice.
- Restore cannot start while a write/restore/migration operation is already active.

Command rules belong to Presenter/Application state, not random control event chains.

## 9. Keyboard requirements

P1 must be usable without a mouse for the core create/save/open flow.

Minimum:

- logical `TabIndex`;
- mnemonic labels/actions where useful;
- Enter activates the primary action when not ambiguous;
- Escape cancels/closes where safe;
- selection/open works via keyboard;
- focus is visibly discernible;
- validation does not trap keyboard focus.

## 10. Accessibility baseline

Use native controls where possible.

For custom/composite controls:

- set meaningful `AccessibleName`;
- use `AccessibleDescription` only when it adds information;
- labels are associated with input fields;
- statuses are textually available;
- colour is supplemental, never the sole channel;
- important information is not screenshot-only.

Manual smoke verification should include Windows accessibility inspection tooling when practical.

## 11. DPI and layout

Application baseline:

- Per-Monitor V2 DPI awareness;
- `AutoScaleMode = Dpi` unless a tested form-specific reason says otherwise;
- use Dock/Anchor/TableLayoutPanel/FlowLayoutPanel deliberately;
- avoid pixel-perfect fixed layouts;
- avoid deep nested layout-panel hierarchies;
- do not clip required labels/buttons at 200%.

Required manual checks for P1:

- 100%;
- 125% or 150%;
- 200%.

For the 0.1 release, test at least two supported Windows 11 configurations.

## 12. Language

German is the default UI language for the product baseline.

Persist language-neutral codes/IDs, not translated status strings.

User-visible strings should be prepared for `.resx` centralisation when the first forms stabilise; P1 need not implement a complete localisation system if that would delay the architecture proof.

## 13. `0.1.0` project-catalog expansion

After P1 is proven, `0.1.0` adds a practical Master-Detail project catalog.

### Project list

Minimum:

- Key;
- Name;
- selected lifecycle/status information once defined;
- type/area if implemented;
- clear empty state;
- create/open/edit actions.

Filtering/sorting should be added only for fields actually present in 0.1.

### Project editor

Expand toward the Lastenheft project master record:

- Key (immutable);
- Name;
- Short description;
- Goal;
- Benefit;
- project type;
- project area;
- responsibility;
- status/lifecycle fields approved for 0.1;
- tags if included in the 0.1 baseline.

Do not expose future Requirement/Risk/Decision tabs merely as empty placeholders.

## 14. Archive/delete UX

P1 has no destructive delete command.

Before adding archive/deactivate in 0.1:

- define semantics in domain/database specification;
- show clearly that history/data remains;
- make reactivation discoverable if supported;
- reserve destructive database cleanup for explicit maintenance rules, not normal UI.

## 15. Backup/restore UI for P1

The PoC may use a deliberately simple dialog/menu entry.

Backup:

- user chooses target;
- operation reports success/failure and file path;
- no silent overwrite without confirmation.

Restore:

- clearly identifies selected backup;
- validates before destructive switch;
- explicitly warns that active data will be replaced only after validation;
- reports that a rollback copy was created;
- invalid backup never proceeds to replacement.

A polished wizard is not required for P1.

## 16. UI test IDs

| ID | Requirement |
| --- | --- |
| UI-P1-001 | New project can be completed by keyboard |
| UI-P1-002 | Key/name validation is understandable and not colour-only |
| UI-P1-003 | Duplicate key preserves entered data and explains conflict |
| UI-P1-004 | Reopened Project shows persisted values |
| UI-P1-005 | 200% scaling does not clip required information |
| UI-P1-006 | Form does not access `DbContext` directly |
| UI-P1-007 | Infrastructure failure displays safe message + correlation ID |
| UI-P1-008 | Concept screenshot is not presented as implemented UI |

## 17. Explicitly deferred UI

Not before demonstrated need:

- Ribbon;
- dashboard charts;
- generic Kanban;
- IDE-style docking;
- Dark Mode framework;
- Markdown WebView preview;
- external UI suite;
- product pipeline;
- arbitrary user-customisable layout;
- plugin-hosted controls.

## 18. Definition of Done — UI P1

- MainForm and ProjectEditor support the specified flow;
- no persistence code in Forms/UserControls;
- keyboard flow tested;
- validation tested;
- 100/125-or-150/200% DPI checked;
- required information not clipped;
- safe error handling demonstrated;
- concept image remains labelled as a concept;
- no UI dependency introduced without recorded review.
