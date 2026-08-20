using Sasd.Pims.Application.Projects;
using Sasd.Pims.Application.Requirements;

namespace Sasd.Pims.WinForms;

/// <summary>Provides the project-context Requirement list and its obvious primary actions.</summary>
public sealed class RequirementsForm : Form
{
    private readonly ProjectDto project;
    private readonly ListRequirements listRequirements;
    private readonly CreateRequirement createRequirement;
    private readonly UpdateRequirement updateRequirement;
    private readonly ListExternalReferences listReferences;
    private readonly SaveExternalReference saveReference;
    private readonly OpenExternalReference openReference;
    private readonly ListBox requirements = new() { Dock = DockStyle.Fill, AccessibleName = "Anforderungen des Projekts" };
    private readonly TextBox details = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, AccessibleName = "Anforderungsdetails" };
    private readonly Label status = new() { AutoSize = true };

    public RequirementsForm(ProjectDto project, ListRequirements listRequirements, CreateRequirement createRequirement,
        UpdateRequirement updateRequirement, ListExternalReferences listReferences,
        SaveExternalReference saveReference, OpenExternalReference openReference)
    {
        this.project = project; this.listRequirements = listRequirements; this.createRequirement = createRequirement;
        this.updateRequirement = updateRequirement; this.listReferences = listReferences;
        this.saveReference = saveReference; this.openReference = openReference;
        Text = $"Anforderungen — {project.Key}"; AccessibleName = "Anforderungen bearbeiten";
        AutoScaleMode = AutoScaleMode.Dpi; MinimumSize = new Size(900, 600); StartPosition = FormStartPosition.CenterParent;
        requirements.SelectedIndexChanged += (_, _) => ShowDetails();
        var add = Button("&Neu", "Anforderung anlegen", AddClicked);
        var edit = Button("&Bearbeiten", "Ausgewählte Anforderung bearbeiten", EditClicked);
        var projectReferences = Button("Projekt-&Referenzen", "Referenzen des Projekts bearbeiten", ProjectReferencesClicked);
        var requirementReferences = Button("Anforderungs-Re&ferenzen", "Referenzen der ausgewählten Anforderung bearbeiten", RequirementReferencesClicked);
        var close = new Button { Text = "&Schließen", AccessibleName = "Anforderungen schließen", AutoSize = true, DialogResult = DialogResult.OK };
        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 340 };
        split.Panel1.Controls.Add(requirements); split.Panel2.Controls.Add(details);
        var commands = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true };
        commands.Controls.AddRange([add, edit, projectReferences, requirementReferences, close]);
        Controls.Add(split); Controls.Add(status); Controls.Add(commands); CancelButton = close;
    }

    protected override async void OnShown(EventArgs e) { base.OnShown(e); await RefreshAsync(); }
    private async void AddClicked(object? sender, EventArgs e)
    {
        var references = await LoadProjectReferencesAsync();
        using var editor = new RequirementEditorForm(project.Id, createRequirement, updateRequirement, references);
        if (editor.ShowDialog(this) == DialogResult.OK) await RefreshAsync();
    }
    private async void EditClicked(object? sender, EventArgs e)
    {
        if (Selected is null) return;
        var references = await LoadProjectReferencesAsync();
        using var editor = new RequirementEditorForm(project.Id, createRequirement, updateRequirement, references, Selected);
        if (editor.ShowDialog(this) == DialogResult.OK) await RefreshAsync(Selected.Id);
    }
    private void ProjectReferencesClicked(object? sender, EventArgs e)
    { using var form = new ExternalReferencesForm(project.Id, null, listReferences, saveReference, openReference); form.ShowDialog(this); }
    private void RequirementReferencesClicked(object? sender, EventArgs e)
    { if (Selected is null) return; using var form = new ExternalReferencesForm(project.Id, Selected.Id, listReferences, saveReference, openReference); form.ShowDialog(this); }
    private RequirementDto? Selected => (requirements.SelectedItem as Row)?.Value;
    private async Task<IReadOnlyList<ExternalReferenceDto>> LoadProjectReferencesAsync() =>
        (await listReferences.ExecuteAsync(project.Id)).Value ?? [];
    private async Task RefreshAsync(Guid? preferredId = null)
    {
        var result = await listRequirements.ExecuteAsync(project.Id);
        if (result.Value is null) { status.Text = "Anforderungen konnten nicht geladen werden."; return; }
        var rows = result.Value.Select(item => new Row(item)).ToList(); requirements.DataSource = rows;
        requirements.SelectedItem = preferredId is null ? null : rows.FirstOrDefault(item => item.Value.Id == preferredId);
        status.Text = rows.Count == 0 ? "Keine Anforderungen vorhanden." : $"{rows.Count} Anforderung(en).";
    }
    private void ShowDetails()
    {
        if (Selected is null) { details.Text = "Wählen Sie eine Anforderung aus."; return; }
        details.Text = $"{project.Key} / {Selected.Key} — {Selected.Title}{Environment.NewLine}{Environment.NewLine}" +
            $"Priorität: {RequirementLabels.Priority(Selected.Priority)}{Environment.NewLine}Entscheidung: {RequirementLabels.Decision(Selected.DecisionStatus)}{Environment.NewLine}" +
            $"Entscheidungsgrund: {Selected.DecisionReason ?? "—"}{Environment.NewLine}Quelle: {RequirementLabels.Source(Selected.SourceType)}{Environment.NewLine}" +
            $"Quellenhinweis: {Selected.SourceSummary ?? "—"}{Environment.NewLine}{Environment.NewLine}" +
            "Akzeptanzkriterien:" + Environment.NewLine + (Selected.AcceptanceCriteria.Count == 0 ? "—" :
                string.Join(Environment.NewLine, Selected.AcceptanceCriteria.Select(item => $"{item.Sequence}. {item.Text}")));
    }
    private static Button Button(string text, string name, EventHandler clicked)
    { var button = new Button { Text = text, AccessibleName = name, AutoSize = true }; button.Click += clicked; return button; }
    private sealed record Row(RequirementDto Value)
    { public override string ToString() => $"{Value.Key} — {Value.Title} [{RequirementLabels.Priority(Value.Priority)}, {RequirementLabels.Decision(Value.DecisionStatus)}]"; }
}
