using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Sasd.Pims.Infrastructure.Diagnostics;

public sealed class JsonLineFileLoggerProvider : ILoggerProvider
{
    private readonly string _logDirectory;
    private readonly TimeProvider _timeProvider;
    private readonly object _writeLock = new();

    public JsonLineFileLoggerProvider(string logDirectory, TimeProvider? timeProvider = null)
    {
        _logDirectory = Path.GetFullPath(logDirectory);
        _timeProvider = timeProvider ?? TimeProvider.System;
        Directory.CreateDirectory(_logDirectory);
        DeleteExpiredLogs();
    }

    public ILogger CreateLogger(string categoryName) => new JsonLineFileLogger(this, categoryName);

    public void Dispose()
    {
    }

    private void Write(string category, LogLevel level, EventId eventId, string message, Exception? exception)
    {
        var now = _timeProvider.GetUtcNow();
        var entry = new
        {
            timestampUtc = now,
            level = level.ToString(),
            category,
            eventId = eventId.Id,
            eventName = eventId.Name,
            message,
            exceptionType = exception?.GetType().FullName,
        };
        var line = JsonSerializer.Serialize(entry);
        var filePath = Path.Combine(_logDirectory, $"pims-{now:yyyyMMdd}.jsonl");
        lock (_writeLock)
        {
            File.AppendAllText(filePath, line + Environment.NewLine);
        }
    }

    private void DeleteExpiredLogs()
    {
        var threshold = _timeProvider.GetUtcNow().UtcDateTime.AddDays(-14);
        foreach (var path in Directory.EnumerateFiles(_logDirectory, "pims-*.jsonl"))
        {
            if (File.GetLastWriteTimeUtc(path) < threshold)
            {
                File.Delete(path);
            }
        }
    }

    private sealed class JsonLineFileLogger(JsonLineFileLoggerProvider provider, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                provider.Write(category, logLevel, eventId, formatter(state, exception), exception);
            }
        }
    }
}
