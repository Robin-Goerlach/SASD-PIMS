using Sasd.Pims.Domain.Requirements;

namespace Sasd.Pims.Application.Search;

/// <summary>Defines a bounded server-side search over PIMS read models.</summary>
public sealed record SearchQuery(
    string? Text = null,
    Guid? ProjectId = null,
    SearchObjectType? ObjectType = null,
    RequirementPriority? RequirementPriority = null,
    RequirementDecisionStatus? DecisionStatus = null,
    RequirementSourceType? SourceType = null,
    ExternalReferenceType? ReferenceType = null,
    SearchSort Sort = SearchSort.Relevance,
    int Offset = 0,
    int Limit = 200);
