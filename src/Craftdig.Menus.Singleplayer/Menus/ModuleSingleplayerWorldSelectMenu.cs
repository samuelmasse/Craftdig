namespace Craftdig;

[Module]
public class ModuleSingleplayerWorldSelectMenu(
    AppStyle s,
    ModuleScope scope,
    ModuleSingleplayerPrepareWorldAction singleplayerPrepareWorldAction,
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

        void Load(WorldPaths paths) => PushMenu(root, singleplayerPrepareWorldAction.Run(paths)
            .Get<DimensionSingleplayerWorldLoadingMenu>().Create);

        Node(root, out var topBar)
            .Mutate(s.TopBar);
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
                    .TextboxMaxLengthV(29)
                    .SizeV((716, s.ItemHeight))
                    .TextboxStringBuilderV(search)
                    .IsInitialFocusV(true)
                    .TextboxOnTextUpdatedF(() =>
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
            .Mutate(s.MiddleBar);
        {
            var screenshots = scope.New<ModuleWorldScreenshots>();

            s.Selector(middle, out var select);
            select.Mutate().OnFrameF(() => screenshots.RefillBucket());

            for (int i = 0; i < worlds.Count; i++)
            {
                var world = worlds[i];
                var itemHeight = 128;
                int index = i;

                Node(select, out var item)
                    .Mutate(s.SelectorItem)
                    .SizeRelativeV((0, 0))
                    .SizeV((s.ItemWidthL * 2f, itemHeight + s.ItemSpacingS * 2))
                    .OnFocusF(() => selected = world)
                    .OnUnselectF(() => selected = null)
                    .OnDoubleClickF(() => Load(world.Paths))
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
                            .OnPressF(() => Load(world.Paths));
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
            .Mutate(s.BottomBar);
        {
            Node(bottomBar, out var buttonsList)
                .Mutate(s.ButtonBar);
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
                        .OnPressF(() => Load(selected!.Paths));

                    Node(leftButtonsVertical, out var leftButtonsHorizontal)
                        .Mutate(s.ButtonRow);
                    {
                        Node(leftButtonsHorizontal)
                            .OnPressF(() => PushMenu(root, r => editWorldMenu.Create(r, selected!)))
                            .TextV("Edit")
                            .Mutate(s.Button)
                            .IsInputDisabledF(() => selected == null);

                        Node(leftButtonsHorizontal)
                            .OnPressF(() => PushMenu(root, r => deleteWorldMenu.Create(r, selected!)))
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
                        .OnPressF(() => PushMenu(root, newWorldMenu.Create))
                        .TextV("Create New World")
                        .Mutate(s.Button);

                    Node(rightButtonsVertical, out var rightButtonsHorizontal)
                        .Mutate(s.ButtonRow);
                    {
                        Node(rightButtonsHorizontal)
                            .OnPressF(() => PushMenu(root, r => newWorldMenu.Create(r, selected!.Meta)))
                            .TextV("Re-Create")
                            .Mutate(s.Button)
                            .IsInputDisabledF(() => selected == null);

                        Node(rightButtonsHorizontal)
                            .OnPressF(() => PopMenu(root))
                            .TextV("Back")
                            .Mutate(s.Button);
                    }
                }
            }
        }
    }
}
