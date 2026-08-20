using Sasd.Pims.Domain.Requirements;

namespace Sasd.Pims.Application.Requirements;

/// <summary>Neutral result model for a manually maintained reference.</summary>
public sealed record ExternalReferenceDto(Guid Id, Guid ProjectId, Guid? RequirementId,
    ExternalReferenceType Type, string Title, string Target, int Revision)
{
    public static ExternalReferenceDto FromDomain(ExternalReference item) => new(item.Id, item.ProjectId,
        item.RequirementId, item.Type, item.Title, item.Target, item.Revision);
}
