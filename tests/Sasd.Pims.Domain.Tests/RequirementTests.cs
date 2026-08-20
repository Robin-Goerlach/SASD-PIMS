using Sasd.Pims.Domain.Projects;
using Sasd.Pims.Domain.Requirements;
using Xunit;

namespace Sasd.Pims.Domain.Tests;

public sealed class RequirementTests
{
    [Fact]
    public void ControlledVocabulariesContainOnlyApprovedValues()
    {
        Assert.Equal(["Must", "Should", "Could"], Enum.GetNames<RequirementPriority>());
        Assert.Equal(["Proposed", "Approved", "Deferred", "Rejected"], Enum.GetNames<RequirementDecisionStatus>());
        Assert.Equal(7, Enum.GetValues<RequirementSourceType>().Length);
        Assert.Contains(ExternalReferenceType.ExternalTask, Enum.GetValues<ExternalReferenceType>());
        Assert.DoesNotContain(Enum.GetNames<ExternalReferenceType>(), value => value == "Task");
    }

    [Theory]
    [InlineData(RequirementDecisionStatus.Deferred)]
    [InlineData(RequirementDecisionStatus.Rejected)]
    public void DeferredAndRejectedRequireReason(RequirementDecisionStatus status)
    {
        var exception = Assert.Throws<DomainValidationException>(() => Create(priority: RequirementPriority.Should,
            status: status, reason: null));
        Assert.Contains(exception.Errors, item => item.Code == "RequirementDecisionReasonRequired");
    }

    [Fact]
    public void MustRequiresCriterionAndCriteriaKeepOrder()
    {
        var id = Guid.NewGuid();
        var second = AcceptanceCriterion.Create(Guid.NewGuid(), id, 2, "Second");
        var first = AcceptanceCriterion.Create(Guid.NewGuid(), id, 1, "First");
        var requirement = Create(id, RequirementPriority.Must, criteria: [second, first]);

        Assert.Equal([1, 2], requirement.AcceptanceCriteria.Select(item => item.Sequence));
        Assert.Throws<DomainValidationException>(() => Create(priority: RequirementPriority.Must, criteria: []));
    }

    [Fact]
    public void UpdatePreservesIdentityProjectAndKey()
    {
        var requirement = Create(priority: RequirementPriority.Should);
        var updated = requirement.Update("Changed", null, null, RequirementPriority.Could,
            RequirementDecisionStatus.Approved, null, RequirementSourceType.Research, null, null, null, []);

        Assert.Equal(requirement.Id, updated.Id);
        Assert.Equal(requirement.ProjectId, updated.ProjectId);
        Assert.Equal("REQ-001", updated.Key);
        Assert.Equal(2, updated.Revision);
    }

    [Fact]
    public void ReferenceRequiresProjectAndPreservesOwnershipWhenEdited()
    {
        var projectId = Guid.NewGuid();
        var requirementId = Guid.NewGuid();
        var reference = ExternalReference.Create(Guid.NewGuid(), projectId, requirementId,
            ExternalReferenceType.WebUrl, "Source", "https://example.test");
        var updated = reference.Update(ExternalReferenceType.Document, "Document", "https://example.test/doc");

        Assert.Equal(projectId, updated.ProjectId);
        Assert.Equal(requirementId, updated.RequirementId);
        Assert.Equal(2, updated.Revision);
        Assert.Throws<DomainValidationException>(() => ExternalReference.Create(Guid.NewGuid(), Guid.Empty, null,
            ExternalReferenceType.WebUrl, "Bad", "https://example.test"));
    }

    private static Requirement Create(Guid? id = null, RequirementPriority priority = RequirementPriority.Should,
        RequirementDecisionStatus status = RequirementDecisionStatus.Proposed, string? reason = null,
        IReadOnlyList<AcceptanceCriterion>? criteria = null)
    {
        var requirementId = id ?? Guid.NewGuid();
        criteria ??= priority == RequirementPriority.Must
            ? [AcceptanceCriterion.Create(Guid.NewGuid(), requirementId, 1, "Criterion")]
            : [];
        return Requirement.Create(requirementId, Guid.NewGuid(), "REQ-001", "Title", null, null, priority,
            status, reason, RequirementSourceType.Internal, null, null, null, criteria);
    }
}
