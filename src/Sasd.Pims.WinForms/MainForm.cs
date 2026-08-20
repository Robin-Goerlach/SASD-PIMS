using Sasd.Pims.Application.Projects;
using Sasd.Pims.Application.Recovery;
using System.Globalization;

namespace Sasd.Pims.WinForms;

/// <summary>Provides the German master-detail project catalog and operational recovery commands.</summary>
public sealed class MainForm : Form
{
    private readonly CreateProject _create;
    private readonly LoadProject _load;
    private readonly ListProjects _list;
    private readonly UpdateProject _update;
    private readonly SetProjectArchiveState _archive;
    private readonly UpdateProjectSteering _updateSteering;
    private readonly MarkProjectReviewed _markReviewed;
    private readonly ListProjectBlockers _listBlockers;
    private readonly AddProjectBlocker _addBlocker;
    private readonly ResolveProjectBlocker _resolveBlocker;
    private readonly ExportProject _export;
    private readonly CreateDatabaseBackup _backup;
    private readonly RestoreDatabaseBackup _restore;
    private readonly string _databasePath;
    private readonly string _rollbackDirectory;
    private readonly string _version;
    private readonly ListBox _projects = new();
    private readonly TextBox _search = new();
    private readonly CheckBox _includeArchived = new();
    private readonly CheckBox _needsAttention = new();
    private readonly Label _details = new();
    private readonly Button _edit = new();
    private readonly Button _archiveButton = new();
    private readonly Button _steeringButton = new();
    private readonly Button _blockersButton = new();
    private readonly ToolStripStatusLabel _status = new("Bereit");
    private ProjectDto? _selected;

    public MainForm(CreateProject createProject, LoadProject loadProject, ListProjects listProjects,
        UpdateProject updateProject, SetProjectArchiveState archiveProject,
        UpdateProjectSteering updateSteering, MarkProjectReviewed markReviewed,
        ListProjectBlockers listBlockers, AddProjectBlocker addBlocker, ResolveProjectBlocker resolveBlocker,
        ExportProject exportProject,
        CreateDatabaseBackup createBackup, RestoreDatabaseBackup restoreBackup, string databasePath,
        string rollbackDirectory, string applicationVersion)
    {
        _create = createProject;
        _load = loadProject;
        _list = listProjects;
        _update = updateProject;
        _archive = archiveProject;
        _updateSteering = updateSteering;
        _markReviewed = markReviewed;
        _listBlockers = listBlockers;
        _addBlocker = addBlocker;
        _resolveBlocker = resolveBlocker;
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
        _steeringButton.Text = "&Steuerung"; Configure(_steeringButton, "Status, Termin und Review bearbeiten", SteeringClicked);
        _blockersButton.Text = "&Blocker"; Configure(_blockersButton, "Blocker und Verlauf bearbeiten", BlockersClicked);
        var export = Button("&JSON exportieren", "Ausgewähltes Projekt als JSON exportieren", ExportClicked);
        var backup = Button("&Sicherung", "Datenbanksicherung erstellen", BackupClicked);
        var restore = Button("&Wiederherstellen", "Datenbanksicherung wiederherstellen", RestoreClicked);
        var commands = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top };
        commands.Controls.AddRange([newButton, _edit, _steeringButton, _blockersButton, _archiveButton, export, backup, restore]);

