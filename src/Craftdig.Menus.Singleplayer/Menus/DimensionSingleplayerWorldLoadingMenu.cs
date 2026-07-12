namespace Craftdig.Menus.Singleplayer;

[Dimension]
public class DimensionSingleplayerWorldLoadingMenu(
    AppStyle s,
    AppRenderDistance renderDistance,
    DimensionSingleplayerFindPlayerAction findPlayerAction,
    DimensionSingleplayerEnterWorldAction enterWorldAction,
    DimensionChunks chunks,
    DimensionChunkPending chunkPending,
    DimensionBackend dimensionBackend,
    DimensionFrontend dimensionFrontend,
    DimensionContext dimension)
{
    public void Create(EntMut root)
    {
        root.IsModalFV = true;

        var playerEnt = findPlayerAction.Run();
        playerEnt.IsSeer = true;

        int radius = Math.Min(10, renderDistance.Far / 2);
        int gridSize = radius * 2 + 1;
        int totalChunks = gridSize * gridSize;

        var centerCloc = playerEnt.Position.ToLoc().XY.ToCloc();
        float progress = 0f;

        Node(root, out var form)
            .Mutate(s.VerticalList)
            .SizeInnerMaxRelativeV((1, 0))
            .AlignmentV(Alignment.Center)
            .InnerSpacingV(s.ItemSpacing)
            .OnFrameF(() =>
            {
                dimensionBackend.Frame();
                dimensionFrontend.Frame();
                dimension.Frame();

                int loaded = 0;
                for (int dx = -radius; dx <= radius; dx++)
                    for (int dy = -radius; dy <= radius; dy++)
                        if (chunks.Contains(centerCloc + (dx, dy)))
                            loaded++;

                progress = (float)loaded / totalChunks;

                if (loaded == totalChunks)
                    enterWorldAction.Run(playerEnt);
            });
        {
            Node(form)
                .Mutate(s.Label)
                .AlignmentV(Alignment.Horizontal)
                .TextV("Loading terrain...");

            Node(form, out var bar)
                .SizeRelativeV((0, 0))
                .SizeV((s.ItemWidthL, s.ItemSpacingS))
                .AlignmentV(Alignment.Horizontal)
                .ColorV((0.15f, 0.15f, 0.15f, 1f));
            {
                Node(bar)
                    .SizeRelativeF(() => (progress, 1))
                    .ColorV((0.4f, 0.8f, 0.4f, 1f));
            }

            float tileSize = 12;

            Node(form, out var grid)
                .SizeRelativeV((0, 0))
                .SizeV((tileSize * gridSize, tileSize * gridSize))
                .AlignmentV(Alignment.Horizontal)
                .ColorV((0.08f, 0.08f, 0.08f, 1f));
            {
                float padding = 1;

                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        var cloc = centerCloc + (dx, dy);

                        Node(grid)
                            .SizeRelativeV((0, 0))
                            .SizeV((tileSize - padding, tileSize - padding))
                            .OffsetV(((dx + radius) * tileSize, (dy + radius) * tileSize))
                            .ColorF(() => chunks.Contains(cloc)
                                ? (0.4f, 0.8f, 0.4f, 1f)
                                : chunkPending.Contains(cloc)
                                ? (0.8f, 0.8f, 0.4f, 1f)
                                : (0.25f, 0.25f, 0.25f, 1f));
                    }
                }
            }
        }
    }
}
