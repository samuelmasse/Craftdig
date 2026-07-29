namespace Craftdig.Menus.Singleplayer;

[Dimension]
public class DimensionSingleplayerEnterWorldAction(
    RootState state,
    DimensionScope scope,
    InjectorScopeGraph graph)
{
    public void Run(EntMutIdx playerEnt)
    {
        graph.Scope<PlayerScope>(
                scope,
                "Singleplayer player")
            .With(new PlayerEnt(playerEnt))
            .Run(x => x.Get<PlayerMetrics>().Start())
            .Run(x => state.Current = x.New<PlayerSingleplayerState>());
    }
}
