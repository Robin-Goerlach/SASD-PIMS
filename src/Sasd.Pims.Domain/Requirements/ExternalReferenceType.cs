namespace Sasd.Pims.Domain.Requirements;

/// <summary>Defines the controlled semantic types for manually maintained external references.</summary>
public enum ExternalReferenceType
{
    WebUrl, LocalFile, LocalDirectory, Document, Repository, GitHubRepository, GitHubIssue,
    GitHubPullRequest, ChatConversation, ExternalTask, SasdApplication,
}
