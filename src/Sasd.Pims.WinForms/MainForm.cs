using Sasd.Pims.Application.Projects;
using Sasd.Pims.Application.Recovery;

namespace Sasd.Pims.WinForms;

/// <summary>Provides the German master-detail project catalog and operational recovery commands.</summary>
public sealed class MainForm : Form
{
    private readonly CreateProject _create;
    private readonly LoadProject _load;
    private readonly ListProjects _list;
    private readonly UpdateProject _update;
    private readonly SetProjectArchiveState _archive;
    private readonly ExportProject _export;
    private readonly CreateDatabaseBackup _backup;
    private readonly RestoreDatabaseBackup _restore;
    private readonly string _databasePath;
    private readonly string _rollbackDirectory;
    private readonly string _version;
    private readonly ListBox _projects = new();
    private readonly TextBox _search = new();
    private readonly CheckBox _includeArchived = new();
    private readonly Label _details = new();
    private readonly Button _edit = new();
    private readonly Button _archiveButton = new();
    private readonly ToolStripStatusLabel _status = new("Bereit");
    private ProjectDto? _selected;

    public MainForm(CreateProject createProject, LoadProject loadProject, ListProjects listProjects,
        UpdateProject updateProject, SetProjectArchiveState archiveProject, ExportProject exportProject,
        CreateDatabaseBackup createBackup, RestoreDatabaseBackup restoreBackup, string databasePath,
        string rollbackDirectory, string applicationVersion)
    {
        _create = createProject;
        _load = loadProject;
        _list = listProjects;
        _update = updateProject;
        _archive = archiveProject;
        _export = exportProject;
        _backup = createBackup;
        _restore = restoreBackup;
        _databasePath = databasePath;
        _rollbackDirectory = rollbackDirectory;
        _version = applicationVersion;
        InitializeControls();
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await RefreshProjectsAsync();
    }

    private void InitializeControls()
    {
        Text = "SASD PIMS – Projektkatalog";
        Name = nameof(MainForm);
        AccessibleName = "SASD PIMS Projektkatalog";
        AutoScaleMode = AutoScaleMode.Dpi;
        MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;

        var newButton = Button("&Neu", "Neues Projekt anlegen", NewClicked);
        _edit.Text = "&Bearbeiten"; Configure(_edit, "Ausgewähltes Projekt bearbeiten", EditClicked);
        _archiveButton.Text = "&Archivieren"; Configure(_archiveButton, "Projekt archivieren oder reaktivieren", ArchiveClicked);
        var export = Button("&JSON exportieren", "Ausgewähltes Projekt als JSON exportieren", ExportClicked);
        var backup = Button("&Sicherung", "Datenbanksicherung erstellen", BackupClicked);
        var restore = Button("&Wiederherstellen", "Datenbanksicherung wiederherstellen", RestoreClicked);
        var commands = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top };
        commands.Controls.AddRange([newButton, _edit, _archiveButton, export, backup, restore]);

