using Sasd.Pims.Application.Diagnostics;

namespace Sasd.Pims.WinForms;

/// <summary>Explains fixed local storage locations without offering path mutation.</summary>
public sealed class OperatingInformationForm : Form
{
    public OperatingInformationForm(OperatingPathsInfo paths)
    {
        Text = "Betriebsinformationen und Speicherorte"; AccessibleName = Text;
        AutoScaleMode = AutoScaleMode.Dpi; MinimumSize = new Size(760, 430); StartPosition = FormStartPosition.CenterParent;
        var text = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
            AccessibleName = "Lokale Betriebs- und Datenpfade", Text = $"""
            SQLite-Datenbankdatei:
            {paths.DatabaseFile}

            Log-Verzeichnis:
            {paths.LogDirectory}

            Anwendungsverzeichnis:
            {paths.ApplicationDirectory}

            Recovery-/Rückrollsicherungen:
            {paths.RecoveryDirectory}

            Backup-Ziele: {OperatingPathsInfo.UserSelectedBackupDestination}
            Export-Ziele: {OperatingPathsInfo.UserSelectedExportDestination}

            Diese Ansicht ist schreibgeschützt. Pfade können hier nicht geändert werden.
            """ };
        var close = new Button { Text = "&Schließen", AutoSize = true, DialogResult = DialogResult.OK,
            AccessibleName = "Betriebsinformationen schließen" };
        var commands = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
        commands.Controls.Add(close); Controls.Add(text); Controls.Add(commands); CancelButton = close;
    }
}
