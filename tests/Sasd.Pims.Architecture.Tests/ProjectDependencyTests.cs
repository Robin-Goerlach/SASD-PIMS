using System.Xml.Linq;
using Xunit;

namespace Sasd.Pims.Architecture.Tests;

public sealed class ProjectDependencyTests
{
    private const string Application = "Sasd.Pims.Application";
    private const string Domain = "Sasd.Pims.Domain";
    private const string Infrastructure = "Sasd.Pims.Infrastructure";
    private const string WinForms = "Sasd.Pims.WinForms";

    private static readonly IReadOnlyDictionary<string, string> ProductionProjectPaths =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [Domain] = "src/Sasd.Pims.Domain/Sasd.Pims.Domain.csproj",
            [Application] = "src/Sasd.Pims.Application/Sasd.Pims.Application.csproj",
            [Infrastructure] = "src/Sasd.Pims.Infrastructure/Sasd.Pims.Infrastructure.csproj",
            [WinForms] = "src/Sasd.Pims.WinForms/Sasd.Pims.WinForms.csproj",
        };

    [Fact]
    public void DomainDoesNotReferenceWinForms()
    {
        Assert.DoesNotContain(WinForms, ReadProjectReferences(Domain));
    }

    [Fact]
    public void DomainDoesNotReferenceEntityFrameworkCore()
    {
        Assert.DoesNotContain(
            ReadPackageReferences(Domain),
            package => package.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void DomainDoesNotReferenceInfrastructure()
    {
        Assert.DoesNotContain(Infrastructure, ReadProjectReferences(Domain));
    }

    [Fact]
    public void ApplicationDoesNotReferenceWinForms()
    {
        Assert.DoesNotContain(WinForms, ReadProjectReferences(Application));
    }

    [Fact]
    public void ProductionProjectReferencesAreAcyclic()
    {
        var graph = ProductionProjectPaths.Keys.ToDictionary(
            project => project,
            ReadProjectReferences,
            StringComparer.OrdinalIgnoreCase);

        foreach (var project in graph.Keys)
        {
            Assert.False(ContainsCycle(project, graph, [], []), $"A project-reference cycle starts at {project}.");
        }
    }

    [Fact]
    public void WinFormsFormsAndControlsDoNotUseAConcreteDbContext()
    {
        var winFormsDirectory = Path.GetDirectoryName(GetProjectPath(WinForms))!;
        var sourceFiles = Directory.EnumerateFiles(winFormsDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsBuildOutput(path))
            .Where(IsFormOrControlSource);

        foreach (var sourceFile in sourceFiles)
        {
            var source = File.ReadAllText(sourceFile);
            Assert.DoesNotContain("DbContext", source, StringComparison.Ordinal);
        }
    }

    private static bool ContainsCycle(
        string project,
        IReadOnlyDictionary<string, string[]> graph,
        HashSet<string> visiting,
        HashSet<string> visited)
    {
        if (visited.Contains(project))
        {
            return false;
        }

        if (!visiting.Add(project))
        {
            return true;
        }

        foreach (var dependency in graph[project].Where(graph.ContainsKey))
        {
            if (ContainsCycle(dependency, graph, visiting, visited))
            {
                return true;
            }
        }

        visiting.Remove(project);
        visited.Add(project);
        return false;
    }

    private static string[] ReadProjectReferences(string project)
    {
        var projectPath = GetProjectPath(project);
        var projectDirectory = Path.GetDirectoryName(projectPath)!;

        return LoadProject(projectPath)
            .Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Select(include => Path.GetFullPath(include!, projectDirectory))
            .Select(path => Path.GetFileNameWithoutExtension(path)!)
            .ToArray();
    }

    private static string[] ReadPackageReferences(string project)
    {
        return LoadProject(GetProjectPath(project))
            .Descendants("PackageReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Select(include => include!)
            .ToArray();
    }

    private static XDocument LoadProject(string projectPath)
    {
        return XDocument.Load(projectPath);
    }

    private static string GetProjectPath(string project)
    {
        return Path.Combine(FindRepositoryRoot(), ProductionProjectPaths[project]);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Sasd.Pims.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Could not locate the repository root.");
    }

    private static bool IsBuildOutput(string path)
    {
        var relativePath = Path.GetRelativePath(FindRepositoryRoot(), path);
        var segments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return segments.Contains("bin", StringComparer.OrdinalIgnoreCase)
            || segments.Contains("obj", StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsFormOrControlSource(string path)
    {
        var fileName = Path.GetFileName(path);
        return fileName.Contains("Form", StringComparison.OrdinalIgnoreCase)
            || fileName.Contains("Control", StringComparison.OrdinalIgnoreCase);
    }
}
