# Codex project instructions for SASD PIMS

This package contains only repository-relevant Codex guidance. It does **not** contain package manifests, file inventories or transport-only metadata intended for Git.

## Files

```text
AGENTS.md
docs/codex/
├── README.md
└── FIRST-TASK-0.0.1.md
```

## Apply

Copy the ZIP contents into the root of the `SASD-PIMS` repository.

Then review:

```bash
git status -sb
git diff --check
git diff -- AGENTS.md docs/codex/
```

Suggested commit:

```text
chore(agent): add Codex repository instructions
```

Do not change the existing project baselines while applying this package.

## Maintenance

`AGENTS.md` contains the complete currently planned milestone sequence from `0.0.1-internal` through `1.0.0`.

Only the **Current active milestone** section should normally change when a release stage is accepted. The roadmap summary should change only when the underlying roadmap is formally revised.

Codex tasks should remain small. Prefer one reviewed task/issue at a time and let Codex build/test/debug inside the repository before a commit is requested.
