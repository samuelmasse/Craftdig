namespace Craftdig.Protocol;

public interface IEntSyncReadValidator
{
    bool IsModuleIndexValid(int index);
    bool IsEntIdValid(Guid id);
}
