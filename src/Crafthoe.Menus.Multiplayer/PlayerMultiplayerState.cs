namespace Crafthoe.Menus.Multiplayer;

[Player]
public class PlayerMultiplayerState(
    RootState state,
    WorldTick tick,
    PlayerScope scope,
    PlayerSocket socket,
    PlayerFrontend player,
    PlayerCommonState commonState,
    PlayerMultiplayerDisconnectAction multiplayerDisconnectAction,
    PlayerClient client) : State
{
    public override void Load()
    {
        commonState.Load();
    }

    public override void Unload()
    {
        commonState.Unload();
        multiplayerDisconnectAction.Run();
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

        if (!socket.Connected)
            state.Current = scope.New<PlayerMultiplayerDisconnectedState>();
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
