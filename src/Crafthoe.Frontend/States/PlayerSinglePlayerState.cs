namespace Crafthoe.Frontend;

[Player]
public class PlayerSinglePlayerState(
    DimensionServerContext dimensionServer,
    PlayerCommonState commonState,
    PlayerSinglePlayerUnloadWorldAction singlePlayerUnloadWorldAction) : State
{
    public override void Load()
    {
        commonState.Load();
    }

    public override void Unload()
    {
        commonState.Unload();
        singlePlayerUnloadWorldAction.Run();
    }

    public override void Update(double time)
    {
        commonState.Update(time);
    }

    public override void Render()
    {
        dimensionServer.Frame();
        commonState.Render();
    }

    public override void Draw()
    {
        commonState.Draw();
    }
}
