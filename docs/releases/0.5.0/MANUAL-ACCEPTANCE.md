# Manual MVP acceptance — 0.5.0

All cases below are **PENDING**. Do not change a result to PASS without execution evidence.

## Preconditions and reproducible data

Use the RC ZIP and verify its SHA-256. Use an empty Windows test profile and the documented `%LOCALAPPDATA%\SASD\PIMS` path. Create projects `MVP-A` and `MVP-B`; add steering facts, one Must Requirement with criterion, every reference type, one open and one resolved blocker, then create a backup and both exports. Record date, tester, Windows build, display/monitor, RC hash and evidence filenames.

For each case record: **Actual result:** ___ · **Result:** PENDING · **Date:** ___ · **Tester/environment:** ___ · **Evidence:** ___.

| ID | Steps | Expected result |
| --- | --- | --- |
| M-01 Keyboard core | Complete create/edit/select/archive/reactivate using only keyboard. | Focus is visible; every action and Cancel/Save works without mouse. |
| M-02 Steering | Open steering, change independent phase/activity, review and target date. | German values persist and derived indicators are understandable. |
| M-03 Requirements | Create/edit Must Requirement, source and acceptance criterion. | Validation and reopened facts are correct. |
| M-04 References | Create Project/Requirement references of representative types. | Type, title, target and ownership remain visible. |
| M-05 Search | Use Ctrl+F and combined catalog filters, then reset them. | Expected objects appear within two seconds; context navigation works. |
| M-06 Traceability | Navigate Project tree to Requirement/criterion/reference/blocker. | Existing relationships only; correct workspace opens. |
| M-07 Audit | Change steering/Requirement/reference/blocker facts and open history. | Time, object, identifier, event and redacted values appear; no edit/delete exists. |
| M-08 Export | Choose JSON and Markdown destinations and inspect outputs. | Complete valid JSON and readable profile; no partial file after induced invalid target. |
| M-09 Shell/security | Open approved HTTPS/file/directory targets; try missing target and unsafe scheme. | OS handler opens approved targets; controlled message for missing/unsafe targets. |
| M-10 Backup/restore | Restore verified backup into an empty profile; repeat with corrupt source. | Known state restored; corrupt source rejected and active data untouched. |
| M-11 Paths | Open Help → operating information. | DB, logs, app, recovery and user-selected output semantics are correct/read-only. |
| M-12 DPI/monitor | Inspect at 100/125/150/200%, move between physical monitors, inspect contrast/focus. | No required content is clipped; focus and status do not rely on color alone. |
| M-13 Offline | Disable networking and execute all core flows including local help/export/backup. | No server/account required; external web opening alone is unavailable gracefully. |
| M-14 Endurance | Run `scripts/endurance-0.5.0.ps1` for four hours with periodic negative cases. | No unhandled crash or integrity loss; logs remain data-minimal. |
| M-15 Usability/language | After ≤30-minute introduction, target user performs defined MVP scenarios; inspect all visible text. | ≥80% scenarios without help; no inconsistent meaning/unintended English core UI. |
| M-16 Portable lifecycle | On clean Windows 11 x64: extract, first start, close, remove application directory. | Starts without separate runtime/server; removal steps are sufficient and user data handling is clear. |
| M-17 Compatibility | Repeat smoke on two supported Windows 11 x64 configurations. | Both complete normal visible start and shutdown. |

Optional SHOULD: record a screen-reader spot check; it is not the sole MVP release blocker.
