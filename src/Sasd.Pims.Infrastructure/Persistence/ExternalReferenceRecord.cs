using Sasd.Pims.Domain.Requirements;
namespace Sasd.Pims.Infrastructure.Persistence;
internal sealed class ExternalReferenceRecord
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? RequirementId { get; set; }
    public ExternalReferenceType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public int Revision { get; set; }
    public ProjectRecord Project { get; set; } = null!;
}
