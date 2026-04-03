namespace Craftdig.Menus.Singleplayer;

[Module]
public class ModuleSingleplayerWorldSelectMenu(
    RootBin bin,
    RootGlw gl,
    RootPngs pngs,
    AppStyle s,
    AppPaths paths,
    ModuleSingleplayerLoadWorldAction singleplayerLoadWorldAction,
    ModuleReadWorldMetaAction readWorldMetaAction,
    ModuleReadWorldStateAction readWorldStateAction,
    ModuleSingleplayerNewWorldMenu newWorldMenu)
{
    public void Create(EntObj root)
    {
        WorldMeta? selected = null;

        Node(root, out var topBar)
            .SizeRelativeV(s.Horizontal)
            .SizeV((0, s.BarHeight))
            .ColorV(s.BoardColor);

        Node(root, out var middle)
            .SizeRelativeV((1, 1))
            .SizeV((0, -s.BarHeight * 2))
            .OffsetV((0, s.BarHeight));
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

            Node(middle, out var select)
                .Mutate(s.VerticalList)
                .SizeInnerMaxRelativeV((1, 0))
                .AlignmentV(Alignment.Horizontal);
            foreach (var (dir, paths, meta, state) in worlds)
            {
                var itemHeight = s.ItemHeight * 1.5f;

                Node(select, out var item)
                    .Mutate(s.SelectorItem)
                    .SizeRelativeV((0, 0))
                    .SizeV((s.ItemWidthL * 1.7f, itemHeight + s.ItemSpacingS * 2))
                    .OnPressF(() => selected = meta)
                    .OnDoubleClickF(() => singleplayerLoadWorldAction.Run(paths));
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
                        .IsInputDisabledF(() => selected == null);

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
                        .OnPressF(() => root.StackRootV.NodeStack.Push(
                            new EntObj() { StackRootV = root.StackRootV }.Mutate(newWorldMenu.Create)))
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
