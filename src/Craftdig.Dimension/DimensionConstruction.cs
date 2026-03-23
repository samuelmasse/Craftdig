namespace Craftdig.Dimension;

[Dimension]
public class DimensionConstruction(
    WorldModuleIndices moduleIndices,
    DimensionEnt dimension,
    DimensionBlocks blocks,
    DimensionPlayerBag playerBag,
    DimensionSelected selected)
{
    public void Tick()
    {
        foreach (var ent in playerBag.Ents)
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
            blocks.TrySet(selection.Value.Loc + selection.Value.Normal, moduleIndices[constr.Arg]);
    }
}
