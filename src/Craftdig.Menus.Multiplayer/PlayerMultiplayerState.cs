namespace Craftdig;

[Player]
public class PlayerMultiplayerState(
    RootState state,
    RootKeyboard keyboard,
    ModuleWorldSession session,
    WorldTick tick,
    WorldClock clock,
    DimensionContext context,
    PlayerScope scope,
    PlayerSocket socket,
    PlayerEnt ent,
    PlayerCamera camera,
    PlayerFrontend player,
    PlayerCommonState commonState,
    PlayerIdentityCache identityCache,
    PlayerSlowTickReceiver slowTickReceiver,
    PlayerClient client,
    PlayerMultiplayerDebugMenu multiplayerDebugMenu,
    PlayerMultiplayerNameplates nameplates,
    PlayerMultiplayerRosterMenu multiplayerRosterMenu) : State
{
    private int delay;

    public override void Load()
    {
        commonState.Load();

        camera.SetLookAt(ent.LookAt.Swizzle());
        var movement = ent.Movement;
        movement.LookAt = ent.LookAt;
        ent.Movement = movement;
        ent.IsLoaded = true;
        Node(commonState.Overlay).Mutate(multiplayerDebugMenu.Create);
        Node(commonState.Overlay).Mutate(multiplayerRosterMenu.Create);
    }

    public override void Unload()
    {
        identityCache.SetPlayerListOpen(false);
        commonState.Unload();
        session.Unload();
    }

    public override void Update(double time)
    {
        commonState.Update(time);

        identityCache.SetPlayerListOpen(commonState.Playing && keyboard.IsKeyDown(Keys.Tab));

        if (commonState.Playing)
            player.Input();
    }

    public override void Frame(double time)
    {
        client.Sync();

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
            clock.Tick();
            if (commonState.Playing)
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
        if (commonState.ShowNameplates)
            nameplates.Draw();

        commonState.Draw();
    }
}
