namespace Craftdig;

[Dimension]
public class DimensionDropBackend(
    WorldModuleIndices moduleIndices,
    DimensionPlayerBag playerBag,
    DimensionEntArena entArena)
{
    public void Tick()
    {
        foreach (var ent in playerBag.Ents)
            Tick(ent);
    }

    private void Tick(EntMutIdx ent)
    {
        var drop = ent.Drop;
        ent.Drop = default;

        if (drop.Action == DropAction.None)
            return;

        if (drop.Action == DropAction.DropTest)
        {
            entArena.Alloc().Mutate()
                .IsTestCube(true)
                .TestCubeMaterial(moduleIndices[drop.Arg])
                .TestCubeSize(0.5f)
                .IsRigid(true)
                .IsProjectile(true)
                .HitBox(new Box3d((-0.25, -0.25, -0.25), (0.25, 0.25, 0.25)))
                .Position(ent.Position)
                .Velocity(ent.Velocity + ent.LookAt / 2)
                .IsLoaded(true);
        }
    }
}
