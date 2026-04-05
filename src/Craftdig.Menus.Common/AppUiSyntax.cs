namespace Craftdig.Menus.Common;

public static class AppUiSyntax
{
    public static EntMutator<EntMut> NodeSR(EntMut parent)
    {
        var root = parent.StackRootFV.Resolve();
        return NodeS(root).StackRootV(root);
    }

    public static EntMut NodeStackPopR(EntMut parent) => NodeStackPop(parent.StackRootFV.Resolve());
}
