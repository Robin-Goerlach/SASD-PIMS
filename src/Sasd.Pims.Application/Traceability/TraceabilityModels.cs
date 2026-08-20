namespace Sasd.Pims.Application.Traceability;

public enum TraceabilityNodeType { Project, Requirement, AcceptanceCriterion, ExternalReference, Blocker }

public sealed record TraceabilityNode(
    TraceabilityNodeType Type,
    Guid Id,
    Guid ProjectId,
    string Label,
    IReadOnlyList<TraceabilityNode> Children);
