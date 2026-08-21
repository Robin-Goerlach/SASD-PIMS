using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Sasd.Pims.Application.Search;
using Sasd.Pims.Domain.Projects;
using Sasd.Pims.Domain.Requirements;
using Sasd.Pims.Infrastructure.Persistence;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using Sasd.Pims.Application.Diagnostics;
using Sasd.Pims.Application.Projects;

namespace Sasd.Pims.IntegrationTests;

public sealed class SearchPerformanceBaselineTests(ITestOutputHelper output)
{
    public const int ProjectCount = 500;
    public const int RequirementCount = 10_000;
    public const int ReferenceCount = 20_000;
    public const int BlockerCount = 2_000;
    public const int ChangeEventCount = 10_000;

    [Fact]
    public async Task TypicalSearchOperationsMeetDocumentedP95Target()
    {
        var root = Path.Combine(Path.GetTempPath(), "SASD-PIMS", "benchmark", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var factory = new PimsDbContextFactory(Path.Combine(root, "benchmark.db"), pooling: false);
            await new DatabaseMigrator(factory).MigrateAsync(cancellationToken: TestContext.Current.CancellationToken);
            var seedWatch = Stopwatch.StartNew();
            await SeedAsync(factory, TestContext.Current.CancellationToken);
            seedWatch.Stop();
            var reader = new SqliteSearchReader(factory);
            var projectRepository = new SqliteProjectRepository(factory);
            var blockerRepository = new SqliteProjectBlockerRepository(factory);
            var projectList = new ListProjects(projectRepository, blockerRepository, TimeProvider.System,
                new OperationFailureHandler(NullLogger<OperationFailureHandler>.Instance));
            var queries = new[]
            {
                new SearchQuery("needle", Limit: 200),
                new SearchQuery("Requirement 009999", ObjectType: SearchObjectType.Requirement),
                new SearchQuery("github.example", ObjectType: SearchObjectType.ExternalReference),
                new SearchQuery("Blocker 01999", ObjectType: SearchObjectType.Blocker),
                new SearchQuery(null, ObjectType: SearchObjectType.Requirement, RequirementPriority: RequirementPriority.Must),
            };
            await reader.SearchAsync(queries[0], TestContext.Current.CancellationToken); // warm-up
            var timings = new List<(string Operation, double Milliseconds)>();
            foreach (var query in Enumerable.Range(0, 4).SelectMany(_ => queries))
            {
                var watch = Stopwatch.StartNew();
                _ = await reader.SearchAsync(query, TestContext.Current.CancellationToken);
                watch.Stop(); timings.Add(("GlobalSearch", watch.Elapsed.TotalMilliseconds));
            }
            foreach (var _ in Enumerable.Range(0, 5))
            {
                await MeasureAsync("ProjectList", () => projectList.ExecuteAsync(TestContext.Current.CancellationToken));
                await MeasureAsync("CombinedProjectFilter", () => projectList.ExecuteAsync(new ProjectCatalogFilter(
                    SearchText: "Project", Phase: ProjectPhase.Execution, ActivityState: ActivityState.Active),
                    TestContext.Current.CancellationToken));
                await MeasureAsync("ProjectContextLoad", () => projectRepository.GetByIdAsync(Id(1, 250),
                    TestContext.Current.CancellationToken));
            }
            var ordered = timings.Select(item => item.Milliseconds).Order().ToArray();
            var p95 = ordered[(int)Math.Ceiling(ordered.Length * 0.95) - 1];
            Report($"Environment={Environment.OSVersion}; CPU={Environment.ProcessorCount}; Framework={Environment.Version}");
            Report($"Dataset projects={ProjectCount}, requirements={RequirementCount}, references={ReferenceCount}, blockers={BlockerCount}, changeEvents={ChangeEventCount}");
            Report($"SeedMs={seedWatch.Elapsed.TotalMilliseconds:F1}; MinMs={ordered[0]:F1}; MedianMs={ordered[ordered.Length / 2]:F1}; P95Ms={p95:F1}; MaxMs={ordered[^1]:F1}");
            foreach (var group in timings.GroupBy(item => item.Operation))
            {
                var values = group.Select(item => item.Milliseconds).Order().ToArray();
                Report($"Operation={group.Key}; MedianMs={values[values.Length / 2]:F1}; P95Ms={values[(int)Math.Ceiling(values.Length * .95) - 1]:F1}; MaxMs={values[^1]:F1}");
            }
            Assert.True(p95 < 2000, $"Measured P95 {p95:F1} ms exceeds the 2,000 ms target.");

            async Task MeasureAsync<T>(string operation, Func<Task<T>> action)
            {
                var watch = Stopwatch.StartNew(); await action(); watch.Stop();
                timings.Add((operation, watch.Elapsed.TotalMilliseconds));
            }
            void Report(string value) { output.WriteLine(value); Console.WriteLine($"PERF: {value}"); }
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static async Task SeedAsync(PimsDbContextFactory factory, CancellationToken cancellationToken)
    {
        await using var context = factory.CreateDbContext();
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        var now = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);
        var projects = Enumerable.Range(1, ProjectCount).Select(index => new ProjectRecord
        {
            Id = Id(1, index), Key = $"PRJ-{index:0000}", Name = $"Project {index:0000}",
            ShortDescription = index % 25 == 0 ? "needle synthetic project" : "Synthetic benchmark project",
            Goal = "Search performance", Phase = (ProjectPhase)(index % 5),
            ActivityState = (ActivityState)(index % 5), CreatedAtUtc = now, ModifiedAtUtc = now, Revision = 1,
        }).ToArray();
        context.Projects.AddRange(projects);
        context.Requirements.AddRange(Enumerable.Range(1, RequirementCount).Select(index => new RequirementRecord
        {
            Id = Id(2, index), ProjectId = projects[(index - 1) % ProjectCount].Id, Key = $"REQ-{index:000000}",
            Title = index % 200 == 0 ? $"needle Requirement {index:000000}" : $"Requirement {index:000000}",
            Description = "Synthetic searchable description", Priority = (RequirementPriority)(index % 3),
            DecisionStatus = (RequirementDecisionStatus)(index % 4),
            SourceType = (RequirementSourceType)(index % 7), Revision = 1,
        }));
        context.ExternalReferences.AddRange(Enumerable.Range(1, ReferenceCount).Select(index => new ExternalReferenceRecord
        {
            Id = Id(3, index), ProjectId = projects[(index - 1) % ProjectCount].Id,
            RequirementId = Id(2, ((index - 1) % RequirementCount) + 1), Type = ExternalReferenceType.WebUrl,
            Title = index % 400 == 0 ? $"needle Reference {index:000000}" : $"Reference {index:000000}",
            Target = $"https://github.example/repository/{index:000000}", Revision = 1,
        }));
        context.ProjectBlockers.AddRange(Enumerable.Range(1, BlockerCount).Select(index => new ProjectBlockerRecord
        {
            Id = Id(4, index), ProjectId = projects[(index - 1) % ProjectCount].Id,
            Summary = index % 80 == 0 ? $"needle Blocker {index:00000}" : $"Blocker {index:00000}",
            CreatedAtUtc = now.AddMinutes(index),
        }));
        context.ChangeEvents.AddRange(Enumerable.Range(1, ChangeEventCount).Select(index => new ChangeEventRecord
        {
            Id = Id(5, index), ProjectId = projects[(index - 1) % ProjectCount].Id, EntityType = "Project",
            EntityId = projects[(index - 1) % ProjectCount].Id, EventType = "Phase",
            OccurredAtUtc = now.AddSeconds(index), OldValue = "Idea", NewValue = "Preparation",
        }));
        await context.SaveChangesAsync(cancellationToken);
    }

    private static Guid Id(byte category, int index)
    {
        Span<byte> bytes = stackalloc byte[16];
        bytes[0] = category; BitConverter.TryWriteBytes(bytes[4..], index); return new Guid(bytes);
    }
}
