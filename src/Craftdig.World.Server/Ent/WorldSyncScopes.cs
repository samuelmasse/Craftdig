namespace Craftdig.World.Server;

[World]
public class WorldSyncScopes
{
    private uint next = 1;

    public uint Take() => next++;
}
