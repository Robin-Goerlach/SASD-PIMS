namespace Sasd.Pims.WinForms;

using Sasd.Pims.Application.Projects;
using Sasd.Pims.Infrastructure.Persistence;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var databasePath = Path.Combine(localData, "SASD", "PIMS", "data", "pims.db");
        var contextFactory = new PimsDbContextFactory(databasePath);
        new DatabaseMigrator(contextFactory).MigrateAsync().GetAwaiter().GetResult();

        var repository = new SqliteProjectRepository(contextFactory);
        var mainForm = new MainForm(
            new CreateProject(repository, TimeProvider.System),
            new LoadProject(repository),
            new ListProjects(repository));
        System.Windows.Forms.Application.Run(mainForm);
    }
}
