namespace Crafthoe.World;

[World]
public class WorldModuleIndices(ModuleEnts ents, int[] rtToIndex, int[] indexToRt)
{
    public int this[Ent block] => rtToIndex[block.RuntimeIndex()];
    public Ent this[int index] => ents[indexToRt[index]];
}
