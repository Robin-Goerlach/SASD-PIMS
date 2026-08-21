using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using Sasd.Pims.Application.Diagnostics;
using Sasd.Pims.Application.Projects;
using Sasd.Pims.Domain.Projects;
using Sasd.Pims.Application.Requirements;
using Sasd.Pims.Domain.Requirements;
using Sasd.Pims.Application.Search;
using Sasd.Pims.Application.Traceability;
using Sasd.Pims.Application.Auditing;
using Sasd.Pims.WinForms;
using Xunit;

namespace Sasd.Pims.WinForms.Tests;

public sealed class WinFormsBaselineTests
{
    public static TheoryData<float> DpiScaleFactors => new() { 1.0f, 1.25f, 1.5f, 2.0f };

    [Fact]
    public void EditorProvidesKeyboardOrderAccessibleNamesAndTextValidation()
    {
        RunInSta(() =>
        {
            using var form = CreateEditor();
            form.CreateControl();
            form.PerformLayout();

            var textBoxes = Descendants(form).OfType<TextBox>().OrderBy(control => control.TabIndex).ToArray();
            var save = Descendants(form).OfType<Button>().Single(button => button.Text == "&Speichern");
            var cancel = Descendants(form).OfType<Button>().Single(button => button.Text == "&Abbrechen");
            var validation = Descendants(form).OfType<Label>()
                .Single(label => label.AccessibleName == "Validierungsfehler");

            Assert.Equal(Enumerable.Range(0, 9), textBoxes.Select(control => control.TabIndex));
            Assert.All(textBoxes, control => Assert.False(string.IsNullOrWhiteSpace(control.AccessibleName)));
            Assert.Equal(10, save.TabIndex);
            Assert.Equal(11, cancel.TabIndex);
            Assert.Same(save, form.AcceptButton);
            Assert.Same(cancel, form.CancelButton);

            typeof(ProjectEditorForm)
                .GetMethod("SaveClicked", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(form, [save, EventArgs.Empty]);

            Assert.Contains("erforderlich", validation.Text, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(SystemColors.ControlText, validation.ForeColor);
        });
    }

    [Theory]
    [MemberData(nameof(DpiScaleFactors))]
    public void EditorLayoutRemainsContainedAtBaselineScaleFactors(float scaleFactor)
    {
        RunInSta(() =>
        {
            using var form = CreateEditor();
            form.ClientSize = form.MinimumSize;
            form.Scale(new SizeF(scaleFactor, scaleFactor));
            form.CreateControl();
            form.PerformLayout();

            AssertContained(form);
        });
    }

    [Fact]
    public void MainWindowCommandsExposeKeyboardMnemonicsAndAccessibleNames()
    {
        RunInSta(() =>
        {
            var repository = new EmptyProjectRepository();
            var failures = new OperationFailureHandler(NullLogger<OperationFailureHandler>.Instance);
            var recovery = new NoOpRecoveryService();
            var blockers = new EmptyBlockerRepository();
            var requirements = new EmptyRequirementRepository();
            var references = new EmptyReferenceRepository();
            using var form = new MainForm(
                new CreateProject(repository, TimeProvider.System, failures),
                new LoadProject(repository, failures),
                new ListProjects(repository, failures),
                new UpdateProject(repository, TimeProvider.System, failures),
                new SetProjectArchiveState(repository, TimeProvider.System, failures),
                new UpdateProjectSteering(repository, TimeProvider.System, failures),
                new MarkProjectReviewed(repository, TimeProvider.System, failures),
                new ListProjectBlockers(blockers, failures),
                new AddProjectBlocker(repository, blockers, references, TimeProvider.System, failures),
                new ResolveProjectBlocker(blockers, TimeProvider.System, failures),
                new ListRequirements(requirements, failures),
                new CreateRequirement(repository, requirements, references, failures),
                new UpdateRequirement(requirements, references, failures),
                new ListExternalReferences(references, failures),
                new SaveExternalReference(repository, requirements, references, failures),
                new OpenExternalReference(references, new NoOpReferenceOpener(), failures),
                new ExportProject(repository, new NoOpExportWriter(), TimeProvider.System, failures),
                new(recovery, failures),
                new(recovery, failures),
                "synthetic.db",
                "synthetic-backups",
                "0.1.0");

            var buttons = Descendants(form).OfType<Button>().ToArray();
            Assert.All(buttons, button => Assert.Contains('&', button.Text));
            Assert.All(buttons, button => Assert.False(string.IsNullOrWhiteSpace(button.AccessibleName)));
            Assert.Equal(AutoScaleMode.Dpi, form.AutoScaleMode);
        });
    }

    [Fact]
    public void SteeringAndHelpDialogsRemainKeyboardAndDpiAware()
    {
        RunInSta(() =>
        {
            var failures = new OperationFailureHandler(NullLogger<OperationFailureHandler>.Instance);
            var project = Project.Create(Guid.NewGuid(), "UI", "UI test", null, DateTimeOffset.UtcNow.AddMinutes(-1));
            var repository = new SingleProjectRepository(project);
            using var steering = new ProjectSteeringForm(ProjectDto.FromDomain(project),
                new UpdateProjectSteering(repository, TimeProvider.System, failures),
                new MarkProjectReviewed(repository, TimeProvider.System, failures));
            using var help = new HelpForm();

            Assert.Equal(2, Descendants(steering).OfType<ComboBox>().Count());
            Assert.All(Descendants(steering).OfType<ComboBox>(), control =>
                Assert.False(string.IsNullOrWhiteSpace(control.AccessibleName)));
            Assert.Equal(AutoScaleMode.Dpi, steering.AutoScaleMode);
            Assert.Equal(AutoScaleMode.Dpi, help.AutoScaleMode);
            Assert.Contains("14 Kalendertagen", Descendants(help).OfType<TextBox>().Single().Text,
                StringComparison.Ordinal);
        });
    }

    [Fact]
    public void AuditAndOperatingViewsAreReadOnlyKeyboardAndDpiAware()
    {
        RunInSta(() =>
        {
            using var audit = new ProjectHistoryForm(Guid.NewGuid(), "UI", new EmptyChangeEventReader());
            using var operating = new OperatingInformationForm(new("C:\\data\\pims.db", "C:\\logs",
                "C:\\app", "C:\\backups"));
            var grid = Descendants(audit).OfType<DataGridView>().Single();
            var paths = Descendants(operating).OfType<TextBox>().Single();
            Assert.True(grid.ReadOnly);
            Assert.False(grid.AllowUserToDeleteRows);
            Assert.True(paths.ReadOnly);
            Assert.Contains("vom Benutzer gewählt", paths.Text, StringComparison.Ordinal);
            Assert.Equal(AutoScaleMode.Dpi, audit.AutoScaleMode);
            Assert.Equal(AutoScaleMode.Dpi, operating.AutoScaleMode);
        });
    }

    [Fact]
    public void RequirementAndReferenceDialogsUseNativeAccessibleDpiControls()
    {
        RunInSta(() =>
        {
            var failures = new OperationFailureHandler(NullLogger<OperationFailureHandler>.Instance);
            var projects = new EmptyProjectRepository();
            var requirements = new EmptyRequirementRepository();
            var references = new EmptyReferenceRepository();
            var create = new CreateRequirement(projects, requirements, references, failures);
            var update = new UpdateRequirement(requirements, references, failures);
            var saveReference = new SaveExternalReference(projects, requirements, references, failures);
            using var editor = new RequirementEditorForm(Guid.NewGuid(), create, update, []);
            using var referenceEditor = new ExternalReferenceEditorForm(Guid.NewGuid(), null, saveReference);

            Assert.Equal(AutoScaleMode.Dpi, editor.AutoScaleMode);
            Assert.Equal(AutoScaleMode.Dpi, referenceEditor.AutoScaleMode);
            Assert.All(Descendants(editor).Where(control => control is TextBox or ComboBox or DataGridView),
                control => Assert.False(string.IsNullOrWhiteSpace(control.AccessibleName)));
            Assert.All(Descendants(referenceEditor).OfType<Button>(),
                button => Assert.Contains('&', button.Text));
        });
    }

    [Fact]
    public void SearchAndTraceabilityViewsAreNativeKeyboardAccessibleAndDpiAware()
    {
        RunInSta(() =>
        {
            var projectId = Guid.NewGuid();
            using var search = new SearchForm(new SearchPims(new EmptySearchReader()),
                [new(projectId, "UI", "UI", null, null, ProjectPhase.Idea, ActivityState.Active, null,
                    ReviewFreshness.Current, DueDateIndication.Neutral, [], false)], projectId);
            using var traceability = new TraceabilityForm(projectId, new EmptyTraceabilityReader(projectId));
            Assert.Equal(AutoScaleMode.Dpi, search.AutoScaleMode);
            Assert.Equal(AutoScaleMode.Dpi, traceability.AutoScaleMode);
            Assert.All(Descendants(search).Where(control => control is TextBox or ComboBox or DataGridView),
                control => Assert.False(string.IsNullOrWhiteSpace(control.AccessibleName)));
            Assert.All(Descendants(search).OfType<Button>(), button => Assert.Contains('&', button.Text));
            Assert.Equal("Traceability-Baum", Descendants(traceability).OfType<TreeView>().Single().AccessibleName);
        });
    }

    private static ProjectEditorForm CreateEditor()
    {
        var failures = new OperationFailureHandler(NullLogger<OperationFailureHandler>.Instance);
        return new ProjectEditorForm(new CreateProject(
            new EmptyProjectRepository(),
            TimeProvider.System,
            failures));
    }

    private static void AssertContained(Control parent)
    {
        foreach (var control in parent.Controls.Cast<Control>().Where(control => control.Visible))
        {
            Assert.True(
                parent.ClientRectangle.Contains(control.Bounds),
                $"{control.Name} ({control.GetType().Name}) is clipped at {control.Bounds} in {parent.ClientRectangle}.");
            AssertContained(control);
        }
    }

    private static IEnumerable<Control> Descendants(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (var descendant in Descendants(child))
            {
                yield return descendant;
            }
        }
    }

    private static void RunInSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null)
        {
            throw new InvalidOperationException("The STA UI assertion failed.", failure);
        }
    }

    private sealed class EmptyProjectRepository : IProjectRepository
    {
        public Task<ProjectWriteResult> AddAsync(Project project, CancellationToken cancellationToken) =>
            Task.FromResult(ProjectWriteResult.Saved);

        public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<Project?>(null);

        public Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Project>>([]);

        public Task<ProjectWriteResult> UpdateAsync(
            Project project,
            int expectedRevision,
            CancellationToken cancellationToken) => Task.FromResult(ProjectWriteResult.Saved);
    }

    private sealed class SingleProjectRepository(Project project) : IProjectRepository
    {
        public Task<ProjectWriteResult> AddAsync(Project value, CancellationToken cancellationToken) =>
            Task.FromResult(ProjectWriteResult.Saved);
        public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<Project?>(id == project.Id ? project : null);
        public Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Project>>([project]);
        public Task<ProjectWriteResult> UpdateAsync(Project value, int expectedRevision,
            CancellationToken cancellationToken) => Task.FromResult(ProjectWriteResult.Saved);
    }

    private sealed class NoOpExportWriter : IProjectExportWriter
    {
        public Task WriteAsync(
            ProjectDto project,
            string targetPath,
            DateTimeOffset exportedAtUtc,
            string applicationVersion,
            CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class EmptyBlockerRepository : IProjectBlockerRepository
    {
        public Task AddAsync(ProjectBlocker blocker, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<ProjectBlocker?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<ProjectBlocker?>(null);
        public Task<IReadOnlyList<ProjectBlocker>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ProjectBlocker>>([]);
        public Task<IReadOnlySet<Guid>> GetProjectIdsWithOpenBlockersAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlySet<Guid>>(new HashSet<Guid>());
        public Task<bool> ResolveAsync(ProjectBlocker blocker, CancellationToken cancellationToken) => Task.FromResult(false);
    }

    private sealed class EmptyRequirementRepository : IRequirementRepository
    {
        public Task<string> GetNextKeyAsync(Guid projectId, CancellationToken cancellationToken) => Task.FromResult("REQ-001");
        public Task<RequirementWriteResult> AddAsync(Requirement requirement, CancellationToken cancellationToken) => Task.FromResult(RequirementWriteResult.Saved);
        public Task<RequirementWriteResult> UpdateAsync(Requirement requirement, int expectedRevision, CancellationToken cancellationToken) => Task.FromResult(RequirementWriteResult.Saved);
        public Task<Requirement?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<Requirement?>(null);
        public Task<IReadOnlyList<Requirement>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Requirement>>([]);
    }

    private sealed class EmptyReferenceRepository : IExternalReferenceRepository
    {
        public Task<RequirementWriteResult> AddAsync(ExternalReference reference, CancellationToken cancellationToken) => Task.FromResult(RequirementWriteResult.Saved);
        public Task<RequirementWriteResult> UpdateAsync(ExternalReference reference, int expectedRevision, CancellationToken cancellationToken) => Task.FromResult(RequirementWriteResult.Saved);
        public Task<ExternalReference?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<ExternalReference?>(null);
        public Task<IReadOnlyList<ExternalReference>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<ExternalReference>>([]);
    }

    private sealed class NoOpReferenceOpener : IExternalReferenceOpener
    { public Task OpenAsync(string target, CancellationToken cancellationToken) => Task.CompletedTask; }

    private sealed class EmptySearchReader : ISearchReader
    {
        public Task<SearchPage> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default) =>
            Task.FromResult(new SearchPage([], 0, false));
    }

    private sealed class EmptyChangeEventReader : IChangeEventReader
    {
        public Task<IReadOnlyList<ProjectChangeEventRow>> ListByProjectAsync(Guid projectId,
            CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ProjectChangeEventRow>>([]);
    }

    private sealed class EmptyTraceabilityReader(Guid projectId) : ITraceabilityReader
    {
        public Task<TraceabilityNode?> GetProjectAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<TraceabilityNode?>(new(TraceabilityNodeType.Project, projectId, projectId, "UI", []));
    }

    private sealed class NoOpRecoveryService :
        Sasd.Pims.Application.Recovery.IDatabaseBackupService,
        Sasd.Pims.Application.Recovery.IDatabaseRestoreService
    {
        public Task CreateAsync(
            string databasePath,
            string packagePath,
            string applicationVersion,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<string> RestoreAsync(
            string databasePath,
            string packagePath,
            string rollbackDirectory,
            string applicationVersion,
            CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
    }
}
