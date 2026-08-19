namespace Sasd.Pims.WinForms;

using Sasd.Pims.Application.Projects;
using Microsoft.Extensions.Logging;
using Sasd.Pims.Application.Diagnostics;
using Sasd.Pims.Infrastructure.Diagnostics;
using Sasd.Pims.Infrastructure.Persistence;

internal static class Program
{
    private static readonly Action<ILogger, Exception?> LogApplicationStarted = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(1, "ApplicationStarted"),
        "Application started.");

    private static readonly Action<ILogger, Exception?> LogApplicationStopped = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(2, "ApplicationStopped"),
        "Application stopped.");

    private static readonly Action<ILogger, string, Exception?> LogUnhandledFailure = LoggerMessage.Define<string>(
        LogLevel.Critical,
        new EventId(1001, "UnhandledFailure"),
        "Unhandled application failure. ErrorId {ErrorId}");

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var applicationRoot = Path.Combine(localData, "SASD", "PIMS");
        var databasePath = Path.Combine(applicationRoot, "data", "pims.db");
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddProvider(new JsonLineFileLoggerProvider(Path.Combine(applicationRoot, "logs")));
        });
        var logger = loggerFactory.CreateLogger("Sasd.Pims.Startup");

        try
        {
            var contextFactory = new PimsDbContextFactory(databasePath);
            new DatabaseMigrator(contextFactory).MigrateAsync().GetAwaiter().GetResult();
            var repository = new SqliteProjectRepository(contextFactory);
            var failureHandler = new OperationFailureHandler(
                loggerFactory.CreateLogger<OperationFailureHandler>());
            var mainForm = new MainForm(
                new CreateProject(repository, TimeProvider.System, failureHandler),
                new LoadProject(repository, failureHandler),
                new ListProjects(repository, failureHandler));

            System.Windows.Forms.Application.ThreadException += (_, eventArgs) =>
                ReportUnhandled(logger, eventArgs.Exception);
            TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
            {
                ReportUnhandled(logger, eventArgs.Exception);
                eventArgs.SetObserved();
            };

            LogApplicationStarted(logger, null);
            System.Windows.Forms.Application.Run(mainForm);
            LogApplicationStopped(logger, null);
        }
        catch (Exception exception)
        {
            var errorId = ReportUnhandled(logger, exception);
            MessageBox.Show(
                $"SASD PIMS could not start. Error ID: {errorId}",
                "SASD PIMS",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static string ReportUnhandled(ILogger logger, Exception exception)
    {
        var errorId = Guid.NewGuid().ToString("N");
        LogUnhandledFailure(logger, errorId, exception);
        return errorId;
    }
}
