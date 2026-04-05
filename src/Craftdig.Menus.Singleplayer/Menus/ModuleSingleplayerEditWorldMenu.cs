namespace Craftdig.Menus.Singleplayer;

[Module]
public class ModuleSingleplayerEditWorldMenu(
    AppStyle s,
    ModuleScope scope,
    ModuleWriteWorldMetaAction writeWorldMetaAction)
{
    public void Create(EntMut root, WorldEntry world)
    {
        var name = new StringBuilder(world.Meta.Name);

        Node(root, out var form)
            .Mutate(s.VerticalList)
            .OffsetV((0, s.ItemHeight))
            .SizeV((s.ItemWidth * 2, 0))
            .InnerSpacingV(s.ItemSpacing)
            .AlignmentV(Alignment.Horizontal);
        {
            Node(form)
                .Mutate(s.Label)
                .TextV("World Name");

            Node(form)
                .Mutate(s.Textbox)
                .MaxLengthV(29)
                .StringBuilderV(name)
                .IsInitialFocusV(true);
        }

        Node(root, out var bottomBar)
            .SizeRelativeV(s.Horizontal)
            .SizeV((0, s.BarHeight - s.ItemHeight))
            .AlignmentV(Alignment.Horizontal | Alignment.Bottom)
            .ColorV(s.BoardColor);
        {
            Node(bottomBar, out var buttonsList)
                .Mutate(s.HorizontalList)
                .AlignmentV(Alignment.Center)
                .OffsetMultiplierV(s.ItemSpacingXS)
                .SizeInnerMaxRelativeV(s.Vertical)
                .InnerSpacingV(s.ItemSpacingL)
                .ColorV(s.BoardColor2);
            {
                Node(buttonsList)
                    .OnPressF(() =>
                    {
                        var worldName = name.ToString();
                        if (string.IsNullOrEmpty(worldName))
                            worldName = world.Meta.Name;

                        writeWorldMetaAction.Write(world.Meta with { Name = worldName }, world.Paths);
                        NodeStackPop(root.StackRootFV.Resolve());
                        NodeStackPop(root.StackRootFV.Resolve());
                        NodeS(root.StackRootFV.Resolve()).StackRootV(root.StackRootFV.Resolve())
                            .Mutate(scope.Get<ModuleSingleplayerWorldSelectMenu>().Create);
                    })
                    .TextV("Save")
                    .Mutate(s.Button)
                    .SizeV((s.ItemWidthL, s.ItemHeight));

                Node(buttonsList)
                    .OnPressF(() => NodeStackPop(root.StackRootFV.Resolve()))
                    .TextV("Cancel")
                    .Mutate(s.Button)
                    .SizeV((s.ItemWidthL, s.ItemHeight));
            }
        }
    }
}
