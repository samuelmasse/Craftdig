namespace Craftdig.App;

public readonly record struct LogEntry(DateTime Time, LogLevel Level, string? File, int? Line);
