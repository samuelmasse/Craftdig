namespace Craftdig;

public static class AppUiSyntax
{
    public static EntMutator<EntMut> PushMenu(EntMut parent, Action<EntMut> action)
    {
        var root = parent.StackRootFV.Resolve();
        return NodeS(root).StackRootV(root).MenuOriginF(action).Mutate(action);
    }

    public static EntMutator<EntMut> PushMenu(EntMut parent, Action<EntMut> action, Action<EntMut> companion)
    {
        var root = parent.StackRootFV.Resolve();
        return NodeS(root)
            .CompanionV(NodeC(parent).Mutate(companion))
            .CompanionOriginF(companion)
            .StackRootV(root)
            .MenuOriginF(action)
            .Mutate(action);
    }

    public static EntMut PopMenu(EntMut parent)
    {
        return NodeStackPop(parent.StackRootFV.Resolve());
    }

    public static EntMutator<EntMut> RefreshMenu(EntMut parent)
    {
        var top = PopMenu(parent);
        var companionOrigin = top.CompanionOriginFV.Resolve();
        if (companionOrigin != null)
            return PushMenu(parent, top.MenuOriginFV.Resolve(), companionOrigin);
        return PushMenu(parent, top.MenuOriginFV.Resolve());
    }
}
