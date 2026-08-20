namespace Sasd.Pims.WinForms;

/// <summary>Provides local, offline help for the controlled 0.2 steering vocabulary.</summary>
public sealed class HelpForm : Form
{
    public HelpForm()
    {
        Text = "Hilfe und Glossar";
        AccessibleName = "Hilfe und Glossar";
        AutoScaleMode = AutoScaleMode.Dpi;
        MinimumSize = new Size(700, 520);
        StartPosition = FormStartPosition.CenterParent;
        Controls.Add(new TextBox
        {
            Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
            AccessibleName = "Lokale Hilfe", Text = """
                Projektphase beschreibt, wo ein Projekt im fachlichen Lebenszyklus steht: Idee, Vorbereitung,
                Umsetzung, Validierung oder Abschluss.

                Aktivität beschreibt unabhängig davon, ob gerade gearbeitet wird: Nicht begonnen, Aktiv,
                Pausiert, Abgeschlossen oder Abgebrochen. Archivierung ist ebenfalls unabhängig und löscht nichts.

                Review-Aktualität wird aus dem nächsten Review-Termin und dem heutigen Datum berechnet.
                Zieltermine gelten innerhalb von 14 Kalendertagen als bald fällig. Abgeschlossene oder
                abgebrochene Projekte erhalten keine Terminwarnung.

                Handlungsbedarf besteht bei überfälligem Review, offenem Blocker oder überfälligem Zieltermin.
                Blocker sind konkrete Hindernisse, keine Aufgaben. Gelöste Blocker bleiben im Verlauf erhalten.

                F1 öffnet diese Hilfe. Alle Daten bleiben lokal in der PIMS-Datenbank.
                """,
        });
    }
}
