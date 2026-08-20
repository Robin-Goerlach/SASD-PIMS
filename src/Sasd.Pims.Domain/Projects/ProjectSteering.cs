namespace Sasd.Pims.Domain.Projects;

/// <summary>Derives time-sensitive steering information from persisted project facts.</summary>
public static class ProjectSteering
{
    /// <summary>Derives review freshness without storing a snapshot that would become stale as time passes.</summary>
    public static ReviewFreshness GetReviewFreshness(
        DateTimeOffset? lastReviewedAtUtc,
        DateTimeOffset? nextReviewDueAtUtc,
        DateTimeOffset nowUtc)
    {
        EnsureUtc(nowUtc, nameof(nowUtc));
        if (lastReviewedAtUtc is not null) EnsureUtc(lastReviewedAtUtc.Value, nameof(lastReviewedAtUtc));
        if (nextReviewDueAtUtc is null) return ReviewFreshness.NotScheduled;
        EnsureUtc(nextReviewDueAtUtc.Value, nameof(nextReviewDueAtUtc));

        var comparison = DateOnly.FromDateTime(nextReviewDueAtUtc.Value.UtcDateTime)
            .DayNumber.CompareTo(DateOnly.FromDateTime(nowUtc.UtcDateTime).DayNumber);
        if (comparison < 0) return ReviewFreshness.Overdue;
        if (comparison == 0) return ReviewFreshness.DueToday;
        return lastReviewedAtUtc is null ? ReviewFreshness.NotReviewed : ReviewFreshness.Current;
    }

    /// <summary>Derives the fixed 14-calendar-day target proximity required by 0.2.</summary>
    public static DueDateIndication GetDueDateIndication(
        DateOnly? targetDate,
        ActivityState activityState,
        DateOnly today)
    {
        // Completed and cancelled projects remain neutral: a past target is no longer an actionable schedule signal.
        if (targetDate is null || activityState is ActivityState.Completed or ActivityState.Cancelled)
            return DueDateIndication.Neutral;

        var remainingDays = targetDate.Value.DayNumber - today.DayNumber;
        if (remainingDays < 0) return DueDateIndication.Overdue;
        if (remainingDays == 0) return DueDateIndication.DueToday;
        // The deliberately fixed 14-day boundary avoids introducing premature settings/workflow infrastructure.
        return remainingDays <= 14 ? DueDateIndication.DueSoon : DueDateIndication.OnTrack;
    }

    /// <summary>Returns every concrete attention reason rather than collapsing facts into a generic health colour.</summary>
    public static IReadOnlyList<AttentionReason> GetAttentionReasons(
        ReviewFreshness reviewFreshness,
        DueDateIndication dueDateIndication,
        bool hasOpenBlocker)
    {
        var reasons = new List<AttentionReason>(3);
        if (reviewFreshness == ReviewFreshness.Overdue) reasons.Add(AttentionReason.ReviewOverdue);
        if (hasOpenBlocker) reasons.Add(AttentionReason.OpenBlocker);
        if (dueDateIndication == DueDateIndication.Overdue) reasons.Add(AttentionReason.TargetDateOverdue);
        return reasons;
    }

    private static void EnsureUtc(DateTimeOffset value, string field)
    {
        if (value.Offset != TimeSpan.Zero)
            throw new ArgumentException("Steering timestamps must use UTC.", field);
    }
}
