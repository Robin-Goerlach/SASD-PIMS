using Sasd.Pims.Application.Projects;
using Sasd.Pims.Domain.Projects;
using Xunit;

namespace Sasd.Pims.Application.Tests;

public sealed class ProjectUseCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateValidProjectStoresExactlyOneAggregate()
    {
        var repository = new ProjectRepositoryFake();
        var useCase = new CreateProject(repository, new FixedTimeProvider(Now));

        var result = await useCase.ExecuteAsync(
            new("demo", "Demo project", "Synthetic"),
            TestContext.Current.CancellationToken);

        Assert.Equal(ProjectOperationStatus.Success, result.Status);
        Assert.Equal("DEMO", result.Value?.Key);
        Assert.Equal(Now, result.Value?.CreatedAtUtc);
        Assert.Single(repository.AddedProjects);
    }

    [Fact]
    public async Task InvalidCreateReturnsValidationAndDoesNotWrite()
    {
        var repository = new ProjectRepositoryFake();
        var useCase = new CreateProject(repository, new FixedTimeProvider(Now));

        var result = await useCase.ExecuteAsync(
            new(" ", "Demo project", null),
            TestContext.Current.CancellationToken);

        Assert.Equal(ProjectOperationStatus.ValidationFailed, result.Status);
        Assert.Contains(result.Errors, error => error.Code == "ProjectKeyInvalid");
        Assert.Empty(repository.AddedProjects);
    }

    [Fact]
    public async Task DuplicateKeyBecomesApplicationConflict()
    {
        var repository = new ProjectRepositoryFake { AddResult = ProjectWriteResult.DuplicateKey };
        var useCase = new CreateProject(repository, new FixedTimeProvider(Now));

        var result = await useCase.ExecuteAsync(
            new("DEMO", "Demo project", null),
            TestContext.Current.CancellationToken);

        Assert.Equal(ProjectOperationStatus.Conflict, result.Status);
        Assert.Contains(result.Errors, error => error.Code == "ProjectKeyDuplicate");
    }

    [Fact]
    public async Task LoadExistingProjectReturnsNeutralDto()
    {
        var project = Project.Create(Guid.NewGuid(), "DEMO", "Demo project", null, Now);
        var repository = new ProjectRepositoryFake { ProjectToLoad = project };
        var useCase = new LoadProject(repository);

        var result = await useCase.ExecuteAsync(project.Id, TestContext.Current.CancellationToken);

        Assert.Equal(ProjectOperationStatus.Success, result.Status);
        Assert.Equal(project.Id, result.Value?.Id);
        Assert.Equal("DEMO", result.Value?.Key);
    }

    [Fact]
    public async Task LoadUnknownProjectReturnsNotFound()
    {
        var useCase = new LoadProject(new ProjectRepositoryFake());

        var result = await useCase.ExecuteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        Assert.Equal(ProjectOperationStatus.NotFound, result.Status);
        Assert.Null(result.Value);
    }

    private sealed class ProjectRepositoryFake : IProjectRepository
    {
        public List<Project> AddedProjects { get; } = [];

        public ProjectWriteResult AddResult { get; init; } = ProjectWriteResult.Saved;

        public Project? ProjectToLoad { get; init; }

        public Task<ProjectWriteResult> AddAsync(Project project, CancellationToken cancellationToken)
        {
            AddedProjects.Add(project);
            return Task.FromResult(AddResult);
        }

        public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(ProjectToLoad?.Id == id ? ProjectToLoad : null);
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
