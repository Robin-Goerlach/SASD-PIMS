using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Sasd.Pims.Application.Exchange;
using Sasd.Pims.Domain.Requirements;
using Sasd.Pims.Infrastructure.Persistence;

namespace Sasd.Pims.Infrastructure.Export;

#pragma warning disable CA1305 // Markdown values use explicit canonical enum/date formats; text itself is invariant.
/// <summary>Creates deterministic public exchange 1.0 JSON and Project Markdown via atomic sibling files.</summary>
public sealed class PortableExchangeService(IDbContextFactory<PimsDbContext> contextFactory) : IPortableExchangeService
{
    public const string FormatId = "sasd-pims-exchange";
    public const string FormatVersion = "1.0";
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    public async Task ExportJsonAsync(string targetPath, string productVersion, DateTimeOffset exportedAtUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productVersion);
        EnsureUtc(exportedAtUtc);
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var projects = await context.Projects.AsNoTracking().Include(item => item.Tags)
            .OrderBy(item => item.Key).ToListAsync(cancellationToken).ConfigureAwait(false);
        var requirements = await context.Requirements.AsNoTracking().Include(item => item.AcceptanceCriteria)
            .OrderBy(item => item.ProjectId).ThenBy(item => item.Key).ToListAsync(cancellationToken).ConfigureAwait(false);
        var blockers = (await context.ProjectBlockers.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false))
            .OrderBy(item => item.ProjectId).ThenBy(item => item.CreatedAtUtc).ThenBy(item => item.Id).ToArray();
        var references = await context.ExternalReferences.AsNoTracking().OrderBy(item => item.ProjectId)
            .ThenBy(item => item.RequirementId).ThenBy(item => item.Title).ThenBy(item => item.Id)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var changes = (await context.ChangeEvents.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false))
            .OrderBy(item => item.ProjectId).ThenBy(item => item.OccurredAtUtc).ThenBy(item => item.Id).ToArray();
        var requirementKeys = requirements.ToDictionary(item => item.Id, item => item.Key);

        var envelope = new
        {
            formatId = FormatId,
            formatVersion = FormatVersion,
            productVersion,
            databaseSchemaVersion = Recovery.SqliteRecoveryService.CurrentSchemaVersion,
            exportedAtUtc,
            projects = projects.Select(item => new
            {
                id = item.Id, key = item.Key, name = item.Name, shortDescription = item.ShortDescription,
                goal = item.Goal, benefit = item.Benefit, projectType = item.ProjectType,
                projectArea = item.ProjectArea, responsibility = item.Responsibility,
                phase = item.Phase, activityState = item.ActivityState, targetDate = item.TargetDate,
                lastReviewedAtUtc = item.LastReviewedAtUtc, nextReviewDueAtUtc = item.NextReviewDueAtUtc,
                isArchived = item.IsArchived, createdAtUtc = item.CreatedAtUtc,
                modifiedAtUtc = item.ModifiedAtUtc, revision = item.Revision,
                tags = item.Tags.Select(tag => tag.Value).Order(StringComparer.OrdinalIgnoreCase).ToArray(),
            }).ToArray(),
            requirements = requirements.Select(item => new
            {
                id = item.Id, projectId = item.ProjectId, key = item.Key, title = item.Title,
                description = item.Description, rationale = item.Rationale, priority = item.Priority,
                decisionStatus = item.DecisionStatus, decisionReason = item.DecisionReason,
                sourceType = item.SourceType, sourceDate = item.SourceDate, sourceSummary = item.SourceSummary,
                sourceReferenceId = item.SourceReferenceId, revision = item.Revision,
                acceptanceCriteria = item.AcceptanceCriteria.OrderBy(value => value.Sequence).Select(value => new
                {
                    id = value.Id, requirementId = value.RequirementId, sequence = value.Sequence,
                    text = value.Text, verificationReferenceId = value.VerificationReferenceId,
                }).ToArray(),
            }).ToArray(),
            blockers = blockers.Select(item => new
            {
                id = item.Id, projectId = item.ProjectId, summary = item.Summary, details = item.Details,
                createdAtUtc = item.CreatedAtUtc, resolvedAtUtc = item.ResolvedAtUtc,
                resolutionNote = item.ResolutionNote,
            }).ToArray(),
            externalReferences = references.Select(item => new
            {
                id = item.Id, projectId = item.ProjectId, requirementId = item.RequirementId,
                requirementKey = item.RequirementId is Guid id ? requirementKeys.GetValueOrDefault(id) : null,
                type = item.Type, title = item.Title, target = item.Target,
                locationScope = IsMachineLocal(item) ? "machineLocal" : "portableTarget",
                revision = item.Revision,
            }).ToArray(),
            changeEvents = changes.Select(item => new
            {
                id = item.Id, projectId = item.ProjectId, entityType = item.EntityType, entityId = item.EntityId,
                eventType = item.EventType, occurredAtUtc = item.OccurredAtUtc,
                oldValue = item.OldValue, newValue = item.NewValue,
            }).ToArray(),
        };
        await WriteAtomicAsync(targetPath, async (stream, token) =>
        {
            await JsonSerializer.SerializeAsync(stream, envelope, JsonOptions, token).ConfigureAwait(false);
        }, ValidateJsonAsync, cancellationToken).ConfigureAwait(false);
    }

    public async Task ExportProjectMarkdownAsync(Guid projectId, string targetPath, DateTimeOffset exportedAtUtc,
        CancellationToken cancellationToken = default)
    {
        EnsureUtc(exportedAtUtc);
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var project = await context.Projects.AsNoTracking().Include(item => item.Tags)
            .SingleOrDefaultAsync(item => item.Id == projectId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Project not found.");
        var requirements = await context.Requirements.AsNoTracking().Include(item => item.AcceptanceCriteria)
            .Where(item => item.ProjectId == projectId).OrderBy(item => item.Key).ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var blockers = (await context.ProjectBlockers.AsNoTracking().Where(item => item.ProjectId == projectId)
            .ToListAsync(cancellationToken).ConfigureAwait(false)).OrderBy(item => item.ResolvedAtUtc is not null)
            .ThenBy(item => item.CreatedAtUtc).ToArray();
        var references = await context.ExternalReferences.AsNoTracking().Where(item => item.ProjectId == projectId)
            .OrderBy(item => item.RequirementId).ThenBy(item => item.Title).ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var builder = new StringBuilder();
        builder.AppendLine($"# {Escape(project.Key)} — {Escape(project.Name)}").AppendLine()
            .AppendLine($"Erzeugt: {exportedAtUtc:O}").AppendLine()
            .AppendLine("## Stammdaten").AppendLine()
            .AppendLine($"- ID: `{project.Id:D}`").AppendLine($"- Projektphase: `{project.Phase}`")
            .AppendLine($"- Aktivitätszustand: `{project.ActivityState}`")
            .AppendLine($"- Archiviert: {(project.IsArchived ? "ja" : "nein")}")
            .AppendLine($"- Zieldatum: {project.TargetDate?.ToString("O", System.Globalization.CultureInfo.InvariantCulture) ?? "—"}")
            .AppendLine($"- Zuletzt geprüft: {project.LastReviewedAtUtc?.ToString("O") ?? "—"}")
            .AppendLine($"- Nächste Prüfung: {project.NextReviewDueAtUtc?.ToString("O") ?? "—"}")
            .AppendLine($"- Verantwortung: {Escape(project.Responsibility) ?? "—"}")
            .AppendLine($"- Tags: {(project.Tags.Count == 0 ? "—" : string.Join(", ", project.Tags.Select(item => Escape(item.Value)).Order(StringComparer.OrdinalIgnoreCase)))}")
            .AppendLine().AppendLine("## Anforderungen").AppendLine();
        foreach (var requirement in requirements)
        {
            builder.AppendLine($"### {Escape(requirement.Key)} — {Escape(requirement.Title)}").AppendLine()
                .AppendLine($"Priorität: `{requirement.Priority}` · Entscheidung: `{requirement.DecisionStatus}`").AppendLine();
            if (requirement.AcceptanceCriteria.Count != 0)
            {
                builder.AppendLine("Akzeptanzkriterien:");
                foreach (var criterion in requirement.AcceptanceCriteria.OrderBy(item => item.Sequence))
                    builder.AppendLine($"{criterion.Sequence}. {Escape(criterion.Text)}" +
                        (criterion.VerificationReferenceId is Guid id ? $" (Nachweis `{id:D}`)" : string.Empty));
                builder.AppendLine();
            }
            AppendReferences(builder, references.Where(item => item.RequirementId == requirement.Id));
        }
        builder.AppendLine("## Blockaden").AppendLine();
        foreach (var blocker in blockers)
            builder.AppendLine($"- [{(blocker.ResolvedAtUtc is null ? "offen" : "aufgelöst")}] {Escape(blocker.Summary)}" +
                (blocker.ResolutionNote is null ? string.Empty : $" — {Escape(blocker.ResolutionNote)}"));
        if (blockers.Length == 0) builder.AppendLine("Keine.");
        builder.AppendLine().AppendLine("## Projekt-Referenzen").AppendLine();
        AppendReferences(builder, references.Where(item => item.RequirementId == null));
        var content = builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
        await WriteAtomicAsync(targetPath, async (stream, token) =>
        {
            await using var writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true);
            await writer.WriteAsync(content.AsMemory(), token).ConfigureAwait(false);
            await writer.FlushAsync(token).ConfigureAwait(false);
        }, static (_, _) => Task.CompletedTask, cancellationToken).ConfigureAwait(false);
    }

    private static void AppendReferences(StringBuilder builder, IEnumerable<ExternalReferenceRecord> references)
    {
        var items = references.ToArray();
        foreach (var reference in items)
            builder.AppendLine($"- `{reference.Type}` {Escape(reference.Title)} — `{Escape(reference.Target)}`");
        if (items.Length == 0) builder.AppendLine("Keine.");
        builder.AppendLine();
    }

    private static async Task WriteAtomicAsync(string targetPath, Func<FileStream, CancellationToken, Task> write,
        Func<string, CancellationToken, Task> validate, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);
        var fullPath = Path.GetFullPath(targetPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)
            ?? throw new ArgumentException("Export target must have a parent directory.", nameof(targetPath)));
        var temporary = fullPath + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            await using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                65536, FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await write(stream, cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                stream.Flush(true);
            }
            await validate(temporary, cancellationToken).ConfigureAwait(false);
            if (File.Exists(fullPath)) File.Replace(temporary, fullPath, null); else File.Move(temporary, fullPath);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private static async Task ValidateJsonAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        var root = document.RootElement;
        if (root.GetProperty("formatId").GetString() != FormatId ||
            root.GetProperty("formatVersion").GetString() != FormatVersion)
            throw new InvalidDataException("Exchange envelope validation failed.");
    }

    private static bool IsMachineLocal(ExternalReferenceRecord item) =>
        item.Type is ExternalReferenceType.LocalFile or ExternalReferenceType.LocalDirectory ||
        Path.IsPathFullyQualified(item.Target) && !Uri.TryCreate(item.Target, UriKind.Absolute, out var uri) ||
        Uri.TryCreate(item.Target, UriKind.Absolute, out uri) && uri.IsFile;

    private static string? Escape(string? value) => value?.Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("`", "\\`", StringComparison.Ordinal).Replace("\r", " ", StringComparison.Ordinal)
        .Replace("\n", " ", StringComparison.Ordinal);

    private static void EnsureUtc(DateTimeOffset value)
    {
        if (value.Offset != TimeSpan.Zero) throw new ArgumentException("Export timestamp must be UTC.");
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
#pragma warning restore CA1305
