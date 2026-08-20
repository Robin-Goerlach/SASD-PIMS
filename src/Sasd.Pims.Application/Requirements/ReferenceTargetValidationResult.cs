namespace Sasd.Pims.Application.Requirements;

/// <summary>Returns normalized target syntax or a stable validation error.</summary>
public sealed record ReferenceTargetValidationResult(bool IsValid, string? NormalizedTarget,
    string? ErrorCode, string? ErrorMessage)
{
    public static ReferenceTargetValidationResult Valid(string target) => new(true, target, null, null);
    public static ReferenceTargetValidationResult Invalid(string code, string message) => new(false, null, code, message);
}
