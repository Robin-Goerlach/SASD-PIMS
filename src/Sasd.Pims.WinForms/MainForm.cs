using Sasd.Pims.Application.Projects;
using Sasd.Pims.Application.Recovery;
using Sasd.Pims.Application.Requirements;
using Sasd.Pims.Application.Search;
using Sasd.Pims.Application.Traceability;
using Sasd.Pims.Application.Exchange;
using Sasd.Pims.Application.Auditing;
using Sasd.Pims.Application.Diagnostics;
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
    private readonly ListRequirements _listRequirements;
    private readonly CreateRequirement _createRequirement;
    private readonly UpdateRequirement _updateRequirement;
    private readonly ListExternalReferences _listReferences;
    private readonly SaveExternalReference _saveReference;
    private readonly OpenExternalReference _openReference;
    private readonly ExportProject _export;
    private readonly CreateDatabaseBackup _backup;
    private readonly RestoreDatabaseBackup _restore;
    private readonly string _databasePath;
    private readonly string _rollbackDirectory;
    private readonly string _version;
    private readonly SearchPims? _globalSearch;
    private readonly ITraceabilityReader? _traceability;
    private readonly IPortableExchangeService? _portableExchange;
    private readonly IChangeEventReader? _changeEvents;
    private readonly OperatingPathsInfo? _operatingPaths;
    private readonly ListBox _projects = new();
    private readonly TextBox _search = new();
    private readonly CheckBox _includeArchived = new();
    private readonly CheckBox _needsAttention = new();
    private readonly ComboBox _phaseFilter = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _activityFilter = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _reviewFilter = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _typeFilter = new();
    private readonly TextBox _areaFilter = new();
    private readonly TextBox _tagFilter = new();
    private readonly Label _details = new();
    private readonly Button _edit = new();
    private readonly Button _archiveButton = new();
    private readonly Button _steeringButton = new();
    private readonly Button _blockersButton = new();
    private readonly Button _requirementsButton = new();
    private readonly ToolStripStatusLabel _status = new("Bereit");
    private ProjectDto? _selected;

    public MainForm(CreateProject createProject, LoadProject loadProject, ListProjects listProjects,
        UpdateProject updateProject, SetProjectArchiveState archiveProject,
        UpdateProjectSteering updateSteering, MarkProjectReviewed markReviewed,
        ListProjectBlockers listBlockers, AddProjectBlocker addBlocker, ResolveProjectBlocker resolveBlocker,
        ListRequirements listRequirements, CreateRequirement createRequirement, UpdateRequirement updateRequirement,
        ListExternalReferences listReferences, SaveExternalReference saveReference,
        OpenExternalReference openReference,
        ExportProject exportProject,
        CreateDatabaseBackup createBackup, RestoreDatabaseBackup restoreBackup, string databasePath,
        string rollbackDirectory, string applicationVersion, SearchPims? globalSearch = null,
        ITraceabilityReader? traceability = null, IPortableExchangeService? portableExchange = null,
        IChangeEventReader? changeEvents = null, OperatingPathsInfo? operatingPaths = null)
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
        _listRequirements = listRequirements;
        _createRequirement = createRequirement;
        _updateRequirement = updateRequirement;
        _listReferences = listReferences;
        _saveReference = saveReference;
        _openReference = openReference;
        _export = exportProject;
        _backup = createBackup;
        _restore = restoreBackup;
        _databasePath = databasePath;
        _rollbackDirectory = rollbackDirectory;
        _version = applicationVersion;
        _globalSearch = globalSearch;
        _traceability = traceability;
        _portableExchange = portableExchange;
        _changeEvents = changeEvents;
        _operatingPaths = operatingPaths;
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
        _requirementsButton.Text = "&Anforderungen"; Configure(_requirementsButton, "Anforderungen und Referenzen bearbeiten", RequirementsClicked);
        var export = Button("&JSON exportieren", "Ausgewähltes Projekt als JSON exportieren", ExportClicked);
        var backup = Button("&Sicherung", "Datenbanksicherung erstellen", BackupClicked);
        var restore = Button("&Wiederherstellen", "Datenbanksicherung wiederherstellen", RestoreClicked);
        var commands = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top };
        commands.Controls.AddRange([newButton, _edit, _steeringButton, _blockersButton, _requirementsButton, _archiveButton, export, backup, restore]);

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
        ConfigureFilter(_phaseFilter, "Projektphase filtern", Enum.GetValues<Sasd.Pims.Domain.Projects.ProjectPhase>().Cast<object>());
        ConfigureFilter(_activityFilter, "Aktivitätszustand filtern", Enum.GetValues<Sasd.Pims.Domain.Projects.ActivityState>().Cast<object>());
        ConfigureFilter(_reviewFilter, "Review-Aktualität filtern", Enum.GetValues<Sasd.Pims.Domain.Projects.ReviewFreshness>().Cast<object>());
        ConfigureTextFilter(_typeFilter, "Projektart filtern"); ConfigureTextFilter(_areaFilter, "Projektbereich filtern");
        ConfigureTextFilter(_tagFilter, "Tag filtern");
        var resetFilters = Button("Filter &zurücksetzen", "Alle Projektfilter zurücksetzen", ResetProjectFiltersClicked);
        var filters = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 8, RowCount = 2 };
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        filters.Controls.Add(new Label { Text = "&Suchen", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        filters.Controls.Add(_search, 1, 0);
        filters.Controls.Add(_includeArchived, 2, 0);
        filters.Controls.Add(_needsAttention, 3, 0);
        filters.Controls.Add(_phaseFilter, 0, 1); filters.Controls.Add(_activityFilter, 1, 1);
        filters.Controls.Add(_reviewFilter, 2, 1); filters.Controls.Add(_typeFilter, 3, 1);
        filters.Controls.Add(_areaFilter, 4, 1); filters.Controls.Add(_tagFilter, 5, 1);
        filters.Controls.Add(resetFilters, 6, 1);

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
        var fileMenu = new ToolStripMenuItem("&Datei");
        var fullExport = new ToolStripMenuItem("&Vollständiger JSON-Export");
        fullExport.Click += FullExportClicked;
        var markdownExport = new ToolStripMenuItem("&Markdown-Projektsteckbrief");
        markdownExport.Click += MarkdownExportClicked;
        fileMenu.DropDownItems.AddRange([fullExport, markdownExport]);
        menu.Items.Add(fileMenu);
        var projectMenu = new ToolStripMenuItem("&Projekt");
        var requirementsMenu = new ToolStripMenuItem("&Anforderungen und Referenzen");
        requirementsMenu.Click += RequirementsClicked;
        projectMenu.DropDownItems.Add(requirementsMenu);
        var traceability = new ToolStripMenuItem("&Traceability");
        traceability.Click += TraceabilityClicked;
        projectMenu.DropDownItems.Add(traceability);
        var history = new ToolStripMenuItem("Ä&nderungsverlauf");
        history.Click += HistoryClicked;
        projectMenu.DropDownItems.Add(history);
        menu.Items.Add(projectMenu);
        var viewMenu = new ToolStripMenuItem("&Ansicht");
        var globalSearch = new ToolStripMenuItem("&Globale Suche (Strg+F)") { ShortcutKeys = Keys.Control | Keys.F };
        globalSearch.Click += SearchClicked;
        viewMenu.DropDownItems.Add(globalSearch);
        menu.Items.Add(viewMenu);
        var helpMenu = new ToolStripMenuItem("&Hilfe");
        var glossary = new ToolStripMenuItem("&Hilfe und Glossar (F1)");
        glossary.Click += (_, _) => { using var help = new HelpForm(); help.ShowDialog(this); };
        helpMenu.DropDownItems.Add(glossary);
        var operatingInformation = new ToolStripMenuItem("&Betriebsinformationen und Speicherorte");
        operatingInformation.Click += (_, _) =>
        {
            if (_operatingPaths is null) { _status.Text = "Betriebsinformationen sind nicht verfügbar."; return; }
            using var form = new OperatingInformationForm(_operatingPaths); form.ShowDialog(this);
        };
        helpMenu.DropDownItems.Add(operatingInformation);
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
            if (args.Control && args.KeyCode == Keys.F) { SearchClicked(this, EventArgs.Empty); args.Handled = true; return; }
            if (args.KeyCode != Keys.F1) return;
            using var help = new HelpForm();
            help.ShowDialog(this);
            args.Handled = true;
        };
        SetSelectionState(false);
    }

    private void HistoryClicked(object? sender, EventArgs e)
    {
        if (_selected is null) { _status.Text = "Bitte wählen Sie ein Projekt aus."; return; }
        if (_changeEvents is null) { _status.Text = "Änderungsverlauf ist nicht verfügbar."; return; }
        using var form = new ProjectHistoryForm(_selected.Id, _selected.Key, _changeEvents);
        form.ShowDialog(this);
    }

    private async void SearchClicked(object? sender, EventArgs e)
    {
        if (_globalSearch is null) { _status.Text = "Globale Suche ist nicht verfügbar."; return; }
        var projects = (_projects.DataSource as IEnumerable<ProjectSummaryDto>)?.ToArray() ?? [];
        using var form = new SearchForm(_globalSearch, projects, _selected?.Id);
        if (form.ShowDialog(this) != DialogResult.OK || form.SelectedResult is null) return;
        await NavigateToAsync(form.SelectedResult.ProjectId, form.SelectedResult.ObjectType);
    }

    private async void TraceabilityClicked(object? sender, EventArgs e)
    {
        if (_selected is null) { _status.Text = "Bitte wählen Sie ein Projekt aus."; return; }
        if (_traceability is null) { _status.Text = "Traceability ist nicht verfügbar."; return; }
        using var form = new TraceabilityForm(_selected.Id, _traceability);
        if (form.ShowDialog(this) != DialogResult.OK || form.SelectedNode is null) return;
        var type = form.SelectedNode.Type switch
        {
            TraceabilityNodeType.Requirement or TraceabilityNodeType.AcceptanceCriterion or TraceabilityNodeType.ExternalReference => SearchObjectType.Requirement,
            TraceabilityNodeType.Blocker => SearchObjectType.Blocker,
            _ => SearchObjectType.Project,
        };
        await NavigateToAsync(form.SelectedNode.ProjectId, type);
    }

    private async Task NavigateToAsync(Guid projectId, SearchObjectType objectType)
    {
        await RefreshProjectsAsync(projectId);
        if (_selected is null) return;
        if (objectType == SearchObjectType.Blocker) BlockersClicked(this, EventArgs.Empty);
        else if (objectType is SearchObjectType.Requirement or SearchObjectType.ExternalReference)
            RequirementsClicked(this, EventArgs.Empty);
        _status.Text = "Suchergebnis geöffnet.";
    }

    private async void FullExportClicked(object? sender, EventArgs e)
    {
        if (_portableExchange is null) { _status.Text = "Vollständiger Export ist nicht verfügbar."; return; }
        using var dialog = new SaveFileDialog { AddExtension = true, DefaultExt = "json",
            Filter = "SASD-PIMS-Austausch (*.json)|*.json|Alle Dateien (*.*)|*.*",
            FileName = "sasd-pims-exchange.json", OverwritePrompt = true, Title = "Vollständigen JSON-Export speichern" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { UseWaitCursor = true; _status.Text = "Export läuft …"; await _portableExchange.ExportJsonAsync(dialog.FileName,
            _version, DateTimeOffset.UtcNow); _status.Text = "Vollständiger JSON-Export erstellt und geprüft."; }
        catch (OperationCanceledException) { _status.Text = "Export abgebrochen."; }
        catch (Exception exception) { _status.Text = $"Export fehlgeschlagen: {exception.Message}"; }
        finally { UseWaitCursor = false; }
    }

    private async void MarkdownExportClicked(object? sender, EventArgs e)
    {
        if (_selected is null) { _status.Text = "Bitte wählen Sie ein Projekt aus."; return; }
        if (_portableExchange is null) { _status.Text = "Markdown-Export ist nicht verfügbar."; return; }
        using var dialog = new SaveFileDialog { AddExtension = true, DefaultExt = "md",
            Filter = "Markdown (*.md)|*.md|Alle Dateien (*.*)|*.*", FileName = $"{_selected.Key}-projektsteckbrief.md",
            OverwritePrompt = true, Title = "Markdown-Projektsteckbrief speichern" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { UseWaitCursor = true; _status.Text = "Projektsteckbrief wird erstellt …";
            await _portableExchange.ExportProjectMarkdownAsync(_selected.Id, dialog.FileName, DateTimeOffset.UtcNow);
            _status.Text = "Markdown-Projektsteckbrief erstellt."; }
        catch (OperationCanceledException) { _status.Text = "Export abgebrochen."; }
        catch (Exception exception) { _status.Text = $"Export fehlgeschlagen: {exception.Message}"; }
        finally { UseWaitCursor = false; }
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

    private void ConfigureFilter(ComboBox combo, string accessibleName, IEnumerable<object> values)
    {
        combo.AccessibleName = accessibleName; combo.Items.Add("Alle");
        foreach (var value in values) combo.Items.Add(value);
        combo.SelectedIndex = 0; combo.SelectedIndexChanged += async (_, _) => await RefreshProjectsAsync();
    }

    private void ConfigureTextFilter(TextBox textBox, string accessibleName)
    { textBox.AccessibleName = accessibleName; textBox.PlaceholderText = accessibleName; textBox.TextChanged += async (_, _) => await RefreshProjectsAsync(); }

    private async void ResetProjectFiltersClicked(object? sender, EventArgs e)
    {
        _search.Clear(); _includeArchived.Checked = false; _needsAttention.Checked = false;
        _phaseFilter.SelectedIndex = 0; _activityFilter.SelectedIndex = 0; _reviewFilter.SelectedIndex = 0;
        _typeFilter.Clear(); _areaFilter.Clear(); _tagFilter.Clear(); await RefreshProjectsAsync();
        _status.Text = "Projektfilter zurückgesetzt.";
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
        using var dialog = new ProjectBlockersForm(_selected, _listBlockers, _addBlocker, _resolveBlocker, _listReferences);
        dialog.ShowDialog(this);
        await RefreshProjectsAsync(_selected.Id);
    }

    private void RequirementsClicked(object? sender, EventArgs e)
    {
        if (_selected is null) { _status.Text = "Bitte wählen Sie ein Projekt aus."; return; }
        using var dialog = new RequirementsForm(_selected, _listRequirements, _createRequirement,
            _updateRequirement, _listReferences, _saveReference, _openReference);
        dialog.ShowDialog(this);
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
        _requirementsButton.Enabled = enabled;
        if (enabled) _archiveButton.Text = _selected!.IsArchived ? "&Reaktivieren" : "&Archivieren";
    }

    private async Task RefreshProjectsAsync(Guid? preferredId = null)
    {
        preferredId ??= (_projects.SelectedItem as ProjectSummaryDto)?.Id;
        var result = await _list.ExecuteAsync(new ProjectCatalogFilter(
            IncludeArchived: _includeArchived.Checked, SearchText: _search.Text,
            ProjectType: _typeFilter.Text, ProjectArea: _areaFilter.Text, Tag: _tagFilter.Text,
            Phase: _phaseFilter.SelectedItem as Sasd.Pims.Domain.Projects.ProjectPhase?,
            ActivityState: _activityFilter.SelectedItem as Sasd.Pims.Domain.Projects.ActivityState?,
            ReviewFreshness: _reviewFilter.SelectedItem as Sasd.Pims.Domain.Projects.ReviewFreshness?,
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
