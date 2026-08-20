namespace Sasd.Pims.Application.Search;

/// <summary>Validates limits and delegates search to a persistence-backed read model.</summary>
public sealed class SearchPims(ISearchReader reader)
{
    public Task<SearchPage> ExecuteAsync(SearchQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var normalized = query with
        {
            Text = string.IsNullOrWhiteSpace(query.Text) ? null : query.Text.Trim(),
            Offset = Math.Max(0, query.Offset),
            Limit = Math.Clamp(query.Limit, 1, 500),
        };
        return reader.SearchAsync(normalized, cancellationToken);
    }
}
