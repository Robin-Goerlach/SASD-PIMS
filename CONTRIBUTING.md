# Contributing to SASD PIMS

Thank you for considering a contribution.

SASD PIMS is intentionally being developed as a small, maintainable Windows desktop application. Contributions should improve the product without turning it into a generic project-management platform.

## Before you start

For anything larger than a small documentation or defect fix:

1. search existing issues;
2. open or comment on an issue describing the problem and intended outcome;
3. identify the affected roadmap milestone;
4. call out any architecture, database-schema, file-format or dependency change;
5. wait for scope clarification if the change affects a documented system boundary.

## Development principles

Contributions should preserve these constraints unless an explicit decision changes them:

- Windows Forms is the UI technology.
- The application is local-first/offline-first.
- The domain must remain independent of UI and persistence frameworks.
- SQLite is the authoritative local persistence technology.
- External providers are optional adapters, not prerequisites for core operation.
- New dependencies require a demonstrated need, licence review and maintenance assessment.
- PIMS is not a task manager or Jira replacement.

## Branches

Use short-lived branches from `main`:

- `feat/<issue>-<short-name>`
- `fix/<issue>-<short-name>`
- `docs/<issue>-<short-name>`
- `refactor/<issue>-<short-name>`
- `test/<issue>-<short-name>`
- `chore/<issue>-<short-name>`

Do not create a permanent `develop` branch.

## Commit messages

Use Conventional Commit style:

```text
feat(projects): add project creation use case
fix(persistence): prevent duplicate project codes
docs(readme): clarify pre-release installation status
test(backup): cover restore checksum failure
refactor(domain): isolate project status transition rule
build(deps): update EF Core
```

Use an issue reference in the body or footer where useful:

```text
Refs #42
```

Breaking architectural or data-format changes must be explicit.

## Pull requests

A pull request should:

- explain the user or engineering problem;
- identify the affected issue and milestone;
- describe important trade-offs;
- include tests appropriate to the change;
- update documentation when behaviour or architecture changes;
- contain no real user data, secrets or personal diagnostic files;
- keep the change as small as practical.

For a one-maintainer repository, pull requests are still valuable for architecture-affecting, schema-changing and release-critical changes because they create a review boundary. Trivial documentation corrections may be committed directly during the early bootstrap phase.

## Testing

At minimum, run the repository's build and test commands once they exist.

Changes touching persistence, migration, import/export, backup/restore or external-reference handling require integration or failure-path tests, not only unit tests.

Changes touching WinForms layouts should be checked for:

- keyboard navigation;
- focus order;
- meaningful labels/accessibility names;
- 100%, 125%, 150% and 200% DPI where relevant;
- no clipped text;
- operation without relying on colour alone.

## Dependencies

Do not add a NuGet package just because it saves a small amount of code.

A dependency proposal should record:

- concrete missing capability;
- alternative using framework controls;
- licence;
- current maintenance/release activity;
- supported .NET version;
- transitive dependencies;
- security/supply-chain implications;
- lock-in and replacement cost.

## Documentation

Architecture decisions should be recorded as ADRs when they affect:

- project/module boundaries;
- persistence/schema strategy;
- file or exchange formats;
- security/trust boundaries;
- major dependencies;
- deployment/update model;
- externally visible compatibility.

## Security

Do not publish exploitable vulnerabilities in a normal issue. Follow `SECURITY.md`.

## Licence

Contribution terms depend on the repository's final licence decision. Do not accept external code contributions before the licence/distribution model has been explicitly approved.