        _search.Dock = DockStyle.Fill;
        _search.AccessibleName = "Projekte nach Kennung oder Name suchen";
        _search.PlaceholderText = "Kennung oder Name";
        _search.TextChanged += async (_, _) => await RefreshProjectsAsync();
        _includeArchived.Text = "&Archivierte anzeigen";
        _includeArchived.AutoSize = true;
        _includeArchived.AccessibleName = "Archivierte Projekte anzeigen";
        _includeArchived.CheckedChanged += async (_, _) => await RefreshProjectsAsync();
        _needsAttention.Text = "&Nur Handlungsbedarf";
        _needsAttention.AutoSize = true;
        _needsAttention.AccessibleName = "Nur Projekte mit Handlungsbedarf anzeigen";
        _needsAttention.CheckedChanged += async (_, _) => await RefreshProjectsAsync();
        var filters = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 4 };
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        filters.Controls.Add(new Label { Text = "&Suchen", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        filters.Controls.Add(_search, 1, 0);
        filters.Controls.Add(_includeArchived, 2, 0);
        filters.Controls.Add(_needsAttention, 3, 0);

        _projects.Dock = DockStyle.Fill;
        _projects.AccessibleName = "Projektliste";
        _projects.FormattingEnabled = true;
        _projects.Format += (_, args) =>
        {
            if (args.ListItem is ProjectSummaryDto summary) args.Value = FormatSummary(summary);
        };
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
        var menu = new MenuStrip();
        var helpMenu = new ToolStripMenuItem("&Hilfe");
        var glossary = new ToolStripMenuItem("&Hilfe und Glossar (F1)");
        glossary.Click += (_, _) => { using var help = new HelpForm(); help.ShowDialog(this); };
        helpMenu.DropDownItems.Add(glossary);
        var about = new ToolStripMenuItem("&Über SASD PIMS");
        about.Click += (_, _) => MessageBox.Show(this, $"SASD PIMS {_version}\nLokaler Projektkatalog",
            "Über SASD PIMS", MessageBoxButtons.OK, MessageBoxIcon.Information);
        helpMenu.DropDownItems.Add(about);
        menu.Items.Add(helpMenu);
        Controls.Add(content);
        Controls.Add(statusStrip);
        Controls.Add(menu);
        MainMenuStrip = menu;
        KeyPreview = true;
        KeyDown += (_, args) =>
        {
            if (args.KeyCode != Keys.F1) return;
            using var help = new HelpForm();
            help.ShowDialog(this);
            args.Handled = true;
        };
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

    private async void SteeringClicked(object? sender, EventArgs e)
    {
        if (_selected is null) return;
        using var dialog = new ProjectSteeringForm(_selected, _updateSteering, _markReviewed);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            await RefreshProjectsAsync(_selected.Id);
            _status.Text = "Projektsteuerung gespeichert.";
        }
    }

    private async void BlockersClicked(object? sender, EventArgs e)
    {
        if (_selected is null) return;
        using var dialog = new ProjectBlockersForm(_selected, _listBlockers, _addBlocker, _resolveBlocker);
        dialog.ShowDialog(this);
        await RefreshProjectsAsync(_selected.Id);
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
        _steeringButton.Enabled = enabled;
        _blockersButton.Enabled = enabled;
        if (enabled) _archiveButton.Text = _selected!.IsArchived ? "&Reaktivieren" : "&Archivieren";
    }

    private async Task RefreshProjectsAsync(Guid? preferredId = null)
    {
        preferredId ??= (_projects.SelectedItem as ProjectSummaryDto)?.Id;
        var result = await _list.ExecuteAsync(new ProjectCatalogFilter(
            IncludeArchived: _includeArchived.Checked, SearchText: _search.Text,
            NeedsAttentionOnly: _needsAttention.Checked));
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
        Phase: {German(project.Phase)}
        Aktivität: {German(project.ActivityState)}
        Zieltermin: {project.TargetDate?.ToString("d", CultureInfo.CurrentCulture) ?? "–"}
        Letztes Review: {project.LastReviewedAtUtc?.ToLocalTime().ToString("g", CultureInfo.CurrentCulture) ?? "–"}
        Nächstes Review: {project.NextReviewDueAtUtc?.ToLocalTime().ToString("g", CultureInfo.CurrentCulture) ?? "–"}
        Zustand: {(project.IsArchived ? "Archiviert" : "Aktiv")}
        Revision: {project.Revision}
        """;

    internal static string German(Sasd.Pims.Domain.Projects.ProjectPhase value) => value switch
    {
        Sasd.Pims.Domain.Projects.ProjectPhase.Idea => "Idee",
        Sasd.Pims.Domain.Projects.ProjectPhase.Preparation => "Vorbereitung",
        Sasd.Pims.Domain.Projects.ProjectPhase.Execution => "Umsetzung",
        Sasd.Pims.Domain.Projects.ProjectPhase.Validation => "Validierung",
        _ => "Abschluss",
    };

    internal static string German(Sasd.Pims.Domain.Projects.ActivityState value) => value switch
    {
        Sasd.Pims.Domain.Projects.ActivityState.NotStarted => "Nicht begonnen",
        Sasd.Pims.Domain.Projects.ActivityState.Active => "Aktiv",
        Sasd.Pims.Domain.Projects.ActivityState.Paused => "Pausiert",
        Sasd.Pims.Domain.Projects.ActivityState.Completed => "Abgeschlossen",
        _ => "Abgebrochen",
    };

    private static string FormatSummary(ProjectSummaryDto summary)
    {
        var attention = summary.NeedsAttention ? " ⚠ Handlungsbedarf" : string.Empty;
        var archive = summary.IsArchived ? " [Archiviert]" : string.Empty;
        return $"{summary.Key} — {summary.Name} · {German(summary.Phase)} / {German(summary.ActivityState)} · " +
            $"{German(summary.ReviewFreshness)} · {German(summary.DueDateIndication)}{attention}{archive}";
    }

    private static string German(Sasd.Pims.Domain.Projects.ReviewFreshness value) => value switch
    {
        Sasd.Pims.Domain.Projects.ReviewFreshness.NotScheduled => "Review nicht geplant",
        Sasd.Pims.Domain.Projects.ReviewFreshness.NotReviewed => "Noch nicht geprüft",
        Sasd.Pims.Domain.Projects.ReviewFreshness.Current => "Review aktuell",
        Sasd.Pims.Domain.Projects.ReviewFreshness.DueToday => "Review heute fällig",
        _ => "Review überfällig",
    };

    private static string German(Sasd.Pims.Domain.Projects.DueDateIndication value) => value switch
    {
        Sasd.Pims.Domain.Projects.DueDateIndication.OnTrack => "Termin im Plan",
        Sasd.Pims.Domain.Projects.DueDateIndication.DueSoon => "Termin bald fällig",
        Sasd.Pims.Domain.Projects.DueDateIndication.DueToday => "Termin heute fällig",
        Sasd.Pims.Domain.Projects.DueDateIndication.Overdue => "Termin überfällig",
        _ => "Kein Terminsignal",
    };

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
