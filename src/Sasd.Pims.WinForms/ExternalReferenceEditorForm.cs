using Sasd.Pims.Application.Projects;
using Sasd.Pims.Application.Requirements;
using Sasd.Pims.Domain.Requirements;

namespace Sasd.Pims.WinForms;

/// <summary>Edits one typed reference without probing or copying its target.</summary>
public sealed class ExternalReferenceEditorForm : Form
{
    private readonly SaveExternalReference save;
    private readonly Guid projectId;
    private readonly Guid? requirementId;
    private readonly ExternalReferenceDto? existing;
    private readonly ComboBox type = new() { DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Referenztyp" };
    private readonly TextBox title = new() { AccessibleName = "Referenztitel" };
    private readonly TextBox target = new() { AccessibleName = "Referenzziel" };
    private readonly Label validation = new() { AutoSize = true, AccessibleName = "Validierungsfehler" };

    public ExternalReferenceEditorForm(Guid projectId, Guid? requirementId, SaveExternalReference save,
        ExternalReferenceDto? existing = null)
    {
        this.projectId = projectId; this.requirementId = requirementId; this.save = save; this.existing = existing;
        Text = existing is null ? "Referenz anlegen" : "Referenz bearbeiten";
        AccessibleName = Text; AutoScaleMode = AutoScaleMode.Dpi; MinimumSize = new Size(650, 300);
        StartPosition = FormStartPosition.CenterParent;
        type.DataSource = Enum.GetValues<ExternalReferenceType>().Select(value => new EnumOption<ExternalReferenceType>(value, RequirementLabels.Reference(value))).ToList();
        type.DisplayMember = nameof(EnumOption<ExternalReferenceType>.Label);
        if (existing is not null) { type.SelectedItem = ((IEnumerable<EnumOption<ExternalReferenceType>>)type.DataSource).Single(item => item.Value == existing.Type); title.Text = existing.Title; target.Text = existing.Target; }
        var saveButton = new Button { Text = "&Speichern", AccessibleName = "Referenz speichern", AutoSize = true };
        saveButton.Click += SaveClicked;
        var cancel = new Button { Text = "&Abbrechen", AccessibleName = "Bearbeitung abbrechen", AutoSize = true, DialogResult = DialogResult.Cancel };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 5 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddRow(layout, 0, "&Typ", type); AddRow(layout, 1, "&Titel", title); AddRow(layout, 2, "&Ziel", target);
        layout.Controls.Add(validation, 0, 3); layout.SetColumnSpan(validation, 2);
        var commands = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        commands.Controls.AddRange([cancel, saveButton]); layout.Controls.Add(commands, 0, 4); layout.SetColumnSpan(commands, 2);
        Controls.Add(layout); AcceptButton = saveButton; CancelButton = cancel;
    }

    private async void SaveClicked(object? sender, EventArgs e)
    {
        var command = new SaveExternalReferenceCommand(((EnumOption<ExternalReferenceType>)type.SelectedItem!).Value, title.Text, target.Text);
        var result = existing is null
            ? await save.CreateAsync(projectId, requirementId, command)
            : await save.UpdateAsync(existing.Id, existing.Revision, command);
        if (result.Status == ProjectOperationStatus.Success) { DialogResult = DialogResult.OK; Close(); return; }
        validation.Text = result.Errors.Count > 0 ? result.Errors[0].Message : "Die Referenz konnte nicht gespeichert werden.";
    }

    private static void AddRow(TableLayoutPanel layout, int row, string label, Control control)
    { control.Dock = DockStyle.Fill; layout.Controls.Add(new Label { Text = label, AutoSize = true }, 0, row); layout.Controls.Add(control, 1, row); }
}
