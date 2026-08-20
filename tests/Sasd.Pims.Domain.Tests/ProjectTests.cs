using Sasd.Pims.Domain.Projects;
using Xunit;

namespace Sasd.Pims.Domain.Tests;

public sealed class ProjectTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 8, 19, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ValidProjectHasStableIdentityUtcTimestampsAndInitialRevision()
    {
        var id = Guid.NewGuid();

        var project = Project.Create(id, " sasd-pims ", " SASD PIMS ", " Synthetic project ", CreatedAt);

        Assert.Equal(id, project.Id);
        Assert.Equal("SASD-PIMS", project.Key.Value);
        Assert.Equal("SASD PIMS", project.Name);
        Assert.Equal("Synthetic project", project.ShortDescription);
        Assert.Equal(CreatedAt, project.CreatedAtUtc);
        Assert.Equal(CreatedAt, project.ModifiedAtUtc);
        Assert.Equal(1, project.Revision);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("-DEMO")]
    [InlineData("DEMO-")]
    [InlineData("DEMO--ONE")]
    [InlineData("DEMO_ONE")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    public void InvalidProjectKeyIsRejected(string? key)
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => Project.Create(Guid.NewGuid(), key, "Demo", null, CreatedAt));

        Assert.Contains(exception.Errors, error => error.Code == "ProjectKeyInvalid");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankProjectNameIsRejected(string? name)
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => Project.Create(Guid.NewGuid(), "DEMO", name, null, CreatedAt));

        Assert.Contains(exception.Errors, error => error.Code == "ProjectNameRequired");
    }

    [Fact]
    public void InvalidCreationAggregatesIndependentInputErrors()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => Project.Create(Guid.NewGuid(), " ", " ", null, CreatedAt));

        Assert.Contains(exception.Errors, error => error.Code == "ProjectKeyInvalid");
        Assert.Contains(exception.Errors, error => error.Code == "ProjectNameRequired");
    }

    [Fact]
    public void ProjectKeyCannotBeChangedThroughMutationApi()
    {
        var project = Project.Create(Guid.NewGuid(), "DEMO", "Demo", null, CreatedAt);

        project.UpdateDetails("Renamed", null, CreatedAt.AddMinutes(1));

        Assert.Equal("DEMO", project.Key.Value);
    }

    [Fact]
    public void UpdatingDetailsAdvancesModificationEvidence()
    {
        var project = Project.Create(Guid.NewGuid(), "DEMO", "Demo", null, CreatedAt);
        var modifiedAt = CreatedAt.AddMinutes(1);

        project.UpdateDetails(" Updated ", " Details ", modifiedAt);

        Assert.Equal("Updated", project.Name);
        Assert.Equal("Details", project.ShortDescription);
        Assert.Equal(CreatedAt, project.CreatedAtUtc);
        Assert.Equal(modifiedAt, project.ModifiedAtUtc);
        Assert.Equal(2, project.Revision);
    }

    [Fact]
    public void CompleteMasterDataIsNormalisedAndEditableWithoutChangingIdentity()
    {
        var project = Project.Create(Guid.NewGuid(), " catalog-1 ", " Catalog ", " Short ",
            " Goal ", " Benefit ", " software-project ", " internal-tools ", " Team PIMS ",
            [" desktop ", "Desktop", " local-first "], CreatedAt);
        var id = project.Id;

        project.UpdateDetails("Renamed", "Updated", "New goal", "New benefit", "documentation",
            "internal-tools", "Team Docs", ["docs"], CreatedAt.AddMinutes(1));

        Assert.Equal(id, project.Id);
        Assert.Equal("CATALOG-1", project.Key.Value);
        Assert.Equal("DOCUMENTATION", project.ProjectType);
        Assert.Equal("INTERNAL-TOOLS", project.ProjectArea);
        Assert.Equal("Team Docs", project.Responsibility);
        Assert.Equal(["docs"], project.Tags);
        Assert.Equal(2, project.Revision);
    }

    [Fact]
    public void ArchiveAndReactivateAreReversibleAndAdvanceRevision()
    {
        var project = Project.Create(Guid.NewGuid(), "DEMO", "Demo", null, CreatedAt);
        project.Archive(CreatedAt.AddMinutes(1));
        Assert.True(project.IsArchived);
        Assert.Equal(2, project.Revision);

        project.Reactivate(CreatedAt.AddMinutes(2));
        Assert.False(project.IsArchived);
        Assert.Equal(3, project.Revision);
    }

    [Theory]
    [InlineData("-SOFTWARE")]
    [InlineData("SOFTWARE-")]
    [InlineData("SOFTWARE--PROJECT")]
    [InlineData("SOFTWARE PROJECT")]
    public void InvalidClassificationCodeIsRejected(string code)
    {
        var exception = Assert.Throws<DomainValidationException>(() => Project.Create(Guid.NewGuid(),
            "DEMO", "Demo", null, null, null, code, null, null, [], CreatedAt));
        Assert.Contains(exception.Errors, error => error.Code == "ProjectClassificationInvalid");
    }

    [Fact]
    public void InvalidTagsAreRejected()
    {
        var exception = Assert.Throws<DomainValidationException>(() => Project.Create(Guid.NewGuid(),
            "DEMO", "Demo", null, null, null, null, null, null, [" "], CreatedAt));
        Assert.Contains(exception.Errors, error => error.Code == "ProjectTagInvalid");
    }
}
