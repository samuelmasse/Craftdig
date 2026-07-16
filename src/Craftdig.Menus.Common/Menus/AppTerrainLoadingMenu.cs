namespace Craftdig.Menus.Common;

[App]
public class AppTerrainLoadingMenu(AppStyle s)
{
    public void Create(
        EntMut root,
        TerrainLoadWindow window,
        Func<Vec2i, TerrainChunkLoadingState> chunkState)
    {
        float progress = 0;

        Node(root, out var form)
            .Mutate(s.VerticalList)
            .SizeInnerMaxRelativeV((1, 0))
            .AlignmentV(Alignment.Center)
            .InnerSpacingV(s.ItemSpacing)
            .OnFrameF(() => progress = (float)CountReady() / window.Count);
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

            const float tileSize = 12;

            Node(form, out var grid)
                .SizeRelativeV((0, 0))
                .SizeV((tileSize * window.SideLength, tileSize * window.SideLength))
                .AlignmentV(Alignment.Horizontal)
                .ColorV((0.08f, 0.08f, 0.08f, 1f));
            {
                const float padding = 1;

                for (int i = 0; i < window.Count; i++)
                {
                    int x = i % window.SideLength;
                    int y = i / window.SideLength;
                    var cloc = window[i];

                    Node(grid)
                        .SizeRelativeV((0, 0))
                        .SizeV((tileSize - padding, tileSize - padding))
                        .OffsetV((x * tileSize, y * tileSize))
                        .ColorF(() => chunkState(cloc) switch
                        {
                            TerrainChunkLoadingState.Ready => (0.4f, 0.8f, 0.4f, 1f),
                            TerrainChunkLoadingState.Pending => (0.8f, 0.8f, 0.4f, 1f),
                            _ => (0.25f, 0.25f, 0.25f, 1f),
                        });
                }
            }
        }

        int CountReady()
        {
            int ready = 0;
            for (int i = 0; i < window.Count; i++)
            {
                if (chunkState(window[i]) == TerrainChunkLoadingState.Ready)
                    ready++;
            }

            return ready;
        }
    }
}
