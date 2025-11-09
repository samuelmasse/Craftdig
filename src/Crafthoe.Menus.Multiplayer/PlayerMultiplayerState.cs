namespace Crafthoe.Menus.Multiplayer;

[Player]
public class PlayerMultiplayerState(
    RootState state,
    WorldTick tick,
    DimensionContext context,
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
            else player.NoTick();

            client.Stream();
            context.Tick();
            client.Tick();

            ticks--;
        }

        player.Update(time);
        if (!commonState.Inv)
            player.Input();

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
