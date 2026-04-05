namespace Craftdig.Menus.Singleplayer;

[Module]
public class ModuleSingleplayerWorldSelectMenu(
    RootUiMouse mouse,
    AppStyle s,
    ModuleScope scope,
    ModuleSingleplayerLoadWorldAction singleplayerLoadWorldAction,
    ModuleSingleplayerListWorldsAction listWorldsAction,
    ModuleSingleplayerNewWorldMenu newWorldMenu,
    ModuleSingleplayerDeleteWorldMenu deleteWorldMenu,
    ModuleSingleplayerEditWorldMenu editWorldMenu)

{
    public void Create(EntMut root)
    {
        var worlds = listWorldsAction.Run();
        bool[] filtered = new bool[worlds.Count];
        WorldEntry? selected = null;

        Node(root, out var topBar)
            .SizeRelativeV(s.Horizontal)
            .SizeV((0, s.BarHeight))
            .ColorV(s.BoardColor);
        {
            Node(topBar, out var topBarHor)
                .Mutate(s.VerticalList)
                .InnerSpacingV(s.ItemSpacing)
                .SizeInnerMaxRelativeV((1, 0))
                .AlignmentV(Alignment.Center);
            {
                Node(topBarHor)
                    .Mutate(s.Label)
                    .AlignmentV(Alignment.Horizontal)
                    .TextV("Select World");

                var search = new StringBuilder(string.Empty);

                Node(topBarHor)
                    .Mutate(s.Textbox)
                    .MaxLengthV(29)
                    .SizeV((s.ItemWidthL * 1.4f, s.ItemHeight))
                    .StringBuilderV(search)
                    .IsInitialFocusV(true)
                    .OnTextUpdatedF(() =>
                    {
                        if (search.Length == 0)
                        {
                            Array.Clear(filtered);
                            return;
                        }

                        var term = search.ToString();
                        for (int i = 0; i < worlds.Count; i++)
                            filtered[i] = !worlds[i].Meta.Name.Contains(term, StringComparison.InvariantCultureIgnoreCase);
                    });
            }
        }

        Node(root, out var middle)
            .SizeRelativeV((1, 1))
            .SizeV((0, -s.BarHeight * 2))
            .OffsetV((0, s.BarHeight));
        {
            var screenshots = scope.New<ModuleWorldScreenshots>();

            s.Selector(middle, mouse, out var select);
            select.Mutate().OnFrameF(() => screenshots.RefillBucket());

            for (int i = 0; i < worlds.Count; i++)
            {
                var world = worlds[i];
                var itemHeight = s.ItemHeight * 1.5f;
                int index = i;

                Node(select, out var item)
                    .Mutate(s.SelectorItem)
                    .SizeRelativeV((0, 0))
                    .SizeV((s.ItemWidthL * 1.7f, itemHeight + s.ItemSpacingS * 2))
                    .OnFocusF(() => selected = world)
                    .OnUnselectF(() => selected = null)
                    .OnDoubleClickF(() => singleplayerLoadWorldAction.Run(world.Paths))
                    .FocusGroupV(select)
                    .IsDisabledF(() => filtered[index]);
                {
                    Node(item, out var itemContainer)
                        .SizeRelativeV((1, 1))
                        .PaddingV((s.ItemSpacingS, s.ItemSpacingS, s.ItemSpacingS, s.ItemSpacingS));
                    {
                        Node(itemContainer, out var itemIcon)
                            .Mutate(s.PointingCursor)
                            .IsSelectableV(true)
                            .SizeRelativeV((0, 0))
                            .SizeV((itemHeight, itemHeight))
                            .ColorV((0.2f, 0, 0.6f, 1))
                            .TextureF(() => screenshots[world.Dir]?.Texture)
                            .OnPressF(() => singleplayerLoadWorldAction.Run(world.Paths));
                        {
                            Node(itemIcon)
                                .ColorV((1, 1, 1, 0.5f))
                                .IsDisabledF(() => !itemIcon.IsHoveredR && !item.IsHoveredR);
                        }

                        Node(itemContainer, out var itemList)
                            .Mutate(s.VerticalList)
                            .SizeRelativeV((1, 0))
                            .OffsetV((itemHeight + s.ItemSpacingS, 0))
                            .SizeV((-itemHeight - s.ItemSpacingS, 0))
                            .AlignmentV(Alignment.Left | Alignment.Vertical);
                        {
                            Node(itemList)
                                .Mutate(s.Label)
                                .SizeV((0, s.ItemSpacingS))
                                .TextV(world.Meta.Name);

                            Node(itemList)
                                .Mutate(s.Label)
                                .TextColorV(s.TextColorFaint)
                                .TextV($"{Path.GetFileName(world.Dir)!} ({world.State.LastPlayed.ToLocalTime():yyyy-MM-dd HH 'h' mm})");

                            Node(itemList)
                                .Mutate(s.Label)
                                .TextColorV(s.TextColorFaint)
                                .TextV(world.Meta.GameMode.Name);
                        }
                    }
                }
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
                        .IsInputDisabledF(() => selected == null)
                        .OnPressF(() => singleplayerLoadWorldAction.Run(selected!.Paths));

                    Node(leftButtonsVertical, out var leftButtonsHorizontal)
                        .SizeRelativeV(s.Horizontal)
                        .SizeInnerMaxRelativeV(s.Vertical)
                        .InnerSpacingV(s.ItemSpacing)
                        .InnerLayoutV(InnerLayout.HorizontalList)
                        .InnerSizingV(InnerSizing.HorizontalWeight);
                    {
                        Node(leftButtonsHorizontal)
                            .OnPressF(() => NodeSR(root).Mutate(r => editWorldMenu.Create(r, selected!)))
                            .TextV("Edit")
                            .Mutate(s.Button)
                            .IsInputDisabledF(() => selected == null);

                        Node(leftButtonsHorizontal)
                            .OnPressF(() => NodeSR(root).Mutate(r => deleteWorldMenu.Create(r, selected!)))
                            .TextV("Delete")
                            .Mutate(s.Button)
                            .IsInputDisabledF(() => selected == null);
                    }
                }

                Node(buttonsList, out var rightButtonsVertical)
                    .Mutate(s.VerticalList)
                    .SizeV((s.ItemWidthL, 0))
                    .InnerSpacingV(s.ItemSpacing);
                {
                    Node(rightButtonsVertical)
                        .OnPressF(() => NodeSR(root).Mutate(newWorldMenu.Create))
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
                            .OnPressF(() => NodeSR(root).Mutate(r => newWorldMenu.Create(r, selected!.Meta)))
                            .TextV("Re-Create")
                            .Mutate(s.Button)
                            .IsInputDisabledF(() => selected == null);

                        Node(rightButtonsHorizontal)
                            .OnPressF(() => NodeStackPopR(root))
                            .TextV("Back")
                            .Mutate(s.Button);
                    }
                }
            }
        }
    }
}
