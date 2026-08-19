using Sasd.Pims.Application.Projects;

namespace Sasd.Pims.WinForms;

public sealed class MainForm : Form
{
    private readonly CreateProject _createProject;
    private readonly LoadProject _loadProject;
    private readonly ListProjects _listProjects;
    private readonly ExportProject _exportProject;
    private readonly ListBox _projectList = new();
    private readonly Button _openButton = new();
    private readonly ToolStripStatusLabel _statusLabel = new("Ready");

    public MainForm(
        CreateProject createProject,
        LoadProject loadProject,
        ListProjects listProjects,
        ExportProject exportProject)
    {
        _createProject = createProject;
        _loadProject = loadProject;
        _listProjects = listProjects;
        _exportProject = exportProject;
        InitializeControls();
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await RefreshProjectsAsync();
    }

    private void InitializeControls()
    {
        Text = "SASD PIMS";
        Name = nameof(MainForm);
        AccessibleName = "SASD PIMS main window";
        AutoScaleMode = AutoScaleMode.Dpi;
        MinimumSize = new Size(640, 420);
        StartPosition = FormStartPosition.CenterScreen;

        var newButton = new Button
        {
            Text = "&New project",
            AutoSize = true,
            TabIndex = 0,
            AccessibleName = "New project",
        };
        newButton.Click += NewProjectClicked;

        _projectList.Dock = DockStyle.Fill;
        _projectList.TabIndex = 1;
        _projectList.AccessibleName = "Projects";
        _projectList.SelectedIndexChanged += (_, _) => _openButton.Enabled = _projectList.SelectedItem is not null;
        _projectList.DoubleClick += OpenProjectClicked;

        _openButton.Text = "&Open";
        _openButton.AutoSize = true;
        _openButton.Enabled = false;
        _openButton.TabIndex = 2;
        _openButton.AccessibleName = "Open selected project";
        _openButton.Click += OpenProjectClicked;

        var exportButton = new Button
        {
            Text = "&Export JSON",
            AutoSize = true,
            TabIndex = 3,
            AccessibleName = "Export selected project as JSON",
        };
        exportButton.Click += ExportProjectClicked;

        var projectLabel = new Label
        {
            Text = "&Projects",
            AutoSize = true,
            TabIndex = 3,
        };
        projectLabel.SetBounds(0, 0, 0, 0);

        var commands = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 0, 0, 8),
        };
        commands.Controls.Add(newButton);
        commands.Controls.Add(_openButton);
        commands.Controls.Add(exportButton);

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 1,
            RowCount = 3,
        };
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        content.Controls.Add(commands, 0, 0);
        content.Controls.Add(projectLabel, 0, 1);
        content.Controls.Add(_projectList, 0, 2);

        var statusStrip = new StatusStrip();
        statusStrip.Items.Add(_statusLabel);
        Controls.Add(content);
        Controls.Add(statusStrip);
    }

    private async void NewProjectClicked(object? sender, EventArgs e)
    {
        using var editor = new ProjectEditorForm(_createProject);
        if (editor.ShowDialog(this) == DialogResult.OK)
        {
            await RefreshProjectsAsync();
            _statusLabel.Text = "Project saved.";
        }
    }

    private async void OpenProjectClicked(object? sender, EventArgs e)
    {
        if (_projectList.SelectedItem is not ProjectSummaryDto selected)
        {
            return;
        }

        var result = await _loadProject.ExecuteAsync(selected.Id);
        if (result.Status == ProjectOperationStatus.Success && result.Value is not null)
        {
            using var editor = new ProjectEditorForm(result.Value);
            editor.ShowDialog(this);
            _statusLabel.Text = "Project loaded.";
        }
    }

    private async Task RefreshProjectsAsync()
    {
        var selectedId = (_projectList.SelectedItem as ProjectSummaryDto)?.Id;
        var result = await _listProjects.ExecuteAsync();
        if (result.Status != ProjectOperationStatus.Success || result.Value is null)
        {
            _statusLabel.Text = $"Could not load projects. Error ID: {result.ErrorId}";
            return;
        }

        var projects = result.Value;
        _projectList.DataSource = projects.ToList();
        if (selectedId is not null)
        {
            _projectList.SelectedItem = projects.FirstOrDefault(project => project.Id == selectedId);
        }
    }

    private async void ExportProjectClicked(object? sender, EventArgs e)
    {
        if (_projectList.SelectedItem is not ProjectSummaryDto selected)
        {
            _statusLabel.Text = "Select a project to export.";
            return;
        }

        using var dialog = new SaveFileDialog
        {
            AddExtension = true,
            DefaultExt = "json",
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            FileName = $"{selected.Key}.json",
            OverwritePrompt = true,
            Title = "Export project",
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var version = typeof(MainForm).Assembly.GetName().Version?.ToString() ?? "unknown";
        var result = await _exportProject.ExecuteAsync(selected.Id, dialog.FileName, version);
        _statusLabel.Text = result.Status == ProjectOperationStatus.Success
            ? "Project exported."
            : $"Project export failed. Error ID: {result.ErrorId}";
    }
}
