namespace Craftdig.Menus.Singleplayer;

[Module]
public class ModuleSingleplayerWorldSelectMenu(
    RootUiMouse mouse,
    RootBin bin,
    RootGlw gl,
    RootPngs pngs,
    AppStyle s,
    AppPaths paths,
    ModuleSingleplayerLoadWorldAction singleplayerLoadWorldAction,
    ModuleReadWorldMetaAction readWorldMetaAction,
    ModuleReadWorldStateAction readWorldStateAction,
    ModuleSingleplayerNewWorldMenu newWorldMenu,
    ModuleSingleplayerDeleteWorldMenu deleteWorldMenu)

{
    public void Create(EntMut root)
    {
        var worlds = new List<(string, WorldPaths, WorldMeta, WorldState)>();
        Directory.CreateDirectory(paths.SavePath);
        var dirs = Directory.GetDirectories(paths.SavePath);

        foreach (var dir in dirs)
        {
            try
            {
                var paths = new WorldPaths(dir);
                var meta = readWorldMetaAction.Read(paths);
                var state = readWorldStateAction.Read(paths);
                worlds.Add((dir, paths, meta, state));
            }
            catch { }
        }

        worlds.Sort((a, b) => b.Item4.LastPlayed.CompareTo(a.Item4.LastPlayed));
        bool[] filtered = new bool[worlds.Count];

        WorldMeta? selected = null;

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
                    .OnTextUpdated(() =>
                    {
                        if (search.Length == 0)
                        {
                            Array.Clear(filtered);
                            return;
                        }

                        var term = search.ToString();
                        for (int i = 0; i < worlds.Count; i++)
                        {
                            var (dir, paths, meta, state) = worlds[i];
                            filtered[i] = !meta.Name.Contains(term, StringComparison.InvariantCultureIgnoreCase);
                        }
                    });
            }
        }

        Node(root, out var middle)
            .SizeRelativeV((1, 1))
            .SizeV((0, -s.BarHeight * 2))
            .OffsetV((0, s.BarHeight));
        {
            float scroll = 0;

            Node(middle, out var select)
                .Mutate(s.VerticalList)
                .InnerScrollOffsetF(() => (0, -scroll))
                .SizeInnerMaxRelativeV((1, 0))
                .AlignmentV(Alignment.Horizontal);
            for (int i = 0; i < worlds.Count; i++)
            {
                var (dir, paths, meta, state) = worlds[i];
                var itemHeight = s.ItemHeight * 1.5f;
                int index = i;

                Node(select, out var item)
                    .Mutate(s.SelectorItem)
                    .SizeRelativeV((0, 0))
                    .SizeV((s.ItemWidthL * 1.7f, itemHeight + s.ItemSpacingS * 2))
                    .OnFocusF(() => selected = meta)
                    .OnUnselectF(() => selected = null)
                    .OnDoubleClickF(() => singleplayerLoadWorldAction.Run(paths))
                    .FocusGroupV(select)
                    .IsDisabledF(() => filtered[index]);
                {
                    Node(item, out var itemContainer)
                        .SizeRelativeV((1, 1))
                        .PaddingV((s.ItemSpacingS, s.ItemSpacingS, s.ItemSpacingS, s.ItemSpacingS));
                    {
                        var screenshotFile = Path.Join(dir, "Screenshot.png");
                        ScreenshotTexture? screenshot = null;

                        if (File.Exists(screenshotFile))
                        {
                            var image = pngs[screenshotFile];
                            screenshot = new(bin, new Texture2D(gl, image.Size)
                            {
                                PixelsMipmap = image.Pixels.Span,
                                MagFilter = TextureMagFilter.Linear,
                                MinFilter = TextureMinFilter.LinearMipmapLinear
                            });
                        }

                        Node(itemContainer, out var itemIcon)
                            .Mutate(s.PointingCursor)
                            .IsSelectableV(true)
                            .SizeRelativeV((0, 0))
                            .SizeV((itemHeight, itemHeight))
                            .ColorV((0.2f, 0, 0.6f, 1))
                            .TextureF(() => screenshot?.Texture)
                            .OnPressF(() => singleplayerLoadWorldAction.Run(paths));
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
                                .TextV(meta.Name);

                            Node(itemList)
                                .Mutate(s.Label)
                                .TextColorV(s.TextColorFaint)
                                .TextV($"{Path.GetFileName(dir)!} ({state.LastPlayed.ToLocalTime():yyyy-MM-dd HH 'h' mm})");

                            Node(itemList)
                                .Mutate(s.Label)
                                .TextColorV(s.TextColorFaint)
                                .TextV(meta.GameMode.Name);
                        }
                    }
                }
            }

            Node(middle, out var scrollBar)
                .SizeV((20, 0))
                .SizeRelativeV((0, 1))
                .OffsetV((s.ItemWidthL - s.ItemSpacingXXL - s.ItemSpacingS, 0))
                .AlignmentV(Alignment.Center)
                .ColorV((0, 1, 1, 1))
                .IsDisabledF(() => middle.SizeR.Y / select.SizeInnerSumR.Y >= 1);
            {
                Vector2 pressPoint = default;
                float pressScroll = default;

                Node(scrollBar, out var scrollPuck)
                    .IsSelectableV(true)
                    .IsSilentFocusableV(true)
                    .DeferFocusV(select)
                    .CursorF(() => scrollPuck.IsPressedR ? MouseCursor.ResizeNS : MouseCursor.PointingHand)
                    .ColorF(() => scrollPuck.IsPressedR ? (1, 1, 0, 1) : (0.5f, 0.5f, 1, 1))
                    .OffsetF(() => (0, scroll / select.SizeInnerSumR.Y) * scrollBar.SizeR)
                    .SizeRelativeF(() => (1, Math.Min(1, middle.SizeR.Y / select.SizeInnerSumR.Y)))
                    .OnPressF(() =>
                    {
                        pressScroll = scroll;
                        pressPoint = mouse.Position - scrollBar.DrawOffsetR;
                    })
                    .OnUpdateF(() =>
                    {
                        if (!scrollPuck.IsPressedR)
                            return;

                        var newPoint = mouse.Position - scrollBar.DrawOffsetR;
                        var delta = newPoint.Y - pressPoint.Y;
                        var deltaRelative = delta / scrollBar.SizeR.Y;
                        var deltaTranslated = deltaRelative * select.SizeInnerSumR.Y;
                        scroll = pressScroll + deltaTranslated;
                    });
            }

            Node(middle)
                .SizeRelativeV((1, 1))
                .IsScrollableV(true)
                .OnScrollF((wheel) => scroll += -wheel.Y * s.ScrollStep)
                .OnUpdateF(() =>
                {
                    if (scroll < 0)
                        scroll = 0;

                    var maxScroll = Math.Max(0, select.SizeInnerSumR.Y - middle.SizeR.Y);
                    if (scroll > maxScroll)
                        scroll = maxScroll;
                });
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
                        .OnPressF(() => singleplayerLoadWorldAction.Run(worlds.First(x => x.Item3 == selected).Item2));

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
                            .IsInputDisabledF(() => selected == null);

                        Node(leftButtonsHorizontal)
                            .OnPressF(() =>
                            {
                                var dir = worlds.First(x => x.Item3 == selected).Item1;

                                NodeStack(root.StackRootV).StackRootV(root.StackRootV)
                                    .Mutate(r => deleteWorldMenu.Create(r, selected!, dir));
                            })
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
                        .OnPressF(() => NodeStack(root.StackRootV).StackRootV(root.StackRootV).Mutate(newWorldMenu.Create))
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
                            .OnPressF(() =>
                            {
                                NodeStack(root.StackRootV).StackRootV(root.StackRootV)
                                    .Mutate(r => newWorldMenu.Create(r, selected));
                            })
                            .TextV("Re-Create")
                            .Mutate(s.Button)
                            .IsInputDisabledF(() => selected == null);

                        Node(rightButtonsHorizontal)
                            .OnPressF(() => root.StackRootV.NodeStack.Pop())
                            .TextV("Back")
                            .Mutate(s.Button);
                    }
                }
            }
        }
    }
}
