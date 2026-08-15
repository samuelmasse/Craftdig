namespace Craftdig;

public interface IEntSyncWriteResolver
{
    int ModuleIndex(Ent ent);
    Guid EntId(EntMutIdx ent);
}

public interface IEntSyncReadResolver
{
    Ent ModuleEnt(int index);
    EntMutIdx Ent(Guid id);
}
