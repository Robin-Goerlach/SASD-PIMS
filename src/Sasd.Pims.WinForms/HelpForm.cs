namespace Sasd.Pims.WinForms;

/// <summary>Provides local, offline help for project steering, Requirements and typed references.</summary>
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

                Anforderungen gehören immer genau zu einem Projekt. Ihre Priorität (Muss, Soll, Kann) ist
                unabhängig vom Entscheidungsstatus. Zurückgestellte und abgelehnte Anforderungen bleiben mit
                Begründung erhalten. Muss-Anforderungen benötigen mindestens ein Akzeptanzkriterium.

                Referenzen sind typisierte Verweise; PIMS kopiert weder Dokumente noch Chats, Repositories oder
                Aufgaben. ExternalTask bezeichnet ausdrücklich eine Aufgabe in einem externen System. Ziele werden
                vor dem Speichern und Öffnen zentral geprüft. Fehlende lokale Ziele werden gemeldet, aber niemals
                automatisch gelöscht. PIMS ruft HTTPS-Ziele nicht vorab ab und speichert keine Zugangsdaten.

                Globale Suche (Strg+F) durchsucht Projekte, Anforderungen, Blockaden und externe Referenzen.
                Objekttyp und Projekt sind kombinierbar; ein Treffer öffnet den zugehörigen Arbeitsbereich.
                Traceability stellt ausschließlich vorhandene Projekt-, Anforderungs-, Akzeptanzkriterien-,
                Quellen-, Nachweis-, Referenz- und Blockadenbeziehungen hierarchisch dar.

                Der vollständige JSON-Export verwendet sasd-pims-exchange 1.0. Absolute lokale Pfade bleiben
                unverändert und sind als maschinenlokal gekennzeichnet; Dateien werden weder gelesen noch
                eingebettet. Der Markdown-Projektsteckbrief ist ein Export, kein Report-Snapshot. 0.4.0 bietet
                keinen Import.

                F1 öffnet diese Hilfe. Alle Daten bleiben lokal in der PIMS-Datenbank.
                """,
        });
    }
}
