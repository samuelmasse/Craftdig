namespace Crafthoe.Frontend;

[Player]
public class PlayerEscapeMenu(RootScope scope, RootState state, AppStyle s, PlayerUnloadWorldAction unloadWorldAction)
{
    public EntObj Get(EntObj ui)
    {
        Node(out var menu).SizeRelativeV((1, 1));

        Node(menu, out var list)
            .SizeV((512, 0))
            .SizeInnerSumRelativeV((0, 1))
            .AlignmentV(Alignment.Horizontal)
            .InnerLayoutV(InnerLayout.VerticalList)
            .AlignmentV(Alignment.Center)
            .ColorV((1, 0.4f, 0.7f, 1));
        {
            Node(list)
                .Mut(s.Label)
                .TextV("Game Menu")
                .AlignmentV(Alignment.Horizontal);

            Node(list)
                .Mut(s.Button)
                .OnPressF(() => ui.NodeStack().Pop())
                .TextV("Back to Game");

            Node(list)
                .Mut(s.Button)
                .OnPressF(() =>
                {
                    unloadWorldAction.Run();
                    state.Current = scope.Scope<AppScope>().New<AppMenuState>();
                })
                .TextV("Quit");
        }

        return menu;
    }
}
