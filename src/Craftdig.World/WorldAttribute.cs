namespace Craftdig.World;

[AttributeUsage(AttributeTargets.Property)]
public class SavedAttribute : Attribute;

public enum EntSyncAudience : byte
{
    Everyone,
    Owner,
    Observers
}

[AttributeUsage(AttributeTargets.Property)]
public class SyncedAttribute(
    EntSyncAudience audience = EntSyncAudience.Everyone,
    int maximumCount = 0) : Attribute
{
    public EntSyncAudience Audience { get; } = audience;
    public int MaximumCount { get; } = maximumCount;
}
