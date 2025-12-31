namespace Craftdig.Protocol;

public interface ICommand
{
    static abstract ushort CommandId { get; }
}
