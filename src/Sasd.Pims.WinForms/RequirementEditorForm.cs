using Sasd.Pims.Application.Projects;
using Sasd.Pims.Application.Requirements;
using Sasd.Pims.Domain.Requirements;
using System.ComponentModel;

namespace Sasd.Pims.WinForms;

/// <summary>Edits Requirement facts and persistently ordered acceptance criteria.</summary>
public sealed class RequirementEditorForm : Form
{
    private readonly Guid projectId;
    private readonly CreateRequirement create;
    private readonly UpdateRequirement update;
    private readonly RequirementDto? existing;
    private readonly TextBox title = new() { AccessibleName = "Anforderungstitel" };
    private readonly TextBox description = new() { Multiline = true, Height = 55, AccessibleName = "Anforderungsbeschreibung" };
    private readonly TextBox rationale = new() { Multiline = true, Height = 45, AccessibleName = "Anforderungsbegründung" };
    private readonly ComboBox priority = new() { DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Anforderungspriorität" };
    private readonly ComboBox decision = new() { DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Entscheidungsstatus" };
    private readonly TextBox decisionReason = new() { AccessibleName = "Entscheidungsbegründung" };
    private readonly ComboBox sourceType = new() { DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Quelltyp" };
    private readonly DateTimePicker sourceDate = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true, AccessibleName = "Quelldatum" };
    private readonly TextBox sourceSummary = new() { AccessibleName = "Quellenkurzbeschreibung" };
    private readonly ComboBox sourceReference = new() { DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Quellenreferenz" };
    private readonly DataGridView criteria = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = true, AccessibleName = "Akzeptanzkriterien" };
    private readonly Label validation = new() { AutoSize = true, AccessibleName = "Validierungsfehler" };
    private readonly BindingList<CriterionRow> criterionRows = [];
    private IReadOnlyList<ExternalReferenceDto> availableReferences = [];

    public RequirementEditorForm(Guid projectId, CreateRequirement create, UpdateRequirement update,
        IReadOnlyList<ExternalReferenceDto> references, RequirementDto? existing = null)
    {
        this.projectId = projectId; this.create = create; this.update = update; this.existing = existing;
        availableReferences = references;
        Text = existing is null ? "Anforderung anlegen" : $"Anforderung bearbeiten — {existing.Key}";
        AccessibleName = Text; AutoScaleMode = AutoScaleMode.Dpi; MinimumSize = new Size(850, 700); StartPosition = FormStartPosition.CenterParent;
        priority.DataSource = Enum.GetValues<RequirementPriority>().Select(value => new EnumOption<RequirementPriority>(value, RequirementLabels.Priority(value))).ToList();
        decision.DataSource = Enum.GetValues<RequirementDecisionStatus>().Select(value => new EnumOption<RequirementDecisionStatus>(value, RequirementLabels.Decision(value))).ToList();
        sourceType.DataSource = Enum.GetValues<RequirementSourceType>().Select(value => new EnumOption<RequirementSourceType>(value, RequirementLabels.Source(value))).ToList();
        priority.DisplayMember = decision.DisplayMember = sourceType.DisplayMember = "Label";
        sourceReference.DataSource = ReferenceOptions(references); sourceReference.DisplayMember = nameof(ReferenceOption.Label);
        criteria.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kriterium", DataPropertyName = nameof(CriterionRow.Text), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        var verification = new DataGridViewComboBoxColumn { HeaderText = "Prüfreferenz", DataPropertyName = nameof(CriterionRow.VerificationReferenceId), DataSource = ReferenceOptions(references), DisplayMember = nameof(ReferenceOption.Label), ValueMember = nameof(ReferenceOption.Id), Width = 220 };
        criteria.Columns.Add(verification);
        criteria.DataSource = criterionRows;
        LoadExisting();
        var saveButton = new Button { Text = "&Speichern", AccessibleName = "Anforderung speichern", AutoSize = true };
        saveButton.Click += SaveClicked;
        var cancel = new Button { Text = "&Abbrechen", AccessibleName = "Bearbeitung abbrechen", AutoSize = true, DialogResult = DialogResult.Cancel };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 12 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Add(layout, 0, "&Titel", title); Add(layout, 1, "&Beschreibung", description); Add(layout, 2, "Be&gründung", rationale);
        Add(layout, 3, "&Priorität", priority); Add(layout, 4, "&Entscheidung", decision); Add(layout, 5, "Entscheidungs&grund", decisionReason);
        Add(layout, 6, "&Quelltyp", sourceType); Add(layout, 7, "Quelld&atum", sourceDate); Add(layout, 8, "Quellen&hinweis", sourceSummary);
        Add(layout, 9, "Quellen&referenz", sourceReference);
        layout.Controls.Add(new Label { Text = "&Akzeptanzkriterien", AutoSize = true }, 0, 10); layout.Controls.Add(criteria, 1, 10);
        var commands = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        commands.Controls.AddRange([cancel, saveButton, validation]); layout.Controls.Add(commands, 0, 11); layout.SetColumnSpan(commands, 2);
        Controls.Add(layout); AcceptButton = saveButton; CancelButton = cancel;
    }

    private void LoadExisting()
    {
        var none = ((IEnumerable<ReferenceOption>)sourceReference.DataSource!).First();
        sourceReference.SelectedItem = none;
        if (existing is null) return;
        title.Text = existing.Title; description.Text = existing.Description; rationale.Text = existing.Rationale;
        priority.SelectedItem = ((IEnumerable<EnumOption<RequirementPriority>>)priority.DataSource!).Single(item => item.Value == existing.Priority);
        decision.SelectedItem = ((IEnumerable<EnumOption<RequirementDecisionStatus>>)decision.DataSource!).Single(item => item.Value == existing.DecisionStatus);
        decisionReason.Text = existing.DecisionReason;
        sourceType.SelectedItem = ((IEnumerable<EnumOption<RequirementSourceType>>)sourceType.DataSource!).Single(item => item.Value == existing.SourceType);
        if (existing.SourceDate is { } date) { sourceDate.Checked = true; sourceDate.Value = date.ToDateTime(TimeOnly.MinValue); }
        else sourceDate.Checked = false;
        sourceSummary.Text = existing.SourceSummary;
        sourceReference.SelectedItem = ((IEnumerable<ReferenceOption>)sourceReference.DataSource!).FirstOrDefault(item => item.Id == existing.SourceReferenceId) ?? none;
        foreach (var item in existing.AcceptanceCriteria)
            criterionRows.Add(new CriterionRow(item.Id, item.Text, item.VerificationReferenceId));
    }

    private async void SaveClicked(object? sender, EventArgs e)
    {
        criteria.EndEdit();
        var rows = criterionRows.ToArray();
        var command = new SaveRequirementCommand(title.Text, description.Text, rationale.Text,
            ((EnumOption<RequirementPriority>)priority.SelectedItem!).Value,
            ((EnumOption<RequirementDecisionStatus>)decision.SelectedItem!).Value,
            decisionReason.Text, ((EnumOption<RequirementSourceType>)sourceType.SelectedItem!).Value,
            sourceDate.Checked ? DateOnly.FromDateTime(sourceDate.Value) : null, sourceSummary.Text,
            (sourceReference.SelectedItem as ReferenceOption)?.Id,
            rows.Select(item => new SaveAcceptanceCriterionCommand(item.Id, item.Text, item.VerificationReferenceId)).ToArray());
        var result = existing is null ? await create.ExecuteAsync(projectId, command)
            : await update.ExecuteAsync(existing.Id, existing.Revision, command);
        if (result.Status == ProjectOperationStatus.Success) { DialogResult = DialogResult.OK; Close(); return; }
        validation.Text = result.Errors.Count > 0 ? result.Errors[0].Message : "Die Anforderung konnte nicht gespeichert werden.";
    }

    private static List<ReferenceOption> ReferenceOptions(IEnumerable<ExternalReferenceDto> references) =>
        [new(null, "— keine —"), .. references.Select(item => new ReferenceOption(item.Id, $"{RequirementLabels.Reference(item.Type)}: {item.Title}"))];
    private static void Add(TableLayoutPanel panel, int row, string label, Control control)
    { control.Dock = DockStyle.Fill; panel.Controls.Add(new Label { Text = label, AutoSize = true }, 0, row); panel.Controls.Add(control, 1, row); }
    private sealed record ReferenceOption(Guid? Id, string Label);
    private sealed class CriterionRow(Guid? id, string text, Guid? verificationReferenceId)
    {
        public Guid? Id { get; set; } = id;
        public string Text { get; set; } = text;
        public Guid? VerificationReferenceId { get; set; } = verificationReferenceId;
    }
}