        _search.Dock = DockStyle.Fill;
        _search.AccessibleName = "Projekte nach Kennung oder Name suchen";
        _search.PlaceholderText = "Kennung oder Name";
        _search.TextChanged += async (_, _) => await RefreshProjectsAsync();
        _includeArchived.Text = "&Archivierte anzeigen";
        _includeArchived.AutoSize = true;
        _includeArchived.AccessibleName = "Archivierte Projekte anzeigen";
        _includeArchived.CheckedChanged += async (_, _) => await RefreshProjectsAsync();
        var filters = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3 };
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        filters.Controls.Add(new Label { Text = "&Suchen", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        filters.Controls.Add(_search, 1, 0);
        filters.Controls.Add(_includeArchived, 2, 0);

        _projects.Dock = DockStyle.Fill;
        _projects.AccessibleName = "Projektliste";
        _projects.SelectedIndexChanged += async (_, _) => await SelectionChangedAsync();
        _projects.DoubleClick += EditClicked;
        _details.Dock = DockStyle.Fill;
        _details.AutoSize = false;
        _details.Padding = new Padding(12);
        _details.AccessibleName = "Projektdetails";
        _details.Text = "Wählen Sie ein Projekt aus.";
        var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 330, FixedPanel = FixedPanel.Panel1 };
        split.Panel1.Controls.Add(_projects);
        split.Panel2.Controls.Add(_details);

        var content = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 1, RowCount = 3 };
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        content.Controls.Add(commands, 0, 0);
        content.Controls.Add(filters, 0, 1);
        content.Controls.Add(split, 0, 2);
        var statusStrip = new StatusStrip();
        statusStrip.Items.Add(_status);
        Controls.Add(content);
        Controls.Add(statusStrip);
        SetSelectionState(false);
    }

    private static Button Button(string text, string accessibleName, EventHandler handler)
    {
        var button = new Button { Text = text, AutoSize = true };
        Configure(button, accessibleName, handler);
        return button;
    }

    private static void Configure(Button button, string accessibleName, EventHandler handler)
    {
        button.AutoSize = true;
        button.AccessibleName = accessibleName;
        button.Click += handler;
    }

    private async void NewClicked(object? sender, EventArgs e)
    {
        using var editor = new ProjectEditorForm(_create);
        if (editor.ShowDialog(this) == DialogResult.OK) { await RefreshProjectsAsync(); _status.Text = "Projekt gespeichert."; }
    }

    private async void EditClicked(object? sender, EventArgs e)
    {
        if (_selected is null) return;
        using var editor = new ProjectEditorForm(_selected, _update);
        if (editor.ShowDialog(this) == DialogResult.OK) { await RefreshProjectsAsync(_selected.Id); _status.Text = "Änderungen gespeichert."; }
    }

    private async void ArchiveClicked(object? sender, EventArgs e)
    {
        if (_selected is null) return;
        var archive = !_selected.IsArchived;
        var prompt = archive
            ? "Projekt archivieren? Alle Daten bleiben erhalten und das Projekt kann reaktiviert werden."
            : "Projekt wieder in den aktiven Katalog aufnehmen?";
        if (MessageBox.Show(this, prompt, archive ? "Projekt archivieren" : "Projekt reaktivieren",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.OK) return;
        var result = await _archive.ExecuteAsync(_selected.Id, _selected.Revision, archive);
        if (result.Status == ProjectOperationStatus.Success)
        {
            await RefreshProjectsAsync(result.Value?.Id);
            _status.Text = archive ? "Projekt archiviert." : "Projekt reaktiviert.";
        }
        else _status.Text = FailureText("Archivstatus konnte nicht geändert werden", result);
    }

    private async Task SelectionChangedAsync()
    {
        if (_projects.SelectedItem is not ProjectSummaryDto summary)
        {
            _selected = null; _details.Text = "Wählen Sie ein Projekt aus."; SetSelectionState(false); return;
        }
        var result = await _load.ExecuteAsync(summary.Id);
        if (result.Status != ProjectOperationStatus.Success || result.Value is null)
        {
            _selected = null; SetSelectionState(false); _status.Text = FailureText("Projekt konnte nicht geladen werden", result); return;
        }
        _selected = result.Value;
        SetSelectionState(true);
        _details.Text = FormatDetails(_selected);
    }

    private void SetSelectionState(bool enabled)
    {
        _edit.Enabled = enabled;
        _archiveButton.Enabled = enabled;
        if (enabled) _archiveButton.Text = _selected!.IsArchived ? "&Reaktivieren" : "&Archivieren";
    }

    private async Task RefreshProjectsAsync(Guid? preferredId = null)
    {
        preferredId ??= (_projects.SelectedItem as ProjectSummaryDto)?.Id;
        var result = await _list.ExecuteAsync(new ProjectCatalogFilter(_includeArchived.Checked, _search.Text));
        if (result.Status != ProjectOperationStatus.Success || result.Value is null)
        {
            _status.Text = FailureText("Projekte konnten nicht geladen werden", result); return;
        }
        var items = result.Value.ToList();
        _projects.DataSource = items;
        _projects.SelectedItem = preferredId is null ? null : items.FirstOrDefault(item => item.Id == preferredId);
        _status.Text = items.Count == 0 ? "Keine passenden Projekte vorhanden." : $"{items.Count} Projekt(e) angezeigt.";
    }

    private static string FormatDetails(ProjectDto project) => $"""
        {project.Key} – {project.Name}

        Kurzbeschreibung: {project.ShortDescription ?? "–"}
        Ziel: {project.Goal ?? "–"}
        Nutzen: {project.Benefit ?? "–"}
        Projektart: {project.ProjectType ?? "–"}
        Projektbereich: {project.ProjectArea ?? "–"}
        Verantwortlichkeit: {project.Responsibility ?? "–"}
        Tags: {(project.Tags.Count == 0 ? "–" : string.Join(", ", project.Tags))}
        Zustand: {(project.IsArchived ? "Archiviert" : "Aktiv")}
        Revision: {project.Revision}
        """;

    private async void ExportClicked(object? sender, EventArgs e)
    {
        if (_selected is null) { _status.Text = "Bitte wählen Sie ein Projekt aus."; return; }
        using var dialog = new SaveFileDialog { AddExtension = true, DefaultExt = "json", Filter = "JSON-Dateien (*.json)|*.json|Alle Dateien (*.*)|*.*", FileName = $"{_selected.Key}.json", OverwritePrompt = true, Title = "Projekt exportieren" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var result = await _export.ExecuteAsync(_selected.Id, dialog.FileName, _version);
        _status.Text = result.Status == ProjectOperationStatus.Success ? "Projekt exportiert." : FailureText("Export fehlgeschlagen", result);
    }

    private async void BackupClicked(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog { AddExtension = true, DefaultExt = "zip", Filter = "SASD-PIMS-Sicherungen (*.zip)|*.zip|Alle Dateien (*.*)|*.*", FileName = $"pims-sicherung-{DateTimeOffset.Now:yyyyMMdd-HHmmss}.zip", OverwritePrompt = true, Title = "Sicherung erstellen" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var result = await _backup.ExecuteAsync(_databasePath, dialog.FileName, _version);
        _status.Text = result.Status == ProjectOperationStatus.Success ? "Sicherung erstellt und geprüft." : FailureText("Sicherung fehlgeschlagen", result);
    }

    private async void RestoreClicked(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog { CheckFileExists = true, Filter = "SASD-PIMS-Sicherungen (*.zip)|*.zip|Alle Dateien (*.*)|*.*", Title = "Sicherung wiederherstellen" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        if (MessageBox.Show(this, "Ausgewählte Sicherung wiederherstellen? Der aktuelle Datenbestand wird zuvor als Rückrollsicherung gespeichert.", "Sicherung wiederherstellen", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.OK) return;
        var result = await _restore.ExecuteAsync(_databasePath, dialog.FileName, _rollbackDirectory, _version);
        if (result.Status == ProjectOperationStatus.Success) { await RefreshProjectsAsync(); _status.Text = "Sicherung wiederhergestellt und geprüft."; }
        else _status.Text = FailureText("Wiederherstellung fehlgeschlagen; aktive Daten blieben erhalten", result);
    }

    private static string FailureText<T>(string prefix, ProjectOperationResult<T> result) =>
        string.IsNullOrWhiteSpace(result.ErrorId) ? $"{prefix}." : $"{prefix}. Fehler-ID: {result.ErrorId}";
}
