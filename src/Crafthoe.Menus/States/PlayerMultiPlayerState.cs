namespace Crafthoe.Menus;

[Player]
public class PlayerMultiPlayerState(
    WorldTick tick,
    PlayerFrontend player,
    PlayerCommonState commonState,
    PlayerMultiPlayerDisconnectAction multiPlayerDisconnectAction,
    PlayerClient client) : State
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

        int ticks = tick.Update(time);
        while (ticks > 0)
        {
            if (!commonState.Inv)
                player.Tick();

            client.Tick();

            ticks--;
        }

        if (!commonState.Inv)
            player.Update(time);
    }

    public override void Render()
    {
        client.Frame();
        commonState.Render();
    }

    public override void Draw()
    {
        commonState.Draw();
    }
}
