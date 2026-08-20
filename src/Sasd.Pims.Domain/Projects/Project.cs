using System.Text.RegularExpressions;

namespace Sasd.Pims.Domain.Projects;

/// <summary>Represents the authoritative project master record with stable identity and revision evidence.</summary>
public sealed partial class Project
{
    private Project(Guid id, ProjectKey key, string name, string? shortDescription, string? goal,
        string? benefit, string? projectType, string? projectArea, string? responsibility,
        IReadOnlyList<string> tags, bool isArchived, DateTimeOffset createdAtUtc,
        DateTimeOffset modifiedAtUtc, int revision)
    {
        Id = id;
        Key = key;
        Name = name;
        ShortDescription = shortDescription;
        Goal = goal;
        Benefit = benefit;
        ProjectType = projectType;
        ProjectArea = projectArea;
        Responsibility = responsibility;
        Tags = tags;
        IsArchived = isArchived;
        CreatedAtUtc = createdAtUtc;
        ModifiedAtUtc = modifiedAtUtc;
        Revision = revision;
    }

    public Guid Id { get; }
    public ProjectKey Key { get; }
    public string Name { get; private set; }
    public string? ShortDescription { get; private set; }
    public string? Goal { get; private set; }
    public string? Benefit { get; private set; }
    /// <summary>Gets the optional language-neutral project-type code.</summary>
    public string? ProjectType { get; private set; }
    /// <summary>Gets the optional language-neutral project-area code.</summary>
    public string? ProjectArea { get; private set; }
    public string? Responsibility { get; private set; }
    /// <summary>Gets normalised, case-insensitively unique classification tags.</summary>
    public IReadOnlyList<string> Tags { get; private set; }
    /// <summary>Gets whether the project is hidden from the active catalog without deleting its data.</summary>
    public bool IsArchived { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset ModifiedAtUtc { get; private set; }
    /// <summary>Gets the optimistic-concurrency revision incremented by each accepted mutation.</summary>
    public int Revision { get; private set; }

    public static Project Create(string? key, string? name, string? shortDescription = null) =>
        Create(Guid.NewGuid(), key, name, shortDescription, DateTimeOffset.UtcNow);

    public static Project Create(Guid id, string? key, string? name, string? shortDescription,
        DateTimeOffset createdAtUtc) =>
        Create(id, key, name, shortDescription, null, null, null, null, null, [], createdAtUtc);

    /// <summary>Creates a new active project master record at revision one.</summary>
    public static Project Create(Guid id, string? key, string? name, string? shortDescription,
        string? goal, string? benefit, string? projectType, string? projectArea,
        string? responsibility, IEnumerable<string?>? tags, DateTimeOffset createdAtUtc)
    {
        ValidateIdentityAndTime(id, createdAtUtc);
        ValidateMasterData(key, name, projectType, projectArea, tags);
        return new(id, ProjectKey.Create(key), name!.Trim(), NormaliseText(shortDescription),
            NormaliseText(goal), NormaliseText(benefit), NormaliseCode(projectType),
            NormaliseCode(projectArea), NormaliseText(responsibility), NormaliseTags(tags), false,
            createdAtUtc, createdAtUtc, 1);
    }

    /// <summary>Reconstitutes a persisted project after validating its stored invariants.</summary>
    public static Project Reconstitute(Guid id, string? key, string? name, string? shortDescription,
        string? goal, string? benefit, string? projectType, string? projectArea,
        string? responsibility, IEnumerable<string?>? tags, bool isArchived,
        DateTimeOffset createdAtUtc, DateTimeOffset modifiedAtUtc, int revision)
    {
        ValidateIdentityAndTime(id, createdAtUtc);
        ValidateMasterData(key, name, projectType, projectArea, tags);
        if (modifiedAtUtc.Offset != TimeSpan.Zero || modifiedAtUtc < createdAtUtc)
            throw Error(nameof(ModifiedAtUtc), "ProjectModifiedAtInvalid", "Modified timestamp must be UTC and not precede creation.");
        if (revision < 1)
            throw Error(nameof(Revision), "ProjectRevisionInvalid", "Project revision must be at least 1.");

        return new(id, ProjectKey.Create(key), name!.Trim(), NormaliseText(shortDescription),
            NormaliseText(goal), NormaliseText(benefit), NormaliseCode(projectType),
            NormaliseCode(projectArea), NormaliseText(responsibility), NormaliseTags(tags), isArchived,
            createdAtUtc, modifiedAtUtc, revision);
    }

    public static Project Reconstitute(Guid id, string? key, string? name, string? shortDescription,
        DateTimeOffset createdAtUtc, DateTimeOffset modifiedAtUtc, int revision) =>
        Reconstitute(id, key, name, shortDescription, null, null, null, null, null, [], false,
            createdAtUtc, modifiedAtUtc, revision);

    /// <summary>Replaces editable master data while preserving the stable project key.</summary>
    public void UpdateDetails(string? name, string? shortDescription, string? goal, string? benefit,
        string? projectType, string? projectArea, string? responsibility, IEnumerable<string?>? tags,
        DateTimeOffset modifiedAtUtc)
    {
        ValidateLaterModification(modifiedAtUtc);
        ValidateMasterData(Key.Value, name, projectType, projectArea, tags);
        Name = name!.Trim();
        ShortDescription = NormaliseText(shortDescription);
        Goal = NormaliseText(goal);
        Benefit = NormaliseText(benefit);
        ProjectType = NormaliseCode(projectType);
        ProjectArea = NormaliseCode(projectArea);
        Responsibility = NormaliseText(responsibility);
        Tags = NormaliseTags(tags);
        AcceptMutation(modifiedAtUtc);
    }

    public void UpdateDetails(string? name, string? shortDescription, DateTimeOffset modifiedAtUtc) =>
        UpdateDetails(name, shortDescription, Goal, Benefit, ProjectType, ProjectArea, Responsibility, Tags, modifiedAtUtc);

    /// <summary>Archives an active project without deleting its master data.</summary>
    public void Archive(DateTimeOffset modifiedAtUtc)
    {
        ValidateLaterModification(modifiedAtUtc);
        if (IsArchived) return;
        IsArchived = true;
        AcceptMutation(modifiedAtUtc);
    }

    /// <summary>Returns an archived project to the active catalog.</summary>
    public void Reactivate(DateTimeOffset modifiedAtUtc)
    {
        ValidateLaterModification(modifiedAtUtc);
        if (!IsArchived) return;
        IsArchived = false;
        AcceptMutation(modifiedAtUtc);
    }

    private void AcceptMutation(DateTimeOffset modifiedAtUtc)
    {
        // Persistence compares the previous revision so stale editors cannot overwrite newer data.
        ModifiedAtUtc = modifiedAtUtc;
        Revision++;
    }

    private void ValidateLaterModification(DateTimeOffset value)
    {
        if (value.Offset != TimeSpan.Zero || value <= ModifiedAtUtc)
            throw Error(nameof(ModifiedAtUtc), "ProjectModifiedAtInvalid", "Modified timestamp must be later than the current UTC timestamp.");
    }

    private static void ValidateIdentityAndTime(Guid id, DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty) throw Error(nameof(Id), "ProjectIdRequired", "Project ID is required.");
        if (createdAtUtc.Offset != TimeSpan.Zero)
            throw Error(nameof(CreatedAtUtc), "ProjectCreatedAtInvalid", "Creation timestamp must be UTC.");
    }

