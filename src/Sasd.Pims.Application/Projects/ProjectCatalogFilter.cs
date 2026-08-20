using Sasd.Pims.Domain.Projects;

namespace Sasd.Pims.Application.Projects;

/// <summary>Defines filters supported by the project catalog.</summary>
public sealed record ProjectCatalogFilter(
    bool IncludeArchived = false,
    string? SearchText = null,
    string? ProjectType = null,
    string? ProjectArea = null,
    string? Tag = null,
    ProjectPhase? Phase = null,
    ActivityState? ActivityState = null,
    ReviewFreshness? ReviewFreshness = null,
    bool NeedsAttentionOnly = false);
