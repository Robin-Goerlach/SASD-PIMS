# Release Preparation

## Versioning

Use Semantic Versioning as the public version vocabulary.

Before 1.0:

- `0.0.1-internal` — architecture proof, not a supported product;
- `0.1.0` ... `0.7.0` — incremental pre-1.0 capability;
- `0.8.0` — beta / feature-complete target;
- `0.9.0` — release candidate;
- `1.0.0` — stable baseline.

Do not create a separate marketing versioning scheme.

## Initial packaging recommendation

For the first internal and 0.x releases, prefer the simplest reproducible Windows artifact:

- `win-x64` self-contained publish;
- ZIP package first;
- SHA-256 checksum;
- release notes;
- third-party licence inventory;
- SBOM when dependencies/build tooling make this practical.

An installer should be added only after its installation/update/rollback behaviour has been proven necessary and tested. Do not make the first vertical slice depend on choosing an installer framework.

## Release contents

Expected release layout:

```text
SASD-PIMS-v0.1.0-win-x64.zip
SASD-PIMS-v0.1.0-SHA256SUMS.txt
SASD-PIMS-v0.1.0-SBOM.spdx.json        # when enabled
RELEASE-NOTES-v0.1.0.md
THIRD-PARTY-NOTICES.txt
```

Never package:

- developer-local databases;
- logs;
- diagnostic bundles;
- backup files;
- `.env` files;
- tokens;
- user-specific configuration.

## Release gate

A release candidate must satisfy the applicable milestone Definition of Done plus:

- clean build from the release commit;
- automated tests pass;
- database migration tests pass when schema changes exist;
- backup/restore tests pass;
- import/export compatibility tests pass when formats changed;
- security/dependency review performed;
- version and changelog updated;
- README does not claim unimplemented features;
- user/recovery docs match actual behaviour;
- screenshots use synthetic data;
- release artifact smoke-tested on a clean Windows environment;
- artifact hashes generated and verified.

## GitHub release workflow

1. complete milestone;
2. freeze scope;
3. update `CHANGELOG.md`;
4. update version;
5. run quality/recovery/migration tests;
6. build release artifacts;
7. generate checksums and SBOM/third-party notices;
8. smoke-test artifacts;
9. create signed tag if signing is available and operationally maintained;
10. create GitHub Release from the tag;
11. mark pre-1.0 internal/beta/RC builds appropriately;
12. attach artifacts and release notes;
13. verify download/install/run path.

## Code signing

Windows code signing is desirable for broad external distribution because unsigned executables can cause trust/SmartScreen friction.

However, code-signing certificates may introduce cost and operational key-management requirements. With the current zero-budget constraint, do not make code signing a blocker for internal PoC builds. Re-evaluate before public 1.0 distribution.

## Update mechanism

Do not introduce automatic updates in the vertical slice.

A later update mechanism must prove:

- authentic artifact verification;
- application/data-version compatibility;
- pre-migration backup;
- rollback behaviour;
- failure recovery;
- no silent data downgrade.

Until then, explicit manual release installation is safer.
