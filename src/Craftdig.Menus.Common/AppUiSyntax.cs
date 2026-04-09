namespace Craftdig.Menus.Common;

public static class AppUiSyntax
{
    public static EntMutator<EntMut> PushMenu(EntMut parent, Action<EntMut> action)
    {
        var root = parent.StackRootFV.Resolve();
        return NodeS(root).StackRootV(root).MenuOriginF(action).Mutate(action);
    }

    public static EntMut PopMenu(EntMut parent)
    {
        return NodeStackPop(parent.StackRootFV.Resolve());
    }

    public static EntMutator<EntMut> RefreshMenu(EntMut parent)
    {
        var top = PopMenu(parent);
        return PushMenu(parent, top.MenuOriginFV.Resolve());
    }
}
