namespace Craftdig;

[Dimension]
public class DimensionSingleplayerEnterWorldAction(
    RootState state,
    WorldSingleplayerUnloadAction unload,
    DimensionScope scope,
    InjectorScopeGraph graph)
{
    public void Run(EntMutIdx playerEnt)
    {
        graph.Scope<PlayerScope>(
                scope,
                "Singleplayer player")
            .With(new PlayerEnt(playerEnt))
            .Run(unload.Attach)
            .Run(x => x.Get<PlayerMetrics>().Start())
            .Run(x => state.Current = x.New<PlayerSingleplayerState>());
    }
}
