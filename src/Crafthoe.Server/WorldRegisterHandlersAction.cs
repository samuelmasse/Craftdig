namespace Crafthoe.Server;

[World]
public class WorldRegisterHandlersAction(NetLoop netLoop, NetEcho netEcho)
{
    public void Run()
    {
        netLoop.Register(NetEcho.Type, netEcho.Receive);
    }
}
