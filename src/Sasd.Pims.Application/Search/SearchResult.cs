namespace Sasd.Pims.Application.Search;

/// <summary>A navigation-safe search hit containing identities instead of mutable aggregates.</summary>
public sealed record SearchResult(
    SearchObjectType ObjectType,
    Guid EntityId,
    Guid ProjectId,
    string ProjectKey,
    string ProjectName,
    string Key,
    string Title,
    string MatchHint);

public sealed record SearchPage(IReadOnlyList<SearchResult> Items, int TotalCount, bool HasMore);
