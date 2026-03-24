namespace Craftdig.World.Server;

[Components]
public interface IWorldServerComponents
{
    // Socket
    EntPtrIdx SocketWorldPlayer { get; set; }
    int PlayerSlot { get; set; }

    // Scratched
    bool IsScratched { get; set; }
    ulong[] ScratchedComponents { get; set; }
}
