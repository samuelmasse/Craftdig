namespace Craftdig.Menus.Multiplayer;

[Player]
public class PlayerMultiplayerState(
    RootState state,
    RootKeyboard keyboard,
    WorldTick tick,
    DimensionContext context,
    PlayerScope scope,
    PlayerSocket socket,
    PlayerFrontend player,
    PlayerCommonState commonState,
    PlayerMultiplayerDisconnectAction multiplayerDisconnectAction,
    PlayerSlowTickReceiver slowTickReceiver,
    PlayerClient client,
    PlayerMultiplayerDebugMenu multiplayerDebugMenu) : State
{
    private int delay;

    public override void Load()
    {
        commonState.Load();
        Node(commonState.Menus).Mut(multiplayerDebugMenu.Create);
    }

    public override void Unload()
    {
        commonState.Unload();
        multiplayerDisconnectAction.Run();
    }

    public override void Update(double time)
    {
        commonState.Update(time);

        if (!commonState.Inv && !commonState.Paused)
            player.Input();
    }

    public override void Frame(double time)
    {
        int ticks = Math.Min(tick.Update(time), 8);
        if (keyboard.IsKeyPressed(Keys.L))
            ticks++;
        if (keyboard.IsKeyPressed(Keys.K))
            delay++;

        while (delay > 0 && ticks > 0)
        {
            ticks--;
            delay--;
        }

        while (ticks > 0 && slowTickReceiver.ShouldSlowTick())
            ticks--;

        while (ticks > 0)
        {
            if (!commonState.Inv && !commonState.Paused)
                player.Tick();
            else player.NoTick();

            client.Stream();
            context.Tick();
            client.Tick();

            ticks--;
        }

        player.Update(time);
    }

    public override void Render()
    {
        client.Frame();
        commonState.Render();

        if (!socket.Connected)
            state.Current = scope.New<PlayerMultiplayerDisconnectedState>();
    }

    public override void Draw()
    {
        commonState.Draw();
    }
}
