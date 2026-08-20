namespace Sasd.Pims.Application.Search;

public interface ISearchReader
{
    Task<SearchPage> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default);
}
