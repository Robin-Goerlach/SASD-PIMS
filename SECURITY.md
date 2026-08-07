# Security Policy

## Current support status

SASD PIMS is currently in pre-implementation / pre-release development.

No stable production version is supported yet. Security properties described in the architecture are design targets until they have been implemented and verified.

## Reporting a vulnerability

Do **not** open a public GitHub issue for a vulnerability that could expose users, data or release infrastructure.

Preferred process once the repository is public:

1. enable GitHub Private Vulnerability Reporting;
2. use the repository's **Security → Report a vulnerability** function;
3. include affected version/commit, reproduction steps, impact and any suggested mitigation.

Until private vulnerability reporting has been enabled, contact the repository maintainer privately through the maintainer's published GitHub contact mechanism. A dedicated security contact may be added before the first public release.

Do not attach real PIMS databases, backups, diagnostic bundles or personal documents unless explicitly requested through a private channel and sanitised first.

## Security-relevant areas

Particular care is required for:

- SQLite database migration and integrity;
- backup and restore;
- import/export parsing;
- filesystem paths;
- external URL launching;
- optional provider adapters;
- credentials and tokens;
- logging and diagnostic packages;
- update/release artifacts;
- third-party dependencies.

## Disclosure

The project will acknowledge valid reports where appropriate and will publish a security advisory when a released version is affected.

Because the product has no stable release yet, response-time guarantees are not currently offered. A formal supported-versions table and security response objective should be added before version 1.0.
