# Third-party notices — 0.0.1-internal

This inventory records direct runtime and test dependencies. Transitive packages inherit from
these dependency families and are available in the NuGet restore graph produced by
`dotnet package list --project Sasd.Pims.slnx --include-transitive`.

| Package / tool | Version | Use | Licence | Source / replacement path |
| --- | --- | --- | --- | --- |
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.10 | SQLite ORM/provider | MIT | Microsoft/NuGet; replace only through an approved persistence decision |
| Microsoft.Extensions.Logging | 10.0.10 | UI composition logging abstractions | MIT | Microsoft/NuGet; .NET logging abstraction |
| Microsoft.Extensions.Logging.Abstractions | 10.0.10 | Application logging contract | MIT | Microsoft/NuGet; .NET logging abstraction |
| SQLitePCLRaw.lib.e_sqlite3 | 2.1.12 | Patched native SQLite binary | Apache-2.0 | SQLitePCLRaw/NuGet; pinned over vulnerable transitive 2.1.11 native binary |
| xunit.v3 | 3.2.2 | Automated tests only | Apache-2.0 | xUnit/NuGet; test-only dependency |
| actions/checkout | v7 | CI source checkout only | MIT | GitHub Actions; build-only |
| actions/setup-dotnet | v6 | CI SDK setup only | MIT | GitHub Actions; build-only |

The application itself is licensed under Apache License 2.0; see `LICENSE`.
