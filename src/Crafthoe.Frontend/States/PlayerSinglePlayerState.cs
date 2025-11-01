namespace Crafthoe.Frontend;

[Player]
public class PlayerSinglePlayerState(
    WorldTick tick,
    DimensionServerContext dimensionServer,
    DimensionRigidBag rigidBag,
    DimensionMetrics dimensionMetrics,
    PlayerEnt ent,
    PlayerContext player,
    PlayerCommonState commonState,
    PlayerSinglePlayerUnloadWorldAction singlePlayerUnloadWorldAction) : State
{
    public override void Load()
    {
        ent.Ent.HitBox() = new Box3d((-0.3, -0.3, -1.62), (0.3, 0.3, 0.18));
        ent.Ent.Position() = (15, 0, 120);
        ent.Ent.IsFlying() = true;
        rigidBag.Add(ent.Ent);
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

        if (!commonState.Paused)
        {
            int ticks = tick.Update(time);
            while (ticks > 0)
            {
                if (!commonState.Inv)
                    player.Tick();

                dimensionMetrics.TickMetric.Start();
                dimensionServer.Tick();
                dimensionMetrics.TickMetric.End();

                ticks--;
            }

            if (!commonState.Inv)
                player.Update(time);
        }
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
