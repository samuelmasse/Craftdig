namespace Craftdig.Menus.Singleplayer;

[Player]
public class PlayerSingleplayerState(
    WorldTick tick,
    WorldBackend worldBackend,
    DimensionBackend backend,
    DimensionContext context,
    PlayerMetrics playerMetrics,
    PlayerFrontend player,
    PlayerEnt ent,
    PlayerCamera camera,
    PlayerCommonState commonState,
    PlayerSingleplayerUnloadWorldAction singleplayerUnloadWorldAction,
    PlayerScreenshot screenshot) : State
{
    private bool initialTick = true;

    public override void Load()
    {
        if (ent.IsPlayer)
            camera.SetLookAt(ent.LookAt.Swizzle());

        commonState.Load();
    }

    public override void Unload()
    {
        screenshot.Run();
        commonState.Unload();
        singleplayerUnloadWorldAction.Run();
    }

    public override void Update(double time)
    {
        commonState.Update(time);

        if (!commonState.Paused)
        {
            int ticks = initialTick ? 1 : tick.Update(time);
            initialTick = false;

            while (ticks > 0)
            {
                if (!commonState.Inv)
                    player.Tick();
                else player.NoTick();

                playerMetrics.TickMetric.Start();
                worldBackend.Tick();
                context.Tick();
                backend.Tick();
                playerMetrics.TickMetric.End();

                ticks--;
            }

            player.Update(time);
            if (!commonState.Inv)
                player.Input();
        }
    }

    public override void Render()
    {
        worldBackend.Frame();
        backend.Frame();
        commonState.Render();
    }

    public override void Draw()
    {
        commonState.Draw();
    }
}
