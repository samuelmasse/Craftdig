namespace Craftdig;

[Dimension]
public class DimensionForgetSectionReceiver(DimensionForgottenSections forgottenSections) : DimensionReceiver<ForgetSectionCommand>
{
    public override void Receive(NetSocket ns, ForgetSectionCommand cmd)
    {
        forgottenSections.Add(
            ns,
            cmd.Sloc);
    }
}
