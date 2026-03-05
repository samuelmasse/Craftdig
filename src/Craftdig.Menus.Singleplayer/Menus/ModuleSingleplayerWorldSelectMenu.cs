namespace Craftdig.Menus.Singleplayer;

[Module]
public class ModuleSingleplayerWorldSelectMenu(
    AppStyle s,
    AppPaths paths,
    ModuleSingleplayerLoadWorldAction singleplayerLoadWorldAction,
    ModuleReadWorldMetaAction readWorldMetaAction,
    ModuleSingleplayerNewWorldMenu newWorldMenu)
{
    public void Create(EntObj root)
    {
        Node(root, out var topBar)
            .SizeRelativeV(s.Horizontal)
            .SizeV((0, s.BarHeight))
            .ColorV(s.BoardColor);

        Node(root, out var middle)
            .SizeRelativeV((1, 1))
            .SizeV((0, -s.BarHeight * 2))
            .OffsetV((0, s.BarHeight));
        {
            var worlds = new List<(WorldPaths Paths, WorldMeta Meta)>();
            Directory.CreateDirectory(paths.SavePath);
            var dirs = Directory.GetDirectories(paths.SavePath);

            foreach (var dir in dirs)
            {
                try
                {
                    var paths = new WorldPaths(dir);
                    var meta = readWorldMetaAction.Read(paths);
                    worlds.Add((paths, meta));
                }
                catch { }
            }

            Node(middle, out var select)
                .Mutate(s.VerticalList)
                .SizeInnerMaxRelativeV((1, 0))
                .InnerSpacingV(s.ItemSpacing)
                .AlignmentV(Alignment.Horizontal);
            foreach (var (paths, meta) in worlds)
            {
                Node(select)
                    .Mutate(s.Button)
                    .SizeV((s.ItemWidthL, s.ItemHeight))
                    .TextV(meta.Name)
                    .TooltipV(Path.GetFileName(paths.Root))
                    .OnPressF(() => singleplayerLoadWorldAction.Run(paths));
            }
        }

        Node(root, out var bottomBar)
            .SizeRelativeV(s.Horizontal)
            .SizeV((0, s.BarHeight))
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
                Node(buttonsList, out var leftButtonsVertical)
                    .Mutate(s.VerticalList)
                    .SizeV((s.ItemWidthL, 0))
                    .InnerSpacingV(s.ItemSpacing);
                {
                    Node(leftButtonsVertical)
                        .TextV("Play Selected World")
                        .Mutate(s.Button)
                        .IsInputDisabledV(true);

                    Node(leftButtonsVertical, out var leftButtonsHorizontal)
                        .SizeRelativeV(s.Horizontal)
                        .SizeInnerMaxRelativeV(s.Vertical)
                        .InnerSpacingV(s.ItemSpacing)
                        .InnerLayoutV(InnerLayout.HorizontalList)
                        .InnerSizingV(InnerSizing.HorizontalWeight);
                    {
                        Node(leftButtonsHorizontal)
                            .TextV("Edit")
                            .Mutate(s.Button)
                            .IsInputDisabledV(true);

                        Node(leftButtonsHorizontal)
                            .TextV("Delete")
                            .Mutate(s.Button)
                            .IsInputDisabledV(true);
                    }
                }

                Node(buttonsList, out var rightButtonsVertical)
                    .Mutate(s.VerticalList)
                    .SizeV((s.ItemWidthL, 0))
                    .InnerSpacingV(s.ItemSpacing);
                {
                    Node(rightButtonsVertical)
                        .OnPressF(() => root.StackRootV?.NodeStack.Push(
                            new EntObj() { StackRootV = root.StackRootV}.Mutate(newWorldMenu.Create)))
                        .TextV("Create New World")
                        .Mutate(s.Button);

                    Node(rightButtonsVertical, out var rightButtonsHorizontal)
                        .SizeRelativeV(s.Horizontal)
                        .SizeInnerMaxRelativeV(s.Vertical)
                        .InnerSpacingV(s.ItemSpacing)
                        .InnerLayoutV(InnerLayout.HorizontalList)
                        .InnerSizingV(InnerSizing.HorizontalWeight);
                    {
                        Node(rightButtonsHorizontal)
                            .TextV("Re-Create")
                            .Mutate(s.Button)
                            .IsInputDisabledV(true);

                        Node(rightButtonsHorizontal)
                            .OnPressF(() => root.StackRootV?.NodeStack.Pop())
                            .TextV("Back")
                            .Mutate(s.Button);
                    }
                }
            }
        }
    }
}
