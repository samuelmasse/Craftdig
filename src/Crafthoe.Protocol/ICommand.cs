namespace Crafthoe.Protocol;

public interface ICommand
{
    static abstract int CommandId { get; }
}
