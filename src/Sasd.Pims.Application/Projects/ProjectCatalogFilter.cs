namespace Sasd.Pims.Application.Projects;

/// <summary>Defines filters supported by the 0.1 project catalog.</summary>
public sealed record ProjectCatalogFilter(
    bool IncludeArchived = false,
    string? SearchText = null,
    string? ProjectType = null,
    string? ProjectArea = null,
    string? Tag = null);
