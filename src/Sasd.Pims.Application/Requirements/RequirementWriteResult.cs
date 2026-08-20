namespace Sasd.Pims.Application.Requirements;

/// <summary>Reports persistence outcomes including observable stale-write and key conflicts.</summary>
public enum RequirementWriteResult { Saved, DuplicateKey, ConcurrencyConflict }
