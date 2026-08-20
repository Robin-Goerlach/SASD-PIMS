# Implementation evidence — 0.0.1-internal

**Evidence date:** 2026-08-20
**Implementation commit:** `e45fd20e087c753fef67bb5eb15d4bd6d111c6f9`
**Branch:** `codex/0.0.1-internal`
**Status:** implementation gates passed; maintainer acceptance and milestone transition pending

## Environment

| Item | Evidence |
| --- | --- |
| OS | Windows 11 x64, build 26200 |
| .NET SDK | 10.0.303 (`global.json` baseline 10.0.300, `latestFeature` roll-forward) |
| Runtime target | .NET 10, WinForms, `win-x64` |
| Database | SQLite through EF Core 10.0.10 |
| Schema | `202608190001_InitialProject` |
| Package version | `0.0.1-internal`; file version `0.0.1.0` |

## Automated evidence

The release script ran from commit `e45fd20` and completed:

```powershell
dotnet restore Sasd.Pims.slnx
dotnet build Sasd.Pims.slnx -c Release --no-restore
dotnet test Sasd.Pims.slnx -c Release --no-build
dotnet publish src/Sasd.Pims.WinForms/Sasd.Pims.WinForms.csproj -c Release -r win-x64 --self-contained false
dotnet publish src/Sasd.Pims.WinForms/Sasd.Pims.WinForms.csproj -c Release -r win-x64 --self-contained true
```

Result: Release build succeeded with zero warnings and zero errors; 48 tests passed, zero failed,
zero skipped.

The suite proves:

- Domain and Application validation/use-case behaviour;
- architecture dependency direction and absence of project-reference cycles;
- real SQLite migration, uniqueness and complete context reconstruction/round trip;
- versioned JSON export semantics;
- backup manifest, SHA-256 and consistent reopenable snapshot;
- staged restore, atomic promotion, rollback package and smoke read;
- rejection of checksum mismatch, missing payload, incompatible format/schema and corrupt SQLite;
- injected replacement failure leaves the previous database readable and rollback evidence present;
- keyboard order/mnemonics, accessible names, text validation and contained WinForms layout at
  simulated 100%, 125%, 150% and 200% scale factors.

## Quality-gate matrix

| Gate | Result | Evidence |
| --- | --- | --- |
| QG-P1-01 Build | Pass | release build, zero warnings/errors |
| QG-P1-02 Unit | Pass | Domain/Application tests in 48-test suite |
| QG-P1-03 Architecture | Pass | automated project/package/source dependency tests |
| QG-P1-04 SQLite | Pass | real-file migration, duplicate key and round-trip tests |
| QG-P1-05 Recovery | Pass | positive restore plus six specified negative/rollback paths |
| QG-P1-06 UI baseline | Pass with noted manual limit | automated keyboard/A11y/scale tests; startup logged at 96 DPI |
| QG-P1-07 Package | Pass | reproducible script, clean ZIP and verified SHA-256 |
| QG-P1-08 Dependency review | Pass | `THIRD-PARTY-NOTICES.md`; vulnerable-package check clean after native SQLite pin |
| QG-P1-09 Documentation | Pass | release notes, changelog, baseline decision and this evidence record |

## Publish comparison and artifact

| Model | Files | Uncompressed size | Runtime prerequisite | Result |
| --- | ---: | ---: | --- | --- |
| Framework-dependent | 30 | 8,603,764 bytes (8.21 MiB) | .NET 10 Desktop Runtime x64 | retained as evaluation output |
| Self-contained | 296 | 131,180,173 bytes (125.10 MiB) | none beyond supported Windows 11 x64 | selected default for 0.1.0 |

Generated local artifact (ignored by Git):

- `artifacts/release/0.0.1-internal/SASD-PIMS-0.0.1-internal-win-x64.zip`
- ZIP size: 53,135,139 bytes
- SHA-256: `F178E96FF8ABC61800796FD2F1AD07031E194C69D863255AE6AFA02F1F14DFA1`
- checksum was generated and independently re-read by the release script;
- package scan found no database, log, backup, `.env`, token or secret file.

## Architecture and implementation findings

- The approved four-project modular-monolith split supports the complete P1 path without an
  additional production project.
- Domain remains persistence/UI independent; Application depends only on neutral contracts.
- EF Core contexts are short-lived and created per use case; WinForms forms contain no SQL or
  concrete `DbContext` use.
- SQLite online backup produces the package payload. Restore validates entirely in a same-volume
  staging directory, then uses atomic file replacement and retains an independently verifiable
  rollback ZIP.
- P1 Project keys use the resolved invariant uppercase/hyphen rule recorded in the baseline.
- A small local `ILoggerProvider` was sufficient; no additional logging framework was introduced.
- The self-contained package decision resolves `OD-P1-003` from measured size/runtime evidence.

## Manual observations and limits

- The self-contained executable reached `ApplicationStarted` and wrote the expected data-minimised
  startup log at system DPI 96 (100%). The tool-isolated desktop did not expose a capturable window
  handle, so no screenshot or Narrator claim is made.
- Multi-monitor/per-monitor transitions and a second physical Windows 11 configuration were not
  available in this environment. The P1 layout scale factors are therefore automated structural
  checks, not a substitute for later hands-on usability qualification.
- CI workflow syntax and commands were reviewed locally, but the workflow was not pushed and thus
  no GitHub-hosted run exists, as required by the no-push instruction.
- The package is unsigned and was not tested on a separate clean machine. Code signing and an
  installer are intentionally not P1 blockers.

## Technical debt carried toward 0.1.0

- add hands-on Narrator and cross-monitor DPI smoke evidence on representative Windows 11 systems;
- decide and implement Project edit/concurrency UX as part of the approved 0.1.0 catalog scope;
- add public release/signing or installer work only after the later packaging decision warrants it;
- evaluate dependency lock files/SBOM generation before a supported external release;
- investigate the local environment's occasional excessive parallel MSBuild process creation;
  the release script currently constrains build/publish nodes and disables build servers.

## Decision before 0.1.0

The P1 code is retained and incrementally extended rather than discarded: its architecture,
persistence, export and recovery assumptions are covered by automated evidence. Before beginning
`0.1.0`, the maintainer must explicitly accept this milestone and confirm the next milestone in
accordance with `AGENTS.md`. No 0.1.0 feature is included here.
