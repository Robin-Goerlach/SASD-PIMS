using System.Text.RegularExpressions;

namespace Sasd.Pims.Domain.Projects;

public sealed partial record ProjectKey
{
    private ProjectKey(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ProjectKey Create(string? value)
    {
        var normalised = value?.Trim().ToUpperInvariant() ?? string.Empty;
        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new DomainValidationException(errors);
        }

        return new ProjectKey(normalised);
    }

    public override string ToString() => Value;

    internal static IReadOnlyList<DomainValidationError> Validate(string? value)
    {
        var normalised = value?.Trim().ToUpperInvariant() ?? string.Empty;
        return normalised.Length is < 1 or > 64 || !ValidProjectKey().IsMatch(normalised)
            ?
            [
                new(
                    nameof(Project.Key),
                    "ProjectKeyInvalid",
                    "Project key must contain 1-64 letters, digits or single hyphens and start and end with a letter or digit."),
            ]
            : [];
    }

    [GeneratedRegex("^[A-Z0-9]+(?:-[A-Z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex ValidProjectKey();
}
