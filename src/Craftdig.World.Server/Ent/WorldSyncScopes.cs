namespace Craftdig;

[World]
public class WorldSyncScopes
{
    private uint next = 1;

    public uint Take() => next++;
}
