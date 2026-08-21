using Sasd.Pims.Application.Projects;
using Sasd.Pims.Application.Requirements;

namespace Sasd.Pims.WinForms;

/// <summary>Shows blocker history and supports the explicit add and resolve transitions.</summary>
public sealed class ProjectBlockersForm : Form
{
    private readonly ProjectDto project;
    private readonly ListProjectBlockers list;
    private readonly AddProjectBlocker add;
    private readonly ResolveProjectBlocker resolve;
    private readonly ListBox blockers = new() { Dock = DockStyle.Fill, AccessibleName = "Blockerverlauf" };
    private readonly TextBox blockerDetails = new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, Multiline = true, Height = 75,
        AccessibleName = "Details des ausgewählten Blockers",
    };
    private readonly TextBox summary = new() { AccessibleName = "Blocker-Kurztext" };
    private readonly TextBox details = new() { AccessibleName = "Blocker-Details", Multiline = true, Height = 55 };
    private readonly TextBox cause = new() { AccessibleName = "Blocker-Ursache" };
    private readonly TextBox impact = new() { AccessibleName = "Blocker-Auswirkung" };
    private readonly TextBox affectedObject = new() { AccessibleName = "Betroffenes Objekt" };
    private readonly TextBox nextAction = new() { AccessibleName = "Nächste Maßnahme" };
    private readonly ComboBox externalTask = new() { AccessibleName = "Externe Aufgabenreferenz", DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ListExternalReferences listReferences;
    private readonly Label status = new() { AutoSize = true };

    public ProjectBlockersForm(ProjectDto project, ListProjectBlockers list, AddProjectBlocker add,
        ResolveProjectBlocker resolve, ListExternalReferences listReferences)
    {
        this.project = project; this.list = list; this.add = add; this.resolve = resolve;
        this.listReferences = listReferences;
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
        blockers.SelectedIndexChanged += (_, _) => ShowSelectedDetails();
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12), ColumnCount = 2, RowCount = 11 };
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        grid.Controls.Add(blockers, 0, 0); grid.SetColumnSpan(blockers, 2);
        grid.Controls.Add(blockerDetails, 0, 1); grid.SetColumnSpan(blockerDetails, 2);
        grid.Controls.Add(new Label { Text = "&Kurztext", AutoSize = true }, 0, 2); grid.Controls.Add(summary, 1, 2);
        grid.Controls.Add(new Label { Text = "&Details", AutoSize = true }, 0, 3); grid.Controls.Add(details, 1, 3);
        grid.Controls.Add(new Label { Text = "&Ursache", AutoSize = true }, 0, 4); grid.Controls.Add(cause, 1, 4);
        grid.Controls.Add(new Label { Text = "&Auswirkung", AutoSize = true }, 0, 5); grid.Controls.Add(impact, 1, 5);
        grid.Controls.Add(new Label { Text = "Betroffenes &Objekt", AutoSize = true }, 0, 6); grid.Controls.Add(affectedObject, 1, 6);
        grid.Controls.Add(new Label { Text = "&Nächste Maßnahme", AutoSize = true }, 0, 7); grid.Controls.Add(nextAction, 1, 7);
        grid.Controls.Add(new Label { Text = "E&xterne Aufgabe", AutoSize = true }, 0, 8); grid.Controls.Add(externalTask, 1, 8);
        grid.Controls.Add(status, 0, 9); grid.SetColumnSpan(status, 2);
        var commands = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        commands.Controls.AddRange([close, resolveButton, addButton]);
        grid.Controls.Add(commands, 0, 10); grid.SetColumnSpan(commands, 2);
        Controls.Add(grid);
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        externalTask.Items.Add(new ReferenceChoice(null, "– Keine –"));
        var references = await listReferences.ExecuteAsync(project.Id);
        if (references.Value is not null)
            foreach (var reference in references.Value.Where(item => item.Type == Sasd.Pims.Domain.Requirements.ExternalReferenceType.ExternalTask))
                externalTask.Items.Add(new ReferenceChoice(reference.Id, reference.Title));
        externalTask.SelectedIndex = 0;
        await RefreshAsync();
    }
    private async void AddClicked(object? sender, EventArgs e)
    {
        var referenceId = (externalTask.SelectedItem as ReferenceChoice)?.Id;
        var result = await add.ExecuteAsync(project.Id, summary.Text, details.Text, cause.Text, impact.Text,
            affectedObject.Text, nextAction.Text, referenceId);
        if (result.Status == ProjectOperationStatus.Success)
        {
            summary.Clear(); details.Clear(); cause.Clear(); impact.Clear(); affectedObject.Clear(); nextAction.Clear();
            externalTask.SelectedIndex = 0; await RefreshAsync();
        }
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

    private void ShowSelectedDetails()
    {
        if (blockers.SelectedItem is not BlockerRow row) { blockerDetails.Clear(); return; }
        var value = row.Value;
        blockerDetails.Text = $"Kurztext: {value.Summary}{Environment.NewLine}" +
            $"Details: {value.Details ?? "–"}{Environment.NewLine}" +
            $"Ursache: {value.Cause ?? "–"}{Environment.NewLine}" +
            $"Auswirkung: {value.Impact ?? "Nicht im Altbestand erfasst"}{Environment.NewLine}" +
            $"Betroffenes Objekt: {value.AffectedObject ?? "–"}{Environment.NewLine}" +
            $"Nächste Maßnahme: {value.NextAction ?? (value.IsOpen ? "Nicht im Altbestand erfasst" : "–")}{Environment.NewLine}" +
            $"Externe Aufgabe: {value.ExternalTaskReferenceId?.ToString() ?? "–"}{Environment.NewLine}" +
            $"Status: {(value.IsOpen ? "Offen" : "Gelöst")}{Environment.NewLine}" +
            $"Lösung: {value.ResolutionNote ?? "–"}";
    }
    private sealed record BlockerRow(ProjectBlockerDto Value)
    {
        public override string ToString() => Value.IsOpen ? $"Offen: {Value.Summary}" : $"Gelöst: {Value.Summary}";
    }
    private sealed record ReferenceChoice(Guid? Id, string Title)
    {
        public override string ToString() => Title;
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
