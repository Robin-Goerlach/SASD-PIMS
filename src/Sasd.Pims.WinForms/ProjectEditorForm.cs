using Sasd.Pims.Application.Projects;

namespace Sasd.Pims.WinForms;

public sealed class ProjectEditorForm : Form
{
    private readonly CreateProject? _createProject;
    private readonly TextBox _keyTextBox = new();
    private readonly TextBox _nameTextBox = new();
    private readonly TextBox _descriptionTextBox = new();
    private readonly Label _validationSummary = new();
    private readonly Button _saveButton = new();

    public ProjectEditorForm(CreateProject createProject)
    {
        _createProject = createProject;
        InitializeControls(readOnly: false);
    }

    public ProjectEditorForm(ProjectDto project)
    {
        InitializeControls(readOnly: true);
        _keyTextBox.Text = project.Key;
        _nameTextBox.Text = project.Name;
        _descriptionTextBox.Text = project.ShortDescription;
    }

    private void InitializeControls(bool readOnly)
    {
        Text = readOnly ? "Project" : "New project";
        Name = nameof(ProjectEditorForm);
        AccessibleName = Text;
        AutoScaleMode = AutoScaleMode.Dpi;
        MinimumSize = new Size(520, 420);
        StartPosition = FormStartPosition.CenterParent;

        ConfigureTextBox(_keyTextBox, 0, "Project key", readOnly);
        ConfigureTextBox(_nameTextBox, 1, "Project name", readOnly);
        ConfigureTextBox(_descriptionTextBox, 2, "Short description", readOnly);
        _descriptionTextBox.Multiline = true;
        _descriptionTextBox.ScrollBars = ScrollBars.Vertical;

        _validationSummary.AutoSize = true;
        _validationSummary.ForeColor = SystemColors.ControlText;
        _validationSummary.AccessibleName = "Validation summary";
        _validationSummary.TabIndex = 3;

        _saveButton.Text = "&Save";
        _saveButton.AutoSize = true;
        _saveButton.TabIndex = 4;
        _saveButton.Visible = !readOnly;
        _saveButton.Click += SaveClicked;

        var cancelButton = new Button
        {
            Text = readOnly ? "&Close" : "&Cancel",
            AutoSize = true,
            DialogResult = DialogResult.Cancel,
            TabIndex = 5,
        };

        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 5,
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        fields.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        AddField(fields, "Project &key", _keyTextBox, 0);
        AddField(fields, "&Name", _nameTextBox, 1);
        AddField(fields, "&Short description", _descriptionTextBox, 2);
        fields.Controls.Add(_validationSummary, 0, 3);
        fields.SetColumnSpan(_validationSummary, 2);

        var commands = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
        };
        commands.Controls.Add(cancelButton);
        commands.Controls.Add(_saveButton);
        fields.Controls.Add(commands, 0, 4);
        fields.SetColumnSpan(commands, 2);

        Controls.Add(fields);
        AcceptButton = readOnly ? cancelButton : _saveButton;
        CancelButton = cancelButton;
    }

    private static void ConfigureTextBox(TextBox textBox, int tabIndex, string accessibleName, bool readOnly)
    {
        textBox.Dock = DockStyle.Fill;
        textBox.TabIndex = tabIndex;
        textBox.AccessibleName = accessibleName;
        textBox.ReadOnly = readOnly;
    }

    private static void AddField(TableLayoutPanel fields, string labelText, Control input, int row)
    {
        var label = new Label
        {
            Text = labelText,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
        };
        fields.Controls.Add(label, 0, row);
        fields.Controls.Add(input, 1, row);
    }

    private async void SaveClicked(object? sender, EventArgs e)
    {
        if (_createProject is null)
        {
            return;
        }

        _saveButton.Enabled = false;
        try
        {
            var result = await _createProject.ExecuteAsync(
                new(_keyTextBox.Text, _nameTextBox.Text, _descriptionTextBox.Text));
            if (result.Status == ProjectOperationStatus.Success)
            {
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            _validationSummary.Text = string.Join(Environment.NewLine, result.Errors.Select(error => $"• {error.Message}"));
            _validationSummary.Focus();
        }
        finally
        {
            _saveButton.Enabled = true;
        }
    }
}
