using Microsoft.EntityFrameworkCore;
using Sasd.Pims.Application.Traceability;

namespace Sasd.Pims.Infrastructure.Persistence;

/// <summary>Projects only existing project ownership, source and verification relationships.</summary>
public sealed class SqliteTraceabilityReader(IDbContextFactory<PimsDbContext> contextFactory) : ITraceabilityReader
{
    public async Task<TraceabilityNode?> GetProjectAsync(Guid projectId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var project = await context.Projects.AsNoTracking().SingleOrDefaultAsync(item => item.Id == projectId,
            cancellationToken).ConfigureAwait(false);
        if (project is null) return null;
        var requirements = await context.Requirements.AsNoTracking().Where(item => item.ProjectId == projectId)
            .Include(item => item.AcceptanceCriteria).OrderBy(item => item.Key).ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var references = await context.ExternalReferences.AsNoTracking().Where(item => item.ProjectId == projectId)
            .OrderBy(item => item.Title).ToListAsync(cancellationToken).ConfigureAwait(false);
        var blockers = await context.ProjectBlockers.AsNoTracking().Where(item => item.ProjectId == projectId)
            .OrderBy(item => item.Summary).ToListAsync(cancellationToken).ConfigureAwait(false);

        var children = new List<TraceabilityNode>();
        foreach (var requirement in requirements)
        {
            var requirementChildren = new List<TraceabilityNode>();
            if (requirement.SourceReferenceId is Guid sourceId)
                AddReference(requirementChildren, sourceId, "Quelle: ");
            foreach (var criterion in requirement.AcceptanceCriteria.OrderBy(item => item.Sequence))
            {
                var criterionChildren = new List<TraceabilityNode>();
                if (criterion.VerificationReferenceId is Guid verificationId)
                    AddReference(criterionChildren, verificationId, "Nachweis: ");
                requirementChildren.Add(new(TraceabilityNodeType.AcceptanceCriterion, criterion.Id, projectId,
                    $"AK {criterion.Sequence}: {criterion.Text}", criterionChildren));
            }
            requirementChildren.AddRange(references.Where(item => item.RequirementId == requirement.Id)
                .Select(item => ReferenceNode(item, projectId, "Referenz: ")));
            children.Add(new(TraceabilityNodeType.Requirement, requirement.Id, projectId,
                $"{requirement.Key} — {requirement.Title}", requirementChildren));
        }
        children.AddRange(references.Where(item => item.RequirementId == null)
            .Select(item => ReferenceNode(item, projectId, "Projekt-Referenz: ")));
        children.AddRange(blockers.Select(item => new TraceabilityNode(TraceabilityNodeType.Blocker, item.Id,
            projectId, $"Blockade: {item.Summary}", [])));
        return new(TraceabilityNodeType.Project, project.Id, project.Id, $"{project.Key} — {project.Name}", children);

        void AddReference(List<TraceabilityNode> target, Guid id, string prefix)
        {
            var reference = references.SingleOrDefault(item => item.Id == id);
            if (reference is not null) target.Add(ReferenceNode(reference, projectId, prefix));
        }
    }

    private static TraceabilityNode ReferenceNode(ExternalReferenceRecord item, Guid projectId, string prefix) =>
        new(TraceabilityNodeType.ExternalReference, item.Id, projectId, prefix + item.Title, []);
}
