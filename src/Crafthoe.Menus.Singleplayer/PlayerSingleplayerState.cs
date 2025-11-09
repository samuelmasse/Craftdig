namespace Crafthoe.Menus.Singleplayer;

[Player]
public class PlayerSingleplayerState(
    WorldTick tick,
    DimensionBackend backend,
    DimensionContext context,
    PlayerMetrics playerMetrics,
    PlayerFrontend player,
    PlayerCommonState commonState,
    PlayerSingleplayerUnloadWorldAction singleplayerUnloadWorldAction) : State
{
    public override void Load()
    {
        commonState.Load();
    }

    public override void Unload()
    {
        commonState.Unload();
        singleplayerUnloadWorldAction.Run();
    }

    public override void Update(double time)
    {
        commonState.Update(time);

        if (!commonState.Paused)
        {
            int ticks = tick.Update(time);
            while (ticks > 0)
            {
                if (!commonState.Inv)
                    player.Tick();
                else player.NoTick();

                playerMetrics.TickMetric.Start();
                context.Tick();
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
        backend.Frame();
        commonState.Render();
    }

    public override void Draw()
    {
        commonState.Draw();
    }
}