    private static void ValidateMasterData(string? key, string? name, string? type, string? area,
        IEnumerable<string?>? tags)
    {
        var errors = ProjectKey.Validate(key).ToList();
        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new(nameof(Name), "ProjectNameRequired", "Project name is required."));
        ValidateCode(nameof(ProjectType), type, errors);
        ValidateCode(nameof(ProjectArea), area, errors);
        ValidateTagValues(tags, errors);
        if (errors.Count > 0) throw new DomainValidationException(errors);
    }

    private static void ValidateCode(string field, string? value, List<DomainValidationError> errors)
    {
        var code = NormaliseCode(value);
        if (code is not null && (code.Length > 32 || !ClassificationCodePattern().IsMatch(code)))
            errors.Add(new(field, "ProjectClassificationInvalid", "Classification codes must use 1 to 32 letters, digits or single hyphens."));
    }

    private static void ValidateTagValues(IEnumerable<string?>? tags, List<DomainValidationError> errors)
    {
        var values = tags?.ToArray() ?? [];
        if (values.Length > 20) errors.Add(new(nameof(Tags), "ProjectTagsTooMany", "A project can have at most 20 tags."));
        if (values.Any(tag => string.IsNullOrWhiteSpace(tag) || tag.Trim().Length > 32))
            errors.Add(new(nameof(Tags), "ProjectTagInvalid", "Tags must contain 1 to 32 characters."));
    }

    private static string? NormaliseText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string? NormaliseCode(string? value) => NormaliseText(value)?.ToUpperInvariant();
    private static string[] NormaliseTags(IEnumerable<string?>? tags) =>
        (tags ?? []).Select(tag => tag!.Trim()).Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase).ToArray();
    private static DomainValidationException Error(string field, string code, string message) =>
        new([new(field, code, message)]);

    [GeneratedRegex("^[A-Z0-9]+(?:-[A-Z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex ClassificationCodePattern();
}
