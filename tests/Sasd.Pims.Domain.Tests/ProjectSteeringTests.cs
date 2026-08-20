using Sasd.Pims.Domain.Projects;
using Xunit;

namespace Sasd.Pims.Domain.Tests;

public sealed class ProjectSteeringTests
{
    [Fact]
    public void ReviewFreshnessRejectsNonUtcReviewFacts()
    {
        var localOffset = new DateTimeOffset(2026, 8, 1, 10, 0, 0, TimeSpan.FromHours(2));

        Assert.Throws<ArgumentException>(() => ProjectSteering.GetReviewFreshness(
            localOffset,
            null,
            new DateTimeOffset(2026, 8, 20, 8, 0, 0, TimeSpan.Zero)));
    }

    private static readonly DateTimeOffset Now = new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 8, 20);

    [Fact]
    public void PhaseActivityAndArchiveRemainIndependent()
    {
        var project = Project.Create(Guid.NewGuid(), "STEERING", "Steering", null, Now.AddDays(-1));
        project.UpdateSteering(ProjectPhase.Closure, ActivityState.Active, Today.AddDays(5), null,
            Now.AddDays(10), Now);
        project.Archive(Now.AddTicks(1));
        project.Reactivate(Now.AddTicks(2));

        Assert.Equal(ProjectPhase.Closure, project.Phase);
        Assert.Equal(ActivityState.Active, project.ActivityState);
        Assert.False(project.IsArchived);
        Assert.Equal(4, project.Revision);
    }

    [Theory]
    [InlineData(null, null, ReviewFreshness.NotScheduled)]
    [InlineData(null, 1, ReviewFreshness.NotReviewed)]
    [InlineData(-1, 1, ReviewFreshness.Current)]
    [InlineData(-1, 0, ReviewFreshness.DueToday)]
    [InlineData(-2, -1, ReviewFreshness.Overdue)]
    public void ReviewFreshnessIsDerivedAtCalendarBoundaries(
        int? reviewedOffsetDays,
        int? dueOffsetDays,
        ReviewFreshness expected)
    {
        DateTimeOffset? reviewed = reviewedOffsetDays is null ? null : Now.AddDays(reviewedOffsetDays.Value);
        DateTimeOffset? due = dueOffsetDays is null ? null : Now.AddDays(dueOffsetDays.Value);

        Assert.Equal(expected, ProjectSteering.GetReviewFreshness(reviewed, due, Now));
    }

    [Theory]
    [InlineData(null, ActivityState.Active, DueDateIndication.Neutral)]
    [InlineData(15, ActivityState.Active, DueDateIndication.OnTrack)]
    [InlineData(14, ActivityState.Active, DueDateIndication.DueSoon)]
    [InlineData(1, ActivityState.Active, DueDateIndication.DueSoon)]
    [InlineData(0, ActivityState.Active, DueDateIndication.DueToday)]
    [InlineData(-1, ActivityState.Active, DueDateIndication.Overdue)]
    [InlineData(-1, ActivityState.Completed, DueDateIndication.Neutral)]
    [InlineData(-1, ActivityState.Cancelled, DueDateIndication.Neutral)]
    public void DueDateIndicationUsesFixedFourteenDayBoundary(
        int? targetOffsetDays,
        ActivityState activityState,
        DueDateIndication expected)
    {
        DateOnly? target = targetOffsetDays is null ? null : Today.AddDays(targetOffsetDays.Value);
        Assert.Equal(expected, ProjectSteering.GetDueDateIndication(target, activityState, Today));
    }

    [Fact]
    public void AttentionReturnsEveryConcreteReason()
    {
        var reasons = ProjectSteering.GetAttentionReasons(
            ReviewFreshness.Overdue, DueDateIndication.Overdue, hasOpenBlocker: true);

        Assert.Equal([AttentionReason.ReviewOverdue, AttentionReason.OpenBlocker,
            AttentionReason.TargetDateOverdue], reasons);
    }
}
