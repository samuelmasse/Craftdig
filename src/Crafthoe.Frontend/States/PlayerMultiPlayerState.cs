namespace Crafthoe.Frontend;

[Player]
public class PlayerMultiPlayerState(
    PlayerCommonState commonState,
    PlayerMultiPlayerDisconnectAction multiPlayerDisconnectAction) : State
{
    public override void Load()
    {
        commonState.Load();
    }

    public override void Unload()
    {
        commonState.Unload();
        multiPlayerDisconnectAction.Run();
    }

    public override void Update(double time)
    {
        commonState.Update(time);
    }

    public override void Render()
    {
        commonState.Render();
    }

    public override void Draw()
    {
        commonState.Draw();
    }
}
