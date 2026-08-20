# Codex milestone contract — 0.3.0

## Goal

Implement structured project Requirements, ordered acceptance criteria and manually maintained typed references with safe validation/opening, without copying external content or introducing task/document management.

## Binding semantics

Read `docs/implementation/DECISION-0.3.0-REQUIREMENTS-AND-REFERENCES.md` before changing 0.3.0 code. Its controlled vocabularies, ownership rules, same-Project constraints, key allocation and security rules are binding.

## Required vertical scope

- list, create, view and edit Requirements in the selected Project;
- immutable project-local Requirement keys;
- priority, decision state/reason and source information;
- persistent ordered acceptance criteria;
- Project and Requirement references, including source and verification references;
- centralized target/secret validation and controlled OS opening;
- native WinForms menu/help/keyboard/accessibility/DPI integration;
- additive `0.2.0 → 0.3.0` EF Core migration;
- real-SQLite migration, reopen, recovery and concurrency evidence;
- reproducible self-contained `win-x64` ZIP and SHA-256.

## Scope boundary

Do not implement Requirement relationships/import, global search, new exchange contracts, attachments, copied documents/chats, tasks, workflow engines, provider APIs/authentication, reachability persistence or background checking. Do not begin 0.4.0.

## Completion boundary

Push a clean `codex/0.3.0` branch and verify GitHub Quality. Do not merge to `main`, create `v0.3.0`, publish a GitHub Release or begin 0.4.0 without explicit approval.
