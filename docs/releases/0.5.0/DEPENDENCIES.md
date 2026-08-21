# Dependency and licence inventory — 0.5.0

Generated release verification uses `dotnet package list --include-transitive --format json` and packages the machine-readable result plus SPDX 2.3 SBOM. No 0.5 runtime dependency was added.

| Family | Versions in release graph | Licence | Purpose/source |
| --- | --- | --- | --- |
| Microsoft.EntityFrameworkCore / Microsoft.Data.Sqlite | 10.0.10 | MIT | SQLite ORM/provider; NuGet/Microsoft |
| Microsoft.Extensions.* | 10.0.10 | MIT | logging, DI/configuration primitives; NuGet/Microsoft |
| SQLitePCLRaw | bundle/core/provider 2.1.11; native lib 2.1.12 | Apache-2.0; SQLite public-domain native library | managed/native SQLite bridge; NuGet/ericsink |
| .NET self-contained Windows runtime | 10.0.11 release environment | MIT and bundled third-party notices | executable/runtime deployment; Microsoft |
| xUnit/Microsoft.Testing.Platform (build/test only) | xUnit 3.2.2, platform 1.9.1 | Apache-2.0 / MIT | automated tests; not a production runtime addition |
| Microsoft.ApplicationInsights (test transitive) | 2.23.0 | MIT | test-platform transitive telemetry assembly |

Exact direct/transitive package IDs and versions are emitted as `dependency-inventory.json`. Licences were reviewed against upstream package metadata and existing third-party notices. Vulnerability scanning is a blocking release command; findings may not be silently waived.
