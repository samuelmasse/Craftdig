namespace Craftdig.World.Server;

[World]
public class WorldEntSyncResolver(WorldModuleIndices moduleIndices) : IEntSyncWriteResolver
{
    public int ModuleIndex(Ent ent) => moduleIndices[ent];

    public Guid EntId(EntMutIdx ent) => ent.Id;
}
