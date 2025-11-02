namespace Crafthoe.Server;

[World]
public class WorldForgetChunkReceiver
{
    public void Receive(NetSocket ns, NetMessage msg)
    {
        ns.Ent.DimensionScope().Get<DimensionForgottenChunks>().Add(
            ns.Ent,
            MemoryMarshal.AsRef<Vector2i>(msg.Data));
    }
}
