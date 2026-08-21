# Codex milestone contract — 0.5.0

## Goal

Close and qualify every binding MUST-MVP requirement on the released v0.4.0 baseline, producing a technically verified release candidate while leaving honest manual gates pending.

## Binding semantics

Read `docs/implementation/DECISION-0.5.0-FULL-MUST-MVP.md` before changing 0.5.0 code. Earlier 0.2.0–0.4.0 decisions remain binding. Preserve the modular-monolith boundaries, SQLite recovery guarantees, exchange 1.0 contract and local/offline ownership model.

## Required scope

- additive blocker facts and same-Project `ExternalTask` validation;
- real `0.4.0 → 0.5.0` migration with legacy-data preservation;
- Project-scoped read-only ChangeEvent view;
- read-only local operating/data-path view;
- complete versioned MUST traceability and manual acceptance checklist;
- extended reproducible performance qualification;
- MVP operating, recovery, security and portable-use documentation;
- dependency/licence inventory, standards-based SBOM, package and SHA-256;
- full automated regression and release evidence.

## Scope boundary

Import remains Release 1.1 and its damaged-input criterion is not applicable to this MVP. Do not implement any explicit 0.5.0 non-goal or begin 0.6.0.

## Completion boundary

Push a clean `codex/0.5.0` release-candidate branch and verify GitHub Quality. While mandatory manual checks remain pending, do not merge to `main`, create `v0.5.0`, publish a GitHub Release or begin 0.6.0.
