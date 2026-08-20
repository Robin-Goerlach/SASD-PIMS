using Sasd.Pims.Application.Requirements;
using Sasd.Pims.Domain.Requirements;
using Xunit;

namespace Sasd.Pims.Application.Tests;

public sealed class ReferenceTargetValidatorTests
{
    [Theory]
    [InlineData(ExternalReferenceType.WebUrl, "https://example.test/path")]
    [InlineData(ExternalReferenceType.GitHubRepository, "https://github.com/owner/repository")]
    [InlineData(ExternalReferenceType.GitHubIssue, "https://github.com/owner/repository/issues/42")]
    [InlineData(ExternalReferenceType.GitHubPullRequest, "https://github.com/owner/repository/pull/7")]
    [InlineData(ExternalReferenceType.ChatConversation, "https://chat.example.test/conversation/1")]
    public void ApprovedHttpsShapesAreAccepted(ExternalReferenceType type, string target) =>
        Assert.True(ReferenceTargetValidator.Validate(type, target).IsValid);

    [Theory]
    [InlineData("http://example.test")]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/plain,test")]
    [InlineData("powershell:Write-Host")]
    [InlineData("cmd:/c calc")]
    public void UnsafeOrUnsupportedSchemasAreRejected(string target) =>
        Assert.False(ReferenceTargetValidator.Validate(ExternalReferenceType.WebUrl, target).IsValid);

    [Theory]
    [InlineData("https://user:password@example.test/path")]
    [InlineData("https://example.test/path?access_token=secret")]
    [InlineData("ghp_abcdefghijklmnopqrstuvwxyz123456")]
    [InlineData("github_pat_abcdefghijklmnopqrstuvwxyz123456")]
    public void ObviousSecretsAreRejected(string target) =>
        Assert.Equal("ReferenceSecretDetected",
            ReferenceTargetValidator.Validate(ExternalReferenceType.WebUrl, target).ErrorCode);

    [Fact]
    public void AbsoluteLocalTargetsAreAcceptedWithoutExistenceProbe()
    {
        Assert.True(ReferenceTargetValidator.Validate(ExternalReferenceType.LocalFile,
            @"C:\Synthetic\missing.txt").IsValid);
        Assert.True(ReferenceTargetValidator.Validate(ExternalReferenceType.LocalDirectory,
            @"C:\Synthetic\missing").IsValid);
    }

    [Theory]
    [InlineData(ExternalReferenceType.GitHubRepository, "https://example.test/owner/repo")]
    [InlineData(ExternalReferenceType.GitHubIssue, "https://github.com/owner/repo/pull/1")]
    [InlineData(ExternalReferenceType.GitHubPullRequest, "https://github.com/owner/repo/issues/1")]
    public void WrongGitHubShapeIsRejected(ExternalReferenceType type, string target) =>
        Assert.False(ReferenceTargetValidator.Validate(type, target).IsValid);
}
