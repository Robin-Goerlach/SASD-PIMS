using Sasd.Pims.Domain.Projects;
using Xunit;

namespace Sasd.Pims.Domain.Tests;

public sealed class ProjectBlockerTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void BlockerRetainsDetailsAndResolutionHistory()
    {
        var blocker = ProjectBlocker.Create(Guid.NewGuid(), Guid.NewGuid(), " Waiting for access ",
            " Synthetic detail ", "Permission missing", "Work stopped", "Repository", "Request access", null, Now);
        blocker.Resolve(Now.AddHours(1), " Access granted ");

        Assert.False(blocker.IsOpen);
        Assert.Equal("Waiting for access", blocker.Summary);
        Assert.Equal("Synthetic detail", blocker.Details);
        Assert.Equal("Access granted", blocker.ResolutionNote);
        Assert.Equal(Now.AddHours(1), blocker.ResolvedAtUtc);
    }

    [Fact]
    public void BlankSummaryIsRejected()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            ProjectBlocker.Create(Guid.NewGuid(), Guid.NewGuid(), " ", null, null, "Impact", null, "Act", null, Now));
        Assert.Contains(exception.Errors, error => error.Code == "BlockerSummaryRequired");
    }

    [Fact]
    public void ResolvingTwiceIsRejectedAndOriginalEvidenceRemains()
    {
        var blocker = ProjectBlocker.Create(Guid.NewGuid(), Guid.NewGuid(), "Blocked", null, null,
            "Impact", null, "Act", null, Now);
        blocker.Resolve(Now.AddHours(1), "First resolution");

        Assert.Throws<DomainValidationException>(() => blocker.Resolve(Now.AddHours(2), "Overwrite"));
        Assert.Equal("First resolution", blocker.ResolutionNote);
    }

    [Theory]
    [InlineData(null, "Act", "BlockerImpactRequired")]
    [InlineData("Impact", null, "BlockerNextActionRequired")]
    public void NewOpenBlockerRequiresImpactAndNextAction(string? impact, string? nextAction, string code)
    {
        var exception = Assert.Throws<DomainValidationException>(() => ProjectBlocker.Create(Guid.NewGuid(),
            Guid.NewGuid(), "Blocked", null, null, impact, null, nextAction, null, Now));
        Assert.Contains(exception.Errors, error => error.Code == code);
    }
}
