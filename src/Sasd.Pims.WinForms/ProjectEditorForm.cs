using Sasd.Pims.Application.Projects;

namespace Sasd.Pims.WinForms;

/// <summary>Edits project master data through Application use cases.</summary>
public sealed class ProjectEditorForm : Form
{
    private readonly CreateProject? _create;
    private readonly UpdateProject? _update;
    private readonly ProjectDto? _project;
    private readonly TextBox[] _inputs = Enumerable.Range(0, 9).Select(_ => new TextBox()).ToArray();
    private readonly Label _validation = new();
    private readonly Button _save = new();

    public ProjectEditorForm(CreateProject createProject)
    {
        _create = createProject;
        InitializeControls("Neues Projekt");
    }

    public ProjectEditorForm(ProjectDto project, UpdateProject updateProject)
    {
        _project = project;
        _update = updateProject;
        InitializeControls($"Projekt bearbeiten – {project.Key}");
        Populate(project);
        _inputs[0].ReadOnly = true;
    }

    private void InitializeControls(string title)
    {
        Text = title;
        Name = nameof(ProjectEditorForm);
        AccessibleName = title;
        AutoScaleMode = AutoScaleMode.Dpi;
        MinimumSize = new Size(650, 650);
        StartPosition = FormStartPosition.CenterParent;
        var labels = new[] { "Projekt&kennung", "&Name", "&Kurzbeschreibung", "&Ziel", "N&utzen",
            "Projekt&art-Code", "Projekt&bereich-Code", "&Verantwortlichkeit", "&Tags (kommagetrennt)" };
        var accessibleNames = new[] { "Projektkennung", "Projektname", "Kurzbeschreibung", "Ziel", "Nutzen",
            "Projektart-Code", "Projektbereich-Code", "Verantwortlichkeit", "Tags" };
        var fields = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(12), ColumnCount = 2, RowCount = 11 };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var index = 0; index < _inputs.Length; index++)
        {
            var input = _inputs[index];
            input.Dock = DockStyle.Fill;
            input.TabIndex = index;
            input.AccessibleName = accessibleNames[index];
            if (index is 2 or 3 or 4) { input.Multiline = true; input.Height = 60; input.ScrollBars = ScrollBars.Vertical; }
            fields.Controls.Add(new Label { Text = labels[index], AutoSize = true, Anchor = AnchorStyles.Left }, 0, index);
            fields.Controls.Add(input, 1, index);
        }

        _validation.AutoSize = true;
        _validation.ForeColor = SystemColors.ControlText;
        _validation.AccessibleName = "Validierungsfehler";
        _validation.TabIndex = 9;
        fields.Controls.Add(_validation, 0, 9);
        fields.SetColumnSpan(_validation, 2);
        _save.Text = "&Speichern";
        _save.AutoSize = true;
        _save.TabIndex = 10;
        _save.AccessibleName = "Projekt speichern";
        _save.Click += SaveClicked;
        var cancel = new Button { Text = "&Abbrechen", AutoSize = true, DialogResult = DialogResult.Cancel,
            TabIndex = 11, AccessibleName = "Bearbeitung abbrechen" };
        var commands = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        commands.Controls.Add(cancel);
        commands.Controls.Add(_save);
        fields.Controls.Add(commands, 0, 10);
        fields.SetColumnSpan(commands, 2);
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        scroll.Controls.Add(fields);
        Controls.Add(scroll);
        AcceptButton = _save;
        CancelButton = cancel;
    }

    private void Populate(ProjectDto value)
    {
        var values = new[] { value.Key, value.Name, value.ShortDescription, value.Goal, value.Benefit,
            value.ProjectType, value.ProjectArea, value.Responsibility, string.Join(", ", value.Tags) };
        for (var index = 0; index < values.Length; index++) _inputs[index].Text = values[index];
    }

    private async void SaveClicked(object? sender, EventArgs e)
    {
        _save.Enabled = false;
        try
        {
            var tags = _inputs[8].Text.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var result = _project is null
                ? await _create!.ExecuteAsync(new(_inputs[0].Text, _inputs[1].Text, _inputs[2].Text,
                    _inputs[3].Text, _inputs[4].Text, _inputs[5].Text, _inputs[6].Text, _inputs[7].Text, tags))
                : await _update!.ExecuteAsync(new(_project.Id, _project.Revision, _inputs[1].Text,
                    _inputs[2].Text, _inputs[3].Text, _inputs[4].Text, _inputs[5].Text,
                    _inputs[6].Text, _inputs[7].Text, tags));
            if (result.Status == ProjectOperationStatus.Success)
            {
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            _validation.Text = ToGermanError(result);
            _validation.Focus();
        }
        finally { _save.Enabled = true; }
    }

    private static string ToGermanError(ProjectOperationResult<ProjectDto> result)
    {
        if (result.Status == ProjectOperationStatus.InfrastructureFailure)
            return $"Das Projekt konnte nicht gespeichert werden. Fehler-ID: {result.ErrorId}";
        if (result.Status == ProjectOperationStatus.Conflict)
            return "Das Projekt wurde zwischenzeitlich geändert. Bitte neu öffnen und erneut bearbeiten.";
        return string.Join(Environment.NewLine, result.Errors.Select(error => error.Code switch
        {
            "ProjectKeyInvalid" => "• Die Projektkennung muss aus 1–64 Buchstaben, Ziffern oder einzelnen Bindestrichen bestehen.",
            "ProjectKeyDuplicate" => "• Diese Projektkennung ist bereits vorhanden.",
            "ProjectNameRequired" => "• Der Projektname ist erforderlich.",
            "ProjectClassificationInvalid" => "• Art und Bereich dürfen nur Buchstaben, Ziffern und einzelne Bindestriche enthalten.",
            "ProjectTagInvalid" => "• Tags müssen 1–32 Zeichen enthalten.",
            "ProjectTagsTooMany" => "• Es sind höchstens 20 Tags zulässig.",
            _ => $"• {error.Message}",
        }));
    }
}
