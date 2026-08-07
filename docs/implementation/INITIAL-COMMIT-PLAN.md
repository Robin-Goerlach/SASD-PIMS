# Initial Commit Plan

Recommended sequence after the Ready-to-Code baseline:

1. `chore(solution): scaffold .NET 10 solution structure`
2. `build(repo): centralize SDK and package configuration`
3. `test(architecture): enforce project dependency rules`
4. `feat(projects): add minimal project domain model`
5. `feat(projects): add create and load project use cases`
6. `feat(persistence): add SQLite project persistence`
7. `test(persistence): verify SQLite project round trip`
8. `feat(ui): add minimal project editor`
9. `feat(diagnostics): add structured error handling and logging`
10. `feat(export): add versioned project JSON export`
11. `feat(recovery): add verified backup and staged restore`
12. `test(recovery): cover backup and restore failure paths`
13. `ci(quality): add Windows build and test workflow`
14. `build(release): evaluate and package vertical slice`

Principles:

- keep one conceptual change per commit where practical;
- do not mix mass formatting with behaviour;
- do not create future domain tables/controls "for completeness";
- every schema change ships with migration and integration evidence;
- every recovery change includes a failure-path test.
