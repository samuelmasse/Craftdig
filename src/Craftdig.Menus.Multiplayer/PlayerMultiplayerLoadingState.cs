namespace Craftdig;

[Player]
public class PlayerMultiplayerLoadingState(
    RootState state,
    RootUi ui,
    RootBackbuffer backbuffer,
    AppStyle s,
    DimensionFrontend dimensionFrontend,
    PlayerScope scope,
    PlayerClient client,
    PlayerEntSync entSync,
    PlayerTerrainLoading terrainLoading,
    PlayerSocket socket,
    PlayerMultiplayerDisconnectAction disconnect,
    ModuleMainBackgroundMenu mainBackgroundMenu,
    AppTerrainLoadingMenu terrainLoadingMenu) : State
{
    private readonly EntMut menus = Node(ui);
    private EntMut loadingMenu;
    private EntMut waiting;
    private bool menuCreated;
    private bool prepared;

    public override void Load()
    {
        menus.Mutate().StackRootV(menus);
        loadingMenu = PushMenu(menus, CreateWaiting, mainBackgroundMenu.Create).Ent;
        loadingMenu.IsModalFV = true;
    }

    private void CreateWaiting(EntMut root)
    {
        waiting = Node(root)
            .Mutate(s.Label)
            .AlignmentV(Alignment.Center)
            .TextV("Loading terrain...");
    }

    public override void Unload() => NodesRemove(ui, menus);

    public override void Frame(double _)
    {
        terrainLoading.Frame();
        client.Sync();
        if (!socket.Connected)
        {
            var disconnected = scope.New<PlayerMultiplayerDisconnectedState>();
            disconnect.Run();
            state.Current = disconnected;
            return;
        }

        if (prepared)
        {
            state.Current = scope.New<PlayerMultiplayerState>();
            return;
        }

        if (terrainLoading.Begun && !menuCreated)
        {
            NodesRemove(loadingMenu, waiting);
            terrainLoadingMenu.Create(loadingMenu, terrainLoading.Window, ChunkState);
            menuCreated = true;
        }
    }

    public override void Render()
    {
        backbuffer.Clear();
        client.Frame();

        if (terrainLoading.Complete && entSync.OwnerReady)
            prepared = client.Prepare();

        dimensionFrontend.Frame();
    }

    private TerrainChunkLoadingState ChunkState(Vec2i cloc)
    {
        if (terrainLoading.IsReady(cloc))
            return TerrainChunkLoadingState.Ready;
        if (terrainLoading.IsPending(cloc))
            return TerrainChunkLoadingState.Pending;
        return TerrainChunkLoadingState.Missing;
    }
}
