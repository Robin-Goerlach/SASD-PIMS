# `0.0.1-internal` — Vertical Slice Implementation Contract

**Purpose:** Prove the accepted architecture with the smallest complete workflow.

## User-visible story

A user can:

1. start SASD PIMS;
2. create a minimal Project;
3. receive consistent validation feedback;
4. save the Project;
5. close SASD PIMS;
6. start it again;
7. find/load the same Project;
8. export the Project as versioned JSON;
9. create a verifiable backup;
10. restore a valid backup through staging;
11. see a corrupted/tampered backup rejected without damage to active data.

## Minimum production projects

Start with four only when each has a real role:

```text
src/
├── Sasd.Pims.Domain/
├── Sasd.Pims.Application/
├── Sasd.Pims.Infrastructure/
└── Sasd.Pims.WinForms/
```

Tests may begin with:

```text
tests/
├── Sasd.Pims.Domain.Tests/
├── Sasd.Pims.Application.Tests/
├── Sasd.Pims.IntegrationTests/
└── Sasd.Pims.Architecture.Tests/
```

If a test-project split has no practical value during scaffolding, keep tests smaller and split only when the runtime/dependency boundary benefits.

## Dependency rule

```text
WinForms ─────► Application ─────► Domain
    │                              ▲
    └────────► Infrastructure ─────┘
                  │
                  └────► Application ports
```

Forbidden:

- Domain → EF Core/WinForms/Infrastructure
- Application → WinForms
- Form event → SQL/DbContext
- hidden global service locator

## Feature scope

### Included

- Project `Id`, stable `Key`, `Name`, optional `ShortDescription`;
- create/load;
- minimal edit if needed to prove revision/concurrency;
- real SQLite migration/persistence;
- simple project list/open;
- structured error path;
- minimal JSON export;
- backup/restore PoC;
- tests/evidence;
- ZIP publish comparison.

### Explicitly excluded

- Requirement, Product, Blocker, Milestone, Decision, Risk, Release;
- tags/reference administration;
- dashboard;
- charts;
- ribbon;
- Markdown preview;
- GitHub API;
- cloud;
- installer;
- automatic updater;
- generic plugin architecture.

## Technical sequence

1. scaffold solution;
2. centralise SDK/package/build configuration;
3. add architecture tests;
4. implement minimal Project domain;
5. implement create/load use cases;
6. implement real SQLite persistence + migration;
7. prove integration round trip;
8. add minimal WinForms shell/editor;
9. add logging/error correlation;
10. add versioned JSON export;
11. add verified backup/staged restore;
12. add fault-injection tests;
13. add Windows CI quality gate;
14. compare publish models and create internal package;
15. produce evidence/PoC report.

## Definition of Done

- [ ] clean checkout builds;
- [ ] all automated P1 tests pass;
- [ ] dependency gates pass;
- [ ] real SQLite round trip passes;
- [ ] duplicate key is rejected;
- [ ] migration from empty database works;
- [ ] valid backup/restore works;
- [ ] tampered backup is rejected before active replacement;
- [ ] rollback path tested;
- [ ] core flow works by keyboard;
- [ ] DPI smoke checks documented;
- [ ] no real user data/secrets in repository or artifacts;
- [ ] publish comparison recorded;
- [ ] artifact checksum generated;
- [ ] architecture deviations and technical debt recorded;
- [ ] decision made: reuse/refactor/discard PoC code for `0.1.0`.

## Stop rule

Do not add a new domain area merely because the architecture already describes it.

If the vertical slice exposes a problem, fix or consciously revise the architecture before scaling the feature set.
