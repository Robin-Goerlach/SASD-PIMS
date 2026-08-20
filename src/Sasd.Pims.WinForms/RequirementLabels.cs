using Sasd.Pims.Domain.Requirements;

namespace Sasd.Pims.WinForms;

internal static class RequirementLabels
{
    public static string Priority(RequirementPriority value) => value switch
    { RequirementPriority.Must => "Muss", RequirementPriority.Should => "Soll", RequirementPriority.Could => "Kann", _ => value.ToString() };
    public static string Decision(RequirementDecisionStatus value) => value switch
    { RequirementDecisionStatus.Proposed => "Vorgeschlagen", RequirementDecisionStatus.Approved => "Freigegeben", RequirementDecisionStatus.Deferred => "Zurückgestellt", RequirementDecisionStatus.Rejected => "Abgelehnt", _ => value.ToString() };
    public static string Source(RequirementSourceType value) => value switch
    { RequirementSourceType.Internal => "Intern", RequirementSourceType.Stakeholder => "Stakeholder",
      RequirementSourceType.Regulatory => "Recht / Regulatorik", RequirementSourceType.Research => "Recherche",
      RequirementSourceType.ExistingSystem => "Bestandssystem", RequirementSourceType.External => "Extern",
      RequirementSourceType.Other => "Sonstiges", _ => value.ToString() };
    public static string Reference(ExternalReferenceType value) => value switch
    { ExternalReferenceType.WebUrl => "Web-URL", ExternalReferenceType.LocalFile => "Lokale Datei",
      ExternalReferenceType.LocalDirectory => "Lokales Verzeichnis", ExternalReferenceType.Document => "Dokument",
      ExternalReferenceType.Repository => "Repository", ExternalReferenceType.GitHubRepository => "GitHub-Repository",
      ExternalReferenceType.GitHubIssue => "GitHub-Issue", ExternalReferenceType.GitHubPullRequest => "GitHub-Pull-Request",
      ExternalReferenceType.ChatConversation => "Chat-Unterhaltung", ExternalReferenceType.ExternalTask => "Externe Aufgabe",
      ExternalReferenceType.SasdApplication => "SASD-Anwendung", _ => value.ToString() };
}

internal sealed record EnumOption<T>(T Value, string Label) where T : struct, Enum;
