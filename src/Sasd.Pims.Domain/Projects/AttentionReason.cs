namespace Sasd.Pims.Domain.Projects;

/// <summary>Identifies one concrete fact that requires attention in the 0.2 project catalog.</summary>
public enum AttentionReason
{
    ReviewOverdue,
    OpenBlocker,
    TargetDateOverdue,
}
