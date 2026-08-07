# GitHub Repository Preparation

## Recommended repository name

`SASD-PIMS`

Why:

- concise;
- consistent with the product name;
- immediately recognisable as an SASD repository;
- does not over-specify implementation technology;
- leaves room for documentation, tests and future packaging in the same repository.

Alternatives considered:

- `sasd-pims`: conventional lower-case naming, but less consistent with existing SASD-style product naming.
- `SASD-Project-Information-Management-System`: explicit but unnecessarily long.
- `PIMS`: too generic and collision-prone.

## GitHub short description

> Local-first Windows desktop application under development for managing SASD projects, products, requirements, milestones, decisions, risks and related references. Built with .NET 10, Windows Forms and SQLite, with emphasis on traceability, low operational overhead, data integrity and reliable backup/restore.

Character count: 309 / 350.

## Recommended Topics

```text
sasd
pims
project-information-management
requirements-management
traceability
decision-log
risk-management
local-first
offline-first
desktop-app
windows
winforms
dotnet
dotnet-10
csharp
sqlite
ef-core
```

Avoid topics such as `jira-clone`, `task-manager` or `kanban` because they would misrepresent the intended scope.

## Recommended repository settings

Initial recommendation:

| Setting | Recommendation | Reason |
| --- | --- | --- |
| Visibility | Private until licence/publication decision | Prevent accidental source licensing/disclosure |
| Default branch | `main` | Single release-ready trunk |
| Wiki | Off initially | Keep authoritative docs versioned in Git |
| Discussions | Off initially | No current community need |
| Issues | On | Backlog, decisions and defects |
| Projects | Optional | Use only if it helps; PIMS must not depend on GitHub Projects |
| Releases | On when first artifact exists | Versioned distribution |
| Private vulnerability reporting | Enable before public release | Security channel |
| Dependabot alerts | Enable when dependencies exist | Supply-chain visibility |
| Secret scanning | Enable where available | Prevent credential leakage |
| Branch protection | Add when CI is stable | Avoid blocking the bootstrap itself |

## Branch strategy

Use trunk-based development with short-lived branches.

`main` is always intended to be buildable and is the source of release tags.

No permanent `develop` branch.

Recommended branch patterns:

```text
feat/42-project-create
fix/71-restore-validation
docs/18-readme-installation
test/33-sqlite-roundtrip
refactor/55-domain-status
chore/12-repository-bootstrap
```

For a solo maintainer:

- bootstrap commits may go directly to `main`;
- after the first CI pipeline works, architecture-affecting, schema-changing and release-critical changes should use pull requests;
- squash merge feature branches to keep the main history readable;
- delete branches after merge.

## Commit strategy

Use Conventional Commits with optional scope:

```text
feat(projects): add project creation use case
fix(backup): reject invalid manifest checksum
docs(architecture): record import adapter boundary
test(persistence): add project round-trip test
refactor(ui): extract project editor presenter
build(deps): update EF Core
ci(quality): add test and formatting gates
chore(repo): add issue forms
```

Recommended rules:

- one conceptual change per commit where practical;
- no "misc fixes" commits;
- do not mix mass formatting with behavioural changes;
- reference issues in commit or PR text;
- breaking schema/file-format changes must be obvious;
- generated build artifacts are not committed.

## Merge strategy

Preferred: **Squash merge** for normal feature/fix branches.

Use a normal merge commit only when preserving branch history is materially useful. Avoid rebase-merging as the default because it makes the original reviewed change boundary less visible.

## Tags

```text
v0.0.1-internal
v0.1.0
v0.2.0
...
v0.9.0
v1.0.0
```

Use GitHub prerelease status for internal/alpha/beta/RC releases.
