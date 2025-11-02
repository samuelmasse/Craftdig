namespace Crafthoe.Server;

[Dimension]
public class DimensionForgetChunkReceiver(DimensionForgottenChunks forgottenChunks) : DimensionReceiver
{
    public override void Receive(NetSocket ns, NetMessage msg)
    {
        forgottenChunks.Add(
            ns.Ent,
            MemoryMarshal.AsRef<Vector2i>(msg.Data));
    }
}
