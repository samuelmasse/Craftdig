namespace Craftdig;

[Dimension]
public class DimensionConstruction(
    WorldModuleIndices moduleIndices,
    DimensionEnt dimension,
    DimensionBlocks blocks,
    DimensionPlayerBag playerBag,
    DimensionRigidBag rigidBag,
    DimensionSelected selected)
{
    public void Tick()
    {
        foreach (var ent in playerBag.Ents)
            if (!ent.IsRemote)
                Tick(ent);
    }

    private void Tick(EntMutIdx ent)
    {
        var constr = ent.Construction;
        ent.Construction = default;

        if (constr.Action == ConstructionAction.None)
            return;

        var selection = selected[ent];
        if (selection == null)
            return;

        if (constr.Action == ConstructionAction.Remove)
            blocks.TrySet(selection.Value.Loc, dimension.Air);
        else if (constr.Action == ConstructionAction.Place)
        {
            var loc = selection.Value.Loc + selection.Value.Normal;
            var block = moduleIndices[constr.Arg];
            if (!block.IsSolid || !CollidesWithRigid(loc))
                blocks.TrySet(loc, block);
        }
    }

    private bool CollidesWithRigid(Vec3i loc)
    {
        var blockBox = new Box3d(loc, loc + Vec3i.One);

        foreach (var rigid in rigidBag.Ents)
        {
            if (blockBox.IntersectsExclusive(rigid.HitBox.Translated(rigid.Position)))
                return true;
        }

        return false;
    }
}
