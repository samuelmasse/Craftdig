namespace Crafthoe.Server;

[Dimension]
public class DimensionForgetChunkReceiver(DimensionForgottenChunks forgottenChunks) : DimensionReceiver<ForgetChunkCommand>
{
    public override void Receive(NetSocket ns, ForgetChunkCommand cmd)
    {
        forgottenChunks.Add(
            ns.Ent,
            cmd.Cloc);
    }
}
