using Sasd.Pims.Application.Projects;
using Sasd.Pims.Domain.Projects;

namespace Sasd.Pims.WinForms;

/// <summary>Edits the controlled phase, activity, target date and review schedule.</summary>
public sealed class ProjectSteeringForm : Form
{
    private readonly ProjectDto project;
    private readonly UpdateProjectSteering update;
    private readonly MarkProjectReviewed markReviewed;
    private readonly ComboBox phase = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox activity = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker target = OptionalDatePicker();
    private readonly DateTimePicker nextReview = OptionalDatePicker();
    private readonly Label validation = new() { AutoSize = true, AccessibleName = "Validierungsfehler" };

    public ProjectSteeringForm(ProjectDto project, UpdateProjectSteering update, MarkProjectReviewed markReviewed)
    {
        this.project = project;
        this.update = update;
        this.markReviewed = markReviewed;
        Text = $"Projektsteuerung – {project.Key}";
        AccessibleName = "Projektsteuerung bearbeiten";
        AutoScaleMode = AutoScaleMode.Dpi;
        MinimumSize = new Size(560, 340);
        StartPosition = FormStartPosition.CenterParent;
        phase.DataSource = Enum.GetValues<ProjectPhase>().Select(value => new Choice<ProjectPhase>(value,
            MainForm.German(value))).ToArray();
        activity.DataSource = Enum.GetValues<ActivityState>().Select(value => new Choice<ActivityState>(value,
            MainForm.German(value))).ToArray();
        phase.SelectedItem = ((Choice<ProjectPhase>[])phase.DataSource).Single(item => item.Value == project.Phase);
        activity.SelectedItem = ((Choice<ActivityState>[])activity.DataSource).Single(item => item.Value == project.ActivityState);
        SetOptionalDate(target, project.TargetDate);
        SetOptionalDate(nextReview, project.NextReviewDueAtUtc is null
            ? null : DateOnly.FromDateTime(project.NextReviewDueAtUtc.Value.LocalDateTime));
        var tips = new ToolTip();
        tips.SetToolTip(phase, "Fachliche Lebenszyklusphase; unabhängig von Aktivität und Archivierung.");
        tips.SetToolTip(activity, "Aktueller Arbeitszustand; ändert Phase und Archivierung nicht automatisch.");
        tips.SetToolTip(target, "Innerhalb von 14 Kalendertagen wird der Termin als bald fällig angezeigt.");
        tips.SetToolTip(nextReview, "Aus diesem Datum und der aktuellen Zeit wird die Review-Aktualität berechnet.");

        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 6 };
        AddRow(grid, 0, "&Phase", phase);
        AddRow(grid, 1, "&Aktivität", activity);
        AddRow(grid, 2, "&Zieltermin", target);
        AddRow(grid, 3, "&Nächstes Review", nextReview);
        grid.Controls.Add(validation, 0, 4); grid.SetColumnSpan(validation, 2);
        var save = new Button { Text = "&Speichern", AutoSize = true, AccessibleName = "Projektsteuerung speichern" };
        save.Click += SaveClicked;
        var review = new Button { Text = "Review &jetzt abschließen", AutoSize = true, AccessibleName = "Review jetzt abschließen" };
        review.Click += ReviewClicked;
        var cancel = new Button { Text = "&Abbrechen", AutoSize = true, DialogResult = DialogResult.Cancel };
        var commands = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
        commands.Controls.AddRange([cancel, save, review]);
        grid.Controls.Add(commands, 0, 5); grid.SetColumnSpan(commands, 2);
        Controls.Add(grid);
        AcceptButton = save;
        CancelButton = cancel;
    }

    private async void SaveClicked(object? sender, EventArgs e)
    {
        var result = await update.ExecuteAsync(new(project.Id, project.Revision,
            ((Choice<ProjectPhase>)phase.SelectedItem!).Value,
            ((Choice<ActivityState>)activity.SelectedItem!).Value,
            SelectedDate(target), ReviewDueUtc()));
        CompleteOrReport(result);
    }

    private async void ReviewClicked(object? sender, EventArgs e)
    {
        var result = await markReviewed.ExecuteAsync(project.Id, project.Revision, ReviewDueUtc());
        CompleteOrReport(result);
    }

    private void CompleteOrReport(ProjectOperationResult<ProjectDto> result)
    {
        if (result.Status == ProjectOperationStatus.Success)
        {
            DialogResult = DialogResult.OK;
            Close();
            return;
        }
        validation.Text = result.Status == ProjectOperationStatus.Conflict
            ? "Das Projekt wurde zwischenzeitlich geändert. Bitte neu öffnen."
            : string.Join(Environment.NewLine, result.Errors.Select(error => error.Message));
    }

    private DateTimeOffset? ReviewDueUtc() => nextReview.Checked
        ? new DateTimeOffset(nextReview.Value.Date, TimeZoneInfo.Local.GetUtcOffset(nextReview.Value.Date)).ToUniversalTime()
        : null;
    private static DateOnly? SelectedDate(DateTimePicker picker) => picker.Checked ? DateOnly.FromDateTime(picker.Value) : null;
    private static DateTimePicker OptionalDatePicker() => new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Width = 150 };
    private static void SetOptionalDate(DateTimePicker picker, DateOnly? value)
    {
        picker.Checked = value is not null;
        if (value is not null) picker.Value = value.Value.ToDateTime(TimeOnly.MinValue);
    }
    private static void AddRow(TableLayoutPanel grid, int row, string text, Control control)
    {
        grid.Controls.Add(new Label { Text = text, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
        control.Dock = DockStyle.Fill; control.AccessibleName = text.Replace("&", string.Empty, StringComparison.Ordinal);
        grid.Controls.Add(control, 1, row);
    }
    private sealed record Choice<T>(T Value, string Text) { public override string ToString() => Text; }
}
