namespace Craftdig.Menus.Singleplayer;

[Dimension]
public class DimensionSingleplayerEnterWorldAction(
    RootState state,
    DimensionScope scope)
{
    public void Run(EntMutIdx playerEnt)
    {
        scope.Scope<PlayerScope>()
            .With(new PlayerEnt(playerEnt))
            .Run(x => x.Get<PlayerMetrics>().Start())
            .Run(x => state.Current = x.New<PlayerSingleplayerState>());
    }
}
