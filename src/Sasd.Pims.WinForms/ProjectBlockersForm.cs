using Sasd.Pims.Application.Projects;

namespace Sasd.Pims.WinForms;

/// <summary>Shows blocker history and supports the explicit add and resolve transitions.</summary>
public sealed class ProjectBlockersForm : Form
{
    private readonly ProjectDto project;
    private readonly ListProjectBlockers list;
    private readonly AddProjectBlocker add;
    private readonly ResolveProjectBlocker resolve;
    private readonly ListBox blockers = new() { Dock = DockStyle.Fill, AccessibleName = "Blockerverlauf" };
    private readonly TextBox summary = new() { AccessibleName = "Blocker-Kurztext" };
    private readonly TextBox details = new() { AccessibleName = "Blocker-Details", Multiline = true, Height = 55 };
    private readonly Label status = new() { AutoSize = true };

    public ProjectBlockersForm(ProjectDto project, ListProjectBlockers list, AddProjectBlocker add,
        ResolveProjectBlocker resolve)
    {
        this.project = project; this.list = list; this.add = add; this.resolve = resolve;
        Text = $"Blocker – {project.Key}";
        AccessibleName = "Blocker bearbeiten";
        AutoScaleMode = AutoScaleMode.Dpi;
        MinimumSize = new Size(700, 500);
        StartPosition = FormStartPosition.CenterParent;
        var addButton = new Button { Text = "&Blocker hinzufügen", AutoSize = true, AccessibleName = "Blocker hinzufügen" };
        addButton.Click += AddClicked;
        var resolveButton = new Button { Text = "&Ausgewählten lösen", AutoSize = true, AccessibleName = "Ausgewählten Blocker lösen" };
        resolveButton.Click += ResolveClicked;
        var close = new Button { Text = "&Schließen", AutoSize = true, DialogResult = DialogResult.OK };
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 5 };
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        grid.Controls.Add(blockers, 0, 0); grid.SetColumnSpan(blockers, 2);
        grid.Controls.Add(new Label { Text = "&Kurztext", AutoSize = true }, 0, 1); grid.Controls.Add(summary, 1, 1);
        grid.Controls.Add(new Label { Text = "&Details", AutoSize = true }, 0, 2); grid.Controls.Add(details, 1, 2);
        grid.Controls.Add(status, 0, 3); grid.SetColumnSpan(status, 2);
        var commands = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        commands.Controls.AddRange([close, resolveButton, addButton]);
        grid.Controls.Add(commands, 0, 4); grid.SetColumnSpan(commands, 2);
        Controls.Add(grid);
    }

    protected override async void OnShown(EventArgs e) { base.OnShown(e); await RefreshAsync(); }
    private async void AddClicked(object? sender, EventArgs e)
    {
        var result = await add.ExecuteAsync(project.Id, summary.Text, details.Text);
        if (result.Status == ProjectOperationStatus.Success) { summary.Clear(); details.Clear(); await RefreshAsync(); }
        else status.Text = result.Errors.Count > 0 ? result.Errors[0].Message : "Blocker konnte nicht gespeichert werden.";
    }
    private async void ResolveClicked(object? sender, EventArgs e)
    {
        if (blockers.SelectedItem is not BlockerRow row || !row.Value.IsOpen) return;
        using var prompt = new ResolutionNoteForm();
        if (prompt.ShowDialog(this) != DialogResult.OK) return;
        var result = await resolve.ExecuteAsync(row.Value.Id, prompt.Note);
        status.Text = result.Status == ProjectOperationStatus.Success ? "Blocker gelöst." : "Blocker konnte nicht gelöst werden.";
        await RefreshAsync();
    }
    private async Task RefreshAsync()
    {
        var result = await list.ExecuteAsync(project.Id);
        if (result.Value is null) { status.Text = "Blocker konnten nicht geladen werden."; return; }
        blockers.DataSource = result.Value.Select(value => new BlockerRow(value)).ToList();
        status.Text = $"{result.Value.Count(value => value.IsOpen)} offene(r) Blocker.";
    }
    private sealed record BlockerRow(ProjectBlockerDto Value)
    {
        public override string ToString() => Value.IsOpen ? $"Offen: {Value.Summary}" : $"Gelöst: {Value.Summary}";
    }
    private sealed class ResolutionNoteForm : Form
    {
        private readonly TextBox note = new() { Dock = DockStyle.Fill, Multiline = true, AccessibleName = "Lösungshinweis" };
        public string Note => note.Text;
        public ResolutionNoteForm()
        {
            Text = "Blocker lösen"; MinimumSize = new Size(450, 250); AutoScaleMode = AutoScaleMode.Dpi;
            var ok = new Button { Text = "&Lösen", DialogResult = DialogResult.OK, AutoSize = true };
            var cancel = new Button { Text = "&Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };
            var commands = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
            commands.Controls.AddRange([cancel, ok]); Controls.Add(note); Controls.Add(commands); AcceptButton = ok; CancelButton = cancel;
        }
    }
}
