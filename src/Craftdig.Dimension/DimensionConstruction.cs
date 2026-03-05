namespace Craftdig.Dimension;

[Dimension]
public class DimensionConstruction(
    WorldModuleIndices moduleIndices,
    DimensionAir air,
    DimensionBlocks blocks,
    DimensionPlayerBag playerBag,
    DimensionRigidBag rigidBag,
    DimensionSelected selected)
{
    public void Tick()
    {
        foreach (var ent in playerBag.Ents)
            Tick((EntMut)ent);
    }

    private void Tick(EntMut ent)
    {
        var constr = ent.Construction;
        ent.Construction = default;

        if (constr.Action == ConstructionAction.None)
            return;

        if (constr.Action == ConstructionAction.Drop)
        {
            rigidBag.Add(new EntPtr()
                .Mutate()
                .IsTestCube(true)
                .TestCubeMaterial(moduleIndices[constr.Arg])
                .TestCubeSize(0.5f)
                .IsProjectile(true)
                .HitBox(new Box3d((-0.25, -0.25, -0.25), (0.25, 0.25, 0.25)))
                .Position(ent.Position)
                .Velocity(ent.Velocity + ent.Movement.LookAt / 2)
                .Ent);

            return;
        }

        var selection = selected[ent];
        if (selection == null)
            return;

        if (constr.Action == ConstructionAction.Remove)
            blocks.TrySet(selection.Value.Loc, air.Block);
        else if (constr.Action == ConstructionAction.Place)
            blocks.TrySet(selection.Value.Loc + selection.Value.Normal, moduleIndices[constr.Arg]);
    }
}
