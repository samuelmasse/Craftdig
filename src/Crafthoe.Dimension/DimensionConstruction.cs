namespace Crafthoe.Dimension;

[Dimension]
public class DimensionConstruction(
    WorldModuleIndices moduleIndices,
    DimensionAir air,
    DimensionBlocks blocks,
    DimensionPlayerBag playerBag,
    DimensionSelected selected)
{
    public void Tick()
    {
        foreach (var ent in playerBag.Ents)
        {
            Tick((EntMut)ent);
            ent.Construction() = default;
        }
    }

    private void Tick(EntMut ent)
    {
        var constr = ent.Construction();
        if (constr.Action == ConstructionAction.None)
            return;

        var selection = selected[ent];
        if (selection == null)
            return;

        if (constr.Action == ConstructionAction.Remove)
            blocks.TrySet(selection.Value.Loc, air.Block);
        else if (constr.Action == ConstructionAction.Place)
            blocks.TrySet(selection.Value.Loc + selection.Value.Normal, moduleIndices[constr.Arg]);
    }
}
