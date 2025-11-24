namespace Crafthoe.App;

public readonly record struct LogBufferEntry(LogEntry Entry, ReadOnlyMemory<char> Chars);
