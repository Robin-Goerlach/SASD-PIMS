using Sasd.Pims.Application.Projects;
using Sasd.Pims.Application.Requirements;
using Sasd.Pims.Domain.Projects;
using Sasd.Pims.Domain.Requirements;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Sasd.Pims.Application.Diagnostics;
using Xunit;

namespace Sasd.Pims.Application.Tests;

public sealed class ProjectUseCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateValidProjectStoresExactlyOneAggregate()
    {
        var repository = new ProjectRepositoryFake();
        var useCase = CreateUseCase(repository);

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
        var useCase = CreateUseCase(repository);

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
        var useCase = CreateUseCase(repository);

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
        var useCase = new LoadProject(repository, FailureHandler());

        var result = await useCase.ExecuteAsync(project.Id, TestContext.Current.CancellationToken);

        Assert.Equal(ProjectOperationStatus.Success, result.Status);
        Assert.Equal(project.Id, result.Value?.Id);
        Assert.Equal("DEMO", result.Value?.Key);
    }

    [Fact]
    public async Task LoadUnknownProjectReturnsNotFound()
    {
        var useCase = new LoadProject(new ProjectRepositoryFake(), FailureHandler());

        var result = await useCase.ExecuteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        Assert.Equal(ProjectOperationStatus.NotFound, result.Status);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task UpdatePreservesKeyAndReturnsCompleteMasterData()
    {
        var project = Project.Create(Guid.NewGuid(), "DEMO", "Demo", null, Now.AddMinutes(-1));
        var repository = new ProjectRepositoryFake { ProjectToLoad = project };
        var useCase = new UpdateProject(repository, new FixedTimeProvider(Now), FailureHandler());

        var result = await useCase.ExecuteAsync(new(project.Id, 1, "Renamed", "Short", "Goal",
            "Benefit", "software", "internal", "Team", ["local"]), TestContext.Current.CancellationToken);

        Assert.Equal(ProjectOperationStatus.Success, result.Status);
        Assert.Equal("DEMO", result.Value?.Key);
        Assert.Equal("SOFTWARE", result.Value?.ProjectType);
        Assert.Equal(2, result.Value?.Revision);
        Assert.Single(repository.UpdatedProjects);
    }

    [Fact]
    public async Task StaleUpdateReturnsConflictWithoutWriting()
    {
        var project = Project.Reconstitute(Guid.NewGuid(), "DEMO", "Demo", null,
            Now.AddMinutes(-2), Now.AddMinutes(-1), 2);
        var repository = new ProjectRepositoryFake { ProjectToLoad = project };
        var useCase = new UpdateProject(repository, new FixedTimeProvider(Now), FailureHandler());

        var result = await useCase.ExecuteAsync(new(project.Id, 1, "Stale", null, null, null,
            null, null, null, []), TestContext.Current.CancellationToken);

        Assert.Equal(ProjectOperationStatus.Conflict, result.Status);
        Assert.Empty(repository.UpdatedProjects);
    }

    [Fact]
    public async Task ArchiveAndReactivateRemainDiscoverable()
    {
        var project = Project.Create(Guid.NewGuid(), "DEMO", "Demo", null, Now.AddMinutes(-1));
        var repository = new ProjectRepositoryFake { ProjectToLoad = project };
        var useCase = new SetProjectArchiveState(repository, new FixedTimeProvider(Now), FailureHandler());

        var archived = await useCase.ExecuteAsync(project.Id, 1, true, TestContext.Current.CancellationToken);
        var hidden = await new ListProjects(repository, FailureHandler()).ExecuteAsync(TestContext.Current.CancellationToken);
        var visible = await new ListProjects(repository, FailureHandler()).ExecuteAsync(
            new(IncludeArchived: true), TestContext.Current.CancellationToken);

        Assert.True(archived.Value?.IsArchived);
        Assert.Empty(hidden.Value!);
        Assert.Single(visible.Value!);
    }

    [Fact]
    public async Task InfrastructureFailureHasCorrelatedSafeDiagnostic()
    {
        const string projectText = "Sensitive synthetic project text";
        var repository = new ProjectRepositoryFake { Failure = new IOException("Synthetic I/O failure") };
        var logger = new CollectingLogger();
        var useCase = new CreateProject(
            repository,
            new FixedTimeProvider(Now),
            new OperationFailureHandler(logger));

        var result = await useCase.ExecuteAsync(
            new("FAILURE", "Failure", projectText),
            TestContext.Current.CancellationToken);

        Assert.Equal(ProjectOperationStatus.InfrastructureFailure, result.Status);
        Assert.NotNull(result.ErrorId);
        Assert.Contains(result.ErrorId, logger.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(projectText, logger.Message, StringComparison.Ordinal);
        Assert.IsType<IOException>(logger.Exception);
    }

    [Fact]
    public async Task SteeringUpdateKeepsArchiveStateIndependentAndAdvancesRevision()
    {
        var project = Project.Create(Guid.NewGuid(), "DEMO", "Demo", null, Now.AddMinutes(-1));
        project.Archive(Now.AddSeconds(-1));
        var repository = new ProjectRepositoryFake { ProjectToLoad = project };
        var useCase = new UpdateProjectSteering(repository, new FixedTimeProvider(Now), FailureHandler());

        var result = await useCase.ExecuteAsync(new(project.Id, 2, ProjectPhase.Execution,
            ActivityState.Active, new DateOnly(2026, 9, 30), null), TestContext.Current.CancellationToken);

        Assert.Equal(ProjectOperationStatus.Success, result.Status);
        Assert.True(result.Value?.IsArchived);
        Assert.Equal(ProjectPhase.Execution, result.Value?.Phase);
        Assert.Equal(ActivityState.Active, result.Value?.ActivityState);
        Assert.Equal(3, result.Value?.Revision);
    }

    [Fact]
    public async Task CatalogDerivesEveryAttentionReasonFromCurrentFacts()
    {
        var project = Project.Reconstitute(Guid.NewGuid(), "DEMO", "Demo", null, null, null, null, null,
            null, [], ProjectPhase.Execution, ActivityState.Active, new DateOnly(2026, 8, 18),
            Now.AddDays(-30), Now.AddDays(-1), false, Now.AddDays(-60), Now.AddDays(-2), 3);
        var projects = new ProjectRepositoryFake { ProjectToLoad = project };
        var blockers = new BlockerRepositoryFake { OpenProjectIds = [project.Id] };

        var result = await new ListProjects(projects, blockers, new FixedTimeProvider(Now), FailureHandler())
            .ExecuteAsync(new(NeedsAttentionOnly: true), TestContext.Current.CancellationToken);

        var row = Assert.Single(result.Value!);
        Assert.Equal([AttentionReason.ReviewOverdue, AttentionReason.OpenBlocker,
            AttentionReason.TargetDateOverdue], row.AttentionReasons);
    }

    [Fact]
    public async Task BlockerUseCasesRetainResolutionHistory()
    {
        var project = Project.Create(Guid.NewGuid(), "DEMO", "Demo", null, Now.AddMinutes(-1));
        var projects = new ProjectRepositoryFake { ProjectToLoad = project };
        var blockers = new BlockerRepositoryFake();
        var references = new ReferenceRepositoryFake();
        var add = new AddProjectBlocker(projects, blockers, references, new FixedTimeProvider(Now), FailureHandler());

        var added = await add.ExecuteAsync(project.Id, "Decision missing", "Escalated", null,
            "Delivery delayed", null, "Escalate decision", null,
            TestContext.Current.CancellationToken);
        var resolved = await new ResolveProjectBlocker(blockers, new FixedTimeProvider(Now.AddHours(1)), FailureHandler())
            .ExecuteAsync(added.Value!.Id, "Decision recorded", TestContext.Current.CancellationToken);

        Assert.Equal(ProjectOperationStatus.Success, resolved.Status);
        Assert.False(resolved.Value!.IsOpen);
        Assert.Equal("Decision recorded", resolved.Value.ResolutionNote);
        Assert.Single((await blockers.ListByProjectAsync(project.Id, TestContext.Current.CancellationToken)));
    }

    [Fact]
    public async Task BlockerRejectsExternalTaskReferenceFromAnotherProject()
    {
        var project = Project.Create(Guid.NewGuid(), "DEMO", "Demo", null, Now.AddMinutes(-1));
        var externalTask = ExternalReference.Create(Guid.NewGuid(), Guid.NewGuid(), null,
            ExternalReferenceType.ExternalTask, "Task", "https://example.test/tasks/1");
        var useCase = new AddProjectBlocker(new ProjectRepositoryFake { ProjectToLoad = project },
            new BlockerRepositoryFake(), new ReferenceRepositoryFake { Reference = externalTask },
            new FixedTimeProvider(Now), FailureHandler());

        var result = await useCase.ExecuteAsync(project.Id, "Blocked", null, null, "Impact", null,
            "Next", externalTask.Id, TestContext.Current.CancellationToken);

        Assert.Equal(ProjectOperationStatus.ValidationFailed, result.Status);
        Assert.Contains(result.Errors, error => error.Code == "BlockerExternalTaskReferenceInvalid");
    }

    private sealed class ProjectRepositoryFake : IProjectRepository
    {
        public List<Project> AddedProjects { get; } = [];

        public List<Project> UpdatedProjects { get; } = [];

        public ProjectWriteResult AddResult { get; init; } = ProjectWriteResult.Saved;

        public Project? ProjectToLoad { get; init; }

        public Exception? Failure { get; init; }

        public Task<ProjectWriteResult> AddAsync(Project project, CancellationToken cancellationToken)
        {
            if (Failure is not null)
            {
                return Task.FromException<ProjectWriteResult>(Failure);
            }

            AddedProjects.Add(project);
            return Task.FromResult(AddResult);
        }

        public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            if (Failure is not null)
            {
                return Task.FromException<Project?>(Failure);
            }

            return Task.FromResult(ProjectToLoad?.Id == id ? ProjectToLoad : null);
        }

        public Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken)
        {
            if (Failure is not null)
            {
                return Task.FromException<IReadOnlyList<Project>>(Failure);
            }

            return Task.FromResult<IReadOnlyList<Project>>(ProjectToLoad is null ? [] : [ProjectToLoad]);
        }

        public Task<ProjectWriteResult> UpdateAsync(
            Project project,
            int expectedRevision,
            CancellationToken cancellationToken)
        {
            if (Failure is not null) return Task.FromException<ProjectWriteResult>(Failure);
            UpdatedProjects.Add(project);
            return Task.FromResult(ProjectWriteResult.Saved);
        }
    }

    private sealed class BlockerRepositoryFake : IProjectBlockerRepository
    {
        private readonly List<ProjectBlocker> blockers = [];
        public HashSet<Guid> OpenProjectIds { get; init; } = [];
        public Task AddAsync(ProjectBlocker blocker, CancellationToken cancellationToken)
        {
            blockers.Add(blocker);
            return Task.CompletedTask;
        }
        public Task<ProjectBlocker?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(blockers.SingleOrDefault(blocker => blocker.Id == id));
        public Task<IReadOnlyList<ProjectBlocker>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ProjectBlocker>>(blockers.Where(blocker => blocker.ProjectId == projectId).ToArray());
        public Task<IReadOnlySet<Guid>> GetProjectIdsWithOpenBlockersAsync(CancellationToken cancellationToken) =>
            Task.FromResult(OpenProjectIds.Count > 0 ? OpenProjectIds :
                (IReadOnlySet<Guid>)blockers.Where(blocker => blocker.IsOpen).Select(blocker => blocker.ProjectId).ToHashSet());
        public Task<bool> ResolveAsync(ProjectBlocker blocker, CancellationToken cancellationToken) => Task.FromResult(true);
    }

    private sealed class ReferenceRepositoryFake : IExternalReferenceRepository
    {
        public ExternalReference? Reference { get; init; }
        public Task<RequirementWriteResult> AddAsync(ExternalReference reference, CancellationToken cancellationToken) =>
            Task.FromResult(RequirementWriteResult.Saved);
        public Task<RequirementWriteResult> UpdateAsync(ExternalReference reference, int expectedRevision,
            CancellationToken cancellationToken) => Task.FromResult(RequirementWriteResult.Saved);
        public Task<ExternalReference?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Reference?.Id == id ? Reference : null);
        public Task<IReadOnlyList<ExternalReference>> ListByProjectAsync(Guid projectId,
            CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<ExternalReference>>(
                Reference?.ProjectId == projectId ? [Reference] : []);
    }

    private static CreateProject CreateUseCase(IProjectRepository repository) =>
        new(repository, new FixedTimeProvider(Now), FailureHandler());

    private static OperationFailureHandler FailureHandler() =>
        new(NullLogger<OperationFailureHandler>.Instance);

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class CollectingLogger : ILogger<OperationFailureHandler>
    {
        public string Message { get; private set; } = string.Empty;

        public Exception? Exception { get; private set; }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Message = formatter(state, exception);
            Exception = exception;
        }
    }
}
