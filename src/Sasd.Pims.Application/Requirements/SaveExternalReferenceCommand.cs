using Sasd.Pims.Domain.Requirements;

namespace Sasd.Pims.Application.Requirements;

/// <summary>Captures editable reference facts while ownership comes from the current Project context.</summary>
public sealed record SaveExternalReferenceCommand(ExternalReferenceType Type, string? Title, string? Target);
