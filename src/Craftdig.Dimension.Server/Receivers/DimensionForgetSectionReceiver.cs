namespace Craftdig.Server;

[Dimension]
public class DimensionForgetSectionReceiver(DimensionForgottenSections forgottenSections) : DimensionReceiver<ForgetSectionCommand>
{
    public override void Receive(NetSocket ns, ForgetSectionCommand cmd)
    {
        forgottenSections.Add(
            ns.Ent,
            cmd.Sloc);
    }
}
