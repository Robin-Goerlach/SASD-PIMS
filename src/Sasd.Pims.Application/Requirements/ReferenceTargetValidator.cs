using System.Text.RegularExpressions;
using Sasd.Pims.Domain.Requirements;

namespace Sasd.Pims.Application.Requirements;

/// <summary>Centralizes allowlisted target-shape and target-string secret validation.</summary>
public static partial class ReferenceTargetValidator
{
    private static readonly HashSet<string> SecretQueryNames = new(StringComparer.OrdinalIgnoreCase)
    { "token", "access_token", "api_key", "apikey", "secret", "client_secret", "password", "passwd", "pwd" };

    /// <summary>Validates and normalizes the stored target without contacting or reading it.</summary>
    public static ReferenceTargetValidationResult Validate(ExternalReferenceType type, string? target)
    {
        var value = target?.Trim();
        if (string.IsNullOrWhiteSpace(value)) return Invalid("ReferenceTargetRequired", "Ein Referenzziel ist erforderlich.");
        if (CredentialPrefixRegex().IsMatch(value)) return Invalid("ReferenceSecretDetected", "Das Ziel enthält ein erkennbares Zugangsdatenformat.");

        return type switch
        {
            ExternalReferenceType.WebUrl or ExternalReferenceType.ChatConversation or ExternalReferenceType.ExternalTask
                => ValidateHttps(value),
            ExternalReferenceType.LocalFile => ValidateLocal(value, directory: false),
            ExternalReferenceType.LocalDirectory => ValidateLocal(value, directory: true),
            ExternalReferenceType.Document => IsHttps(value) ? ValidateHttps(value) : ValidateLocal(value, false),
            ExternalReferenceType.Repository => IsHttps(value) ? ValidateHttps(value) : ValidateLocal(value, true),
            ExternalReferenceType.GitHubRepository => ValidateGitHub(value, GitHubShape.Repository),
            ExternalReferenceType.GitHubIssue => ValidateGitHub(value, GitHubShape.Issue),
            ExternalReferenceType.GitHubPullRequest => ValidateGitHub(value, GitHubShape.PullRequest),
            ExternalReferenceType.SasdApplication => IsHttps(value) ? ValidateHttps(value) : ValidateLocal(value, false),
            _ => Invalid("ReferenceTypeInvalid", "Der Referenztyp wird nicht unterstützt."),
        };
    }

    private static ReferenceTargetValidationResult ValidateHttps(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || string.IsNullOrWhiteSpace(uri.Host))
            return Invalid("ReferenceHttpsRequired", "Das Ziel muss eine gültige HTTPS-Adresse sein.");
        if (!string.IsNullOrEmpty(uri.UserInfo)) return Invalid("ReferenceSecretDetected", "HTTPS-Ziele dürfen keine Zugangsdaten enthalten.");
        foreach (var part in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var name = Uri.UnescapeDataString(part.Split('=', 2)[0]);
            if (SecretQueryNames.Contains(name)) return Invalid("ReferenceSecretDetected", "Das Ziel enthält einen sensiblen Abfrageparameter.");
        }
        return ReferenceTargetValidationResult.Valid(uri.AbsoluteUri);
    }

    private static ReferenceTargetValidationResult ValidateLocal(string value, bool directory)
    {
        if (!Path.IsPathFullyQualified(value) || Uri.TryCreate(value, UriKind.Absolute, out var uri) && !uri.IsFile)
            return Invalid(directory ? "ReferenceAbsoluteDirectoryRequired" : "ReferenceAbsoluteFileRequired",
                directory ? "Das Ziel muss ein absoluter lokaler Verzeichnispfad sein." : "Das Ziel muss ein absoluter lokaler Dateipfad sein.");
        try { return ReferenceTargetValidationResult.Valid(Path.GetFullPath(value)); }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        { return Invalid("ReferencePathInvalid", "Der lokale Pfad ist ungültig."); }
    }

    private static ReferenceTargetValidationResult ValidateGitHub(string value, GitHubShape shape)
    {
        var https = ValidateHttps(value);
        if (!https.IsValid) return https;
        var uri = new Uri(https.NormalizedTarget!);
        if (!uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase))
            return Invalid("ReferenceGitHubHostInvalid", "Das Ziel muss auf github.com liegen.");
        var segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var valid = shape switch
        {
            GitHubShape.Repository => segments.Length == 2,
            GitHubShape.Issue => segments.Length == 4 && segments[2].Equals("issues", StringComparison.OrdinalIgnoreCase) && int.TryParse(segments[3], out _),
            GitHubShape.PullRequest => segments.Length == 4 && segments[2].Equals("pull", StringComparison.OrdinalIgnoreCase) && int.TryParse(segments[3], out _),
            _ => false,
        };
        return valid ? ReferenceTargetValidationResult.Valid(uri.AbsoluteUri)
            : Invalid("ReferenceGitHubShapeInvalid", "Die GitHub-Adresse passt nicht zum gewählten Referenztyp.");
    }

    private static bool IsHttps(string value) => Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
    private static ReferenceTargetValidationResult Invalid(string code, string message) => ReferenceTargetValidationResult.Invalid(code, message);
    private enum GitHubShape { Repository, Issue, PullRequest }

    [GeneratedRegex(@"(?i)(?:gh[pousr]_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{20,}|sk-[A-Za-z0-9]{20,}|AKIA[0-9A-Z]{16}|-----BEGIN [A-Z ]*PRIVATE KEY-----)")]
    private static partial Regex CredentialPrefixRegex();
}
