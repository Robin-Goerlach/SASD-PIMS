# First Codex task — `0.0.1-internal`

Use this as the first development assignment after `AGENTS.md` is committed.

## Target commit

```text
chore(solution): scaffold .NET 10 solution structure
```

## Assignment

Implement **only** the initial .NET 10 solution scaffold required for the current `0.0.1-internal` vertical slice.

Before editing:

1. read `/AGENTS.md`;
2. read the relevant architecture/baseline documents referenced there;
3. run `git status -sb`;
4. inspect the existing repository structure.

Create the approved production project structure only:

```text
src/
├── Sasd.Pims.Domain/
├── Sasd.Pims.Application/
├── Sasd.Pims.Infrastructure/
└── Sasd.Pims.WinForms/
```

Create only test projects that already have a concrete responsibility for this first slice. Do not create empty projects merely to mirror a future architecture.

Requirements:

- target .NET 10;
- Windows Forms only in `Sasd.Pims.WinForms`;
- Domain has no WinForms/EF Core/SQLite/Infrastructure dependency;
- Application has no WinForms dependency;
- no domain features yet;
- no database schema or migration yet;
- no UI feature implementation beyond template/scaffold requirements;
- no speculative NuGet packages;
- no installer/updater;
- no later-milestone features.

Central build/package files may be added only if they are necessary to make this scaffold coherent; otherwise leave them for the immediately following dedicated build-baseline task.

## Verification

Run, as applicable:

```bash
dotnet --info
dotnet restore
dotnet build
dotnet test
git diff --check
git status -sb
```

If WinForms cross-targeting from WSL requires a project setting, use the smallest documented change needed and explain it in the result.

## Result report

Report:

- files/projects created;
- project references created;
- exact build/test commands run;
- pass/fail result;
- warnings;
- anything not verified;
- any deviation from `AGENTS.md`.

Do **not** commit automatically.

Stop after the scaffold has been implemented and verified.
