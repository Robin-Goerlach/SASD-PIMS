namespace Sasd.Pims.Domain.Projects;

public sealed class Project
{
    private Project(
        Guid id,
        ProjectKey key,
        string name,
        string? shortDescription,
        DateTimeOffset createdAtUtc,
        DateTimeOffset modifiedAtUtc,
        int revision)
    {
        Id = id;
        Key = key;
        Name = name;
        ShortDescription = shortDescription;
        CreatedAtUtc = createdAtUtc;
        ModifiedAtUtc = modifiedAtUtc;
        Revision = revision;
    }

    public Guid Id { get; }

    public ProjectKey Key { get; }

    public string Name { get; private set; }

    public string? ShortDescription { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset ModifiedAtUtc { get; private set; }

    public int Revision { get; private set; }

    public static Project Create(string? key, string? name, string? shortDescription = null)
    {
        return Create(Guid.NewGuid(), key, name, shortDescription, DateTimeOffset.UtcNow);
    }

    public static Project Create(
        Guid id,
        string? key,
        string? name,
        string? shortDescription,
        DateTimeOffset createdAtUtc)
    {
        ValidateIdentityAndTime(id, createdAtUtc);
        var validatedName = ValidateName(name);

        return new Project(
            id,
            ProjectKey.Create(key),
            validatedName,
            NormaliseOptionalText(shortDescription),
            createdAtUtc,
            createdAtUtc,
            1);
    }

    public static Project Reconstitute(
        Guid id,
        string? key,
        string? name,
        string? shortDescription,
        DateTimeOffset createdAtUtc,
        DateTimeOffset modifiedAtUtc,
        int revision)
    {
        ValidateIdentityAndTime(id, createdAtUtc);
        if (modifiedAtUtc.Offset != TimeSpan.Zero || modifiedAtUtc < createdAtUtc)
        {
            throw ValidationError(nameof(ModifiedAtUtc), "ProjectModifiedAtInvalid", "Modified timestamp must be UTC and not precede creation.");
        }

        if (revision < 1)
        {
            throw ValidationError(nameof(Revision), "ProjectRevisionInvalid", "Project revision must be at least 1.");
        }

        return new Project(
            id,
            ProjectKey.Create(key),
            ValidateName(name),
            NormaliseOptionalText(shortDescription),
            createdAtUtc,
            modifiedAtUtc,
            revision);
    }

    public void UpdateDetails(string? name, string? shortDescription, DateTimeOffset modifiedAtUtc)
    {
        if (modifiedAtUtc.Offset != TimeSpan.Zero || modifiedAtUtc <= ModifiedAtUtc)
        {
            throw ValidationError(nameof(ModifiedAtUtc), "ProjectModifiedAtInvalid", "Modified timestamp must be later than the current UTC timestamp.");
        }

        Name = ValidateName(name);
        ShortDescription = NormaliseOptionalText(shortDescription);
        ModifiedAtUtc = modifiedAtUtc;
        Revision++;
    }

    private static void ValidateIdentityAndTime(Guid id, DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw ValidationError(nameof(Id), "ProjectIdRequired", "Project ID is required.");
        }

        if (createdAtUtc.Offset != TimeSpan.Zero)
        {
            throw ValidationError(nameof(CreatedAtUtc), "ProjectCreatedAtInvalid", "Creation timestamp must be UTC.");
        }
    }

    private static string ValidateName(string? name)
    {
        var normalised = name?.Trim() ?? string.Empty;
        if (normalised.Length == 0)
        {
            throw ValidationError(nameof(Name), "ProjectNameRequired", "Project name is required.");
        }

        return normalised;
    }

    private static string? NormaliseOptionalText(string? value)
    {
        var normalised = value?.Trim();
        return string.IsNullOrEmpty(normalised) ? null : normalised;
    }

    private static DomainValidationException ValidationError(string field, string code, string message)
    {
        return new DomainValidationException([new(field, code, message)]);
    }
}
