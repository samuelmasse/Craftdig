namespace Craftdig;

[Server]
public class ServerTick(
    WorldDimensionBag dimensions,
    WorldServer world,
    ServerKicker kicker)
{
    public void Tick()
    {
        kicker.Tick();
        world.Tick();

        foreach (var dimension in dimensions.Ents)
            dimension.DimensionScope.Get<DimensionServer>().Tick();

        world.Stream();

        foreach (var dimension in dimensions.Ents)
            dimension.DimensionScope.Get<DimensionServer>().Stream();
    }
}
