namespace Crafthoe.Frontend;

[Player]
public class PlayerEscapeMenu(RootScope scope, RootState state, AppStyle s, PlayerUnloadWorldAction unloadWorldAction)
{
    public void Create(EntObj root)
    {
        Node(root, out var list)
            .SizeV((s.ItemWidth, 0))
            .SizeInnerSumRelativeV(s.Vertical)
            .AlignmentV(Alignment.Horizontal)
            .InnerLayoutV(InnerLayout.VerticalList)
            .InnerSpacingV(s.ItemSpacing)
            .PaddingV((s.ItemSpacing, s.ItemSpacing, s.ItemSpacing, s.ItemSpacing))
            .AlignmentV(Alignment.Center)
            .ColorV(s.BoardColor);
        {
            Node(list)
                .Mut(s.Label) 
                .TextV("Game Menu")
                .AlignmentV(Alignment.Horizontal);

            Node(list)
                .Mut(s.Button)
                .OnPressF(() => root.StackRootV().NodeStack().Pop())
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
    }
}
