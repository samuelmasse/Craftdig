namespace Craftdig;

[Dimension]
public class DimensionSingleplayerWorldLoadingMenu(
    AppRenderDistance renderDistance,
    AppTerrainLoadingMenu terrainLoadingMenu,
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
        playerEnt.IsLoaded = true;

        int radius = Math.Min(10, renderDistance.Far / 2);
        var centerCloc = playerEnt.Position.ToLoc().Xy.ToCloc();
        var window = new TerrainLoadWindow(centerCloc, radius * 2 + 1);

        terrainLoadingMenu.Create(root, window, ChunkState);
        root.Mutate().OnFrameF(() =>
        {
            dimensionBackend.Frame();
            dimensionFrontend.Frame();
            dimension.Frame();

            int loaded = 0;
            for (int i = 0; i < window.Count; i++)
            {
                if (chunks.Contains(window[i]))
                    loaded++;
            }

            if (loaded == window.Count)
                enterWorldAction.Run(playerEnt);
        });

        TerrainChunkLoadingState ChunkState(Vec2i cloc)
        {
            if (chunks.Contains(cloc))
                return TerrainChunkLoadingState.Ready;
            if (chunkPending.Contains(cloc))
                return TerrainChunkLoadingState.Pending;
            return TerrainChunkLoadingState.Missing;
        }
    }
}
