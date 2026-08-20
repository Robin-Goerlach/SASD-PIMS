using Sasd.Pims.Application.Projects;
using Sasd.Pims.Application.Requirements;

namespace Sasd.Pims.WinForms;

/// <summary>Lists and maintains either Project references or references owned by one Requirement.</summary>
public sealed class ExternalReferencesForm : Form
{
    private readonly Guid projectId;
    private readonly Guid? requirementId;
    private readonly ListExternalReferences list;
    private readonly SaveExternalReference save;
    private readonly OpenExternalReference open;
    private readonly ListBox references = new() { Dock = DockStyle.Fill, AccessibleName = "Typisierte Referenzen" };
    private readonly Label status = new() { AutoSize = true };

    public ExternalReferencesForm(Guid projectId, Guid? requirementId, ListExternalReferences list,
        SaveExternalReference save, OpenExternalReference open)
    {
        this.projectId = projectId; this.requirementId = requirementId; this.list = list; this.save = save; this.open = open;
        Text = requirementId is null ? "Projekt-Referenzen" : "Anforderungs-Referenzen";
        AccessibleName = Text; AutoScaleMode = AutoScaleMode.Dpi; MinimumSize = new Size(760, 480);
        StartPosition = FormStartPosition.CenterParent;
        var add = Button("&Neu", "Referenz anlegen", AddClicked);
        var edit = Button("&Bearbeiten", "Ausgewählte Referenz bearbeiten", EditClicked);
        var openButton = Button("Ö&ffnen", "Ausgewählte Referenz sicher öffnen", OpenClicked);
        var close = new Button { Text = "&Schließen", AccessibleName = "Referenzen schließen", AutoSize = true, DialogResult = DialogResult.OK };
        var commands = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true };
        commands.Controls.AddRange([add, edit, openButton, close]);
        Controls.Add(references); Controls.Add(status); Controls.Add(commands); AcceptButton = openButton; CancelButton = close;
    }

    protected override async void OnShown(EventArgs e) { base.OnShown(e); await RefreshAsync(); }
    private async void AddClicked(object? sender, EventArgs e)
    { using var editor = new ExternalReferenceEditorForm(projectId, requirementId, save); if (editor.ShowDialog(this) == DialogResult.OK) await RefreshAsync(); }
    private async void EditClicked(object? sender, EventArgs e)
    { if (Selected is null) return; using var editor = new ExternalReferenceEditorForm(projectId, requirementId, save, Selected); if (editor.ShowDialog(this) == DialogResult.OK) await RefreshAsync(); }
    private async void OpenClicked(object? sender, EventArgs e)
    {
        if (Selected is null) return;
        var result = await open.ExecuteAsync(Selected.Id);
        status.Text = result.Status == ProjectOperationStatus.Success ? "Referenz geöffnet."
            : result.Errors.Count > 0 ? result.Errors[0].Message : "Die Referenz konnte nicht geöffnet werden.";
    }
    private ExternalReferenceDto? Selected => (references.SelectedItem as Row)?.Value;
    private async Task RefreshAsync()
    {
        var result = await list.ExecuteAsync(projectId, requirementId);
        if (result.Value is null) { status.Text = "Referenzen konnten nicht geladen werden."; return; }
        references.DataSource = result.Value.Select(item => new Row(item)).ToList();
        status.Text = result.Value.Count == 0 ? "Keine Referenzen vorhanden." : $"{result.Value.Count} Referenz(en).";
    }
    private static Button Button(string text, string name, EventHandler clicked)
    { var button = new Button { Text = text, AccessibleName = name, AutoSize = true }; button.Click += clicked; return button; }
    private sealed record Row(ExternalReferenceDto Value)
    { public override string ToString() => $"{RequirementLabels.Reference(Value.Type)}: {Value.Title} — {Value.Target}"; }
}
