using Sasd.Pims.Application.Projects;
using Sasd.Pims.Application.Search;

namespace Sasd.Pims.WinForms;

/// <summary>Provides bounded global or Project-scoped search backed by server-side read models.</summary>
public sealed class SearchForm : Form
{
    private readonly SearchPims search;
    private readonly TextBox query = new() { AccessibleName = "Suchbegriff" };
    private readonly ComboBox objectType = new() { AccessibleName = "Objekttypfilter", DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox project = new() { AccessibleName = "Projektfilter", DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox sort = new() { AccessibleName = "Sortierung", DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView results = new() { AccessibleName = "Suchergebnisse", Dock = DockStyle.Fill,
        ReadOnly = true, AutoGenerateColumns = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false, AllowUserToAddRows = false };
    private readonly Label status = new() { AccessibleName = "Suchstatus", AutoSize = true };

    public SearchResult? SelectedResult => results.CurrentRow?.DataBoundItem as SearchResult;

    public SearchForm(SearchPims search, IReadOnlyList<ProjectSummaryDto> projects, Guid? selectedProjectId = null)
    {
        this.search = search;
        Text = "PIMS durchsuchen"; Name = nameof(SearchForm); AccessibleName = "Globale PIMS-Suche";
        AutoScaleMode = AutoScaleMode.Dpi; MinimumSize = new Size(850, 520); StartPosition = FormStartPosition.CenterParent;
        query.PlaceholderText = "Suchbegriff"; query.Dock = DockStyle.Fill;
        objectType.Items.AddRange(new object[] { "Alle Objekttypen", "Projekte", "Anforderungen", "Blockaden", "Externe Referenzen" });
        objectType.SelectedIndex = 0;
        project.Items.Add(new ProjectChoice(null, "Alle Projekte"));
        foreach (var item in projects.OrderBy(item => item.Key)) project.Items.Add(new ProjectChoice(item.Id, $"{item.Key} — {item.Name}"));
        project.SelectedItem = project.Items.Cast<ProjectChoice>().FirstOrDefault(item => item.Id == selectedProjectId) ?? project.Items[0];
        sort.DataSource = Enum.GetValues<SearchSort>();
        results.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(SearchResult.ObjectType), HeaderText = "Objekttyp" });
        results.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(SearchResult.ProjectKey), HeaderText = "Projekt" });
        results.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(SearchResult.Key), HeaderText = "Kennung" });
        results.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(SearchResult.Title), HeaderText = "Titel", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        results.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(SearchResult.MatchHint), HeaderText = "Trefferhinweis", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        results.DoubleClick += NavigateClicked;
        var run = Button("&Suchen", "Suche ausführen", SearchClicked);
        var reset = Button("Filter &zurücksetzen", "Alle Suchfilter zurücksetzen", ResetClicked);
        var navigate = Button("&Öffnen", "Ausgewähltes Suchergebnis öffnen", NavigateClicked);
        var close = new Button { Text = "S&chließen", AccessibleName = "Suche schließen", AutoSize = true, DialogResult = DialogResult.Cancel };
        var filters = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 8, Padding = new Padding(8) };
        filters.Controls.Add(new Label { Text = "&Suche", AutoSize = true }, 0, 0); filters.Controls.Add(query, 1, 0);
        filters.Controls.Add(objectType, 2, 0); filters.Controls.Add(project, 3, 0); filters.Controls.Add(sort, 4, 0);
        filters.Controls.Add(run, 5, 0); filters.Controls.Add(reset, 6, 0);
        var commands = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(8) };
        commands.Controls.AddRange([navigate, close, status]);
        Controls.Add(results); Controls.Add(commands); Controls.Add(filters); AcceptButton = run; CancelButton = close;
    }

    private async void SearchClicked(object? sender, EventArgs e)
    {
        try
        {
            UseWaitCursor = true; status.Text = "Suche läuft …";
            var page = await search.ExecuteAsync(new SearchQuery(query.Text, (project.SelectedItem as ProjectChoice)?.Id,
                objectType.SelectedIndex == 0 ? null : (SearchObjectType?)(objectType.SelectedIndex - 1),
                Sort: (SearchSort)sort.SelectedItem!, Limit: 200));
            results.DataSource = page.Items.ToList();
            status.Text = page.HasMore ? $"{page.Items.Count} von {page.TotalCount} Treffern; weitere Treffer vorhanden."
                : $"{page.TotalCount} Treffer.";
        }
        catch (Exception exception) { status.Text = $"Suche fehlgeschlagen: {exception.Message}"; }
        finally { UseWaitCursor = false; }
    }

    private void ResetClicked(object? sender, EventArgs e)
    { query.Clear(); objectType.SelectedIndex = 0; project.SelectedIndex = 0; sort.SelectedItem = SearchSort.Relevance; results.DataSource = null; status.Text = "Filter zurückgesetzt."; }
    private void NavigateClicked(object? sender, EventArgs e) { if (SelectedResult is null) return; DialogResult = DialogResult.OK; Close(); }
    private static Button Button(string text, string accessibleName, EventHandler clicked)
    { var value = new Button { Text = text, AccessibleName = accessibleName, AutoSize = true }; value.Click += clicked; return value; }
    private sealed record ProjectChoice(Guid? Id, string Label) { public override string ToString() => Label; }
}
