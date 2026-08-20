# SASD PIMS 0.3.0 — Release Notes

`0.3.0` adds structured project Requirements and manually maintained typed references.

- stable project-local Requirement keys;
- controlled Must/Should/Could priority and independent decision state;
- retained decision reasons for deferred and rejected Requirements;
- source classification and optional source reference;
- ordered acceptance criteria with optional verification references;
- Project and Requirement references with controlled types;
- centralized HTTPS, local-path and GitHub URL validation;
- target-string secret rejection and validation before OS opening;
- local German UI, menu integration, accessibility metadata and F1 glossary;
- additive migration from the released 0.2 database with pre-migration backup.

PIMS does not fetch HTTPS targets, copy documents or chats, authenticate to providers, manage tasks,
check links in the background or persist reachability state. Requirement relationships, search and
portable exchange remain later milestones. JSON project exchange remains `1.1-internal`.
