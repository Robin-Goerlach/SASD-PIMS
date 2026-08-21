using Sasd.Pims.Application.Auditing;
using System.Globalization;

namespace Sasd.Pims.WinForms;

/// <summary>Shows a Project's redacted append-only business history as a read-only native view.</summary>
public sealed class ProjectHistoryForm : Form
{
    private readonly Guid projectId;
    private readonly IChangeEventReader reader;
    private readonly DataGridView history = new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
        AllowUserToDeleteRows = false, AutoGenerateColumns = false, AccessibleName = "Änderungsverlauf" };

    public ProjectHistoryForm(Guid projectId, string projectKey, IChangeEventReader reader)
    {
        this.projectId = projectId; this.reader = reader;
        Text = $"Änderungsverlauf – {projectKey}"; AccessibleName = "Projektbezogener Änderungsverlauf";
        AutoScaleMode = AutoScaleMode.Dpi; MinimumSize = new Size(900, 450); StartPosition = FormStartPosition.CenterParent;
        history.Columns.AddRange(
            Column("Zeitpunkt", "Occurred"), Column("Objektart", "EntityType"), Column("Objekt", "Identifier"),
            Column("Ereignis", "EventType"), Column("Alter Wert (redigiert)", "OldValue"),
            Column("Neuer Wert (redigiert)", "NewValue"));
        var close = new Button { Text = "&Schließen", AutoSize = true, DialogResult = DialogResult.OK,
            AccessibleName = "Änderungsverlauf schließen" };
        var commands = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
        commands.Controls.Add(close); Controls.Add(history); Controls.Add(commands); CancelButton = close;
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        history.DataSource = (await reader.ListByProjectAsync(projectId)).Select(item => new
        {
            Occurred = item.OccurredAtUtc.ToLocalTime().ToString("g", CultureInfo.CurrentCulture),
            item.EntityType, Identifier = item.ObjectIdentifier, item.EventType,
            OldValue = item.OldValue ?? "–", NewValue = item.NewValue ?? "–",
        }).ToArray();
    }

    private static DataGridViewTextBoxColumn Column(string title, string property) => new()
    { HeaderText = title, DataPropertyName = property, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill };
}
