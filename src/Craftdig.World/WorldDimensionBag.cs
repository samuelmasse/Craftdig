namespace Craftdig.World;

[World]
public class WorldDimensionBagMut : EntIdxGatedBagMut<WorldComponents.IsDimensionScope, WorldComponents.IsLoaded>;

[World]
public class WorldDimensionBag(WorldDimensionBagMut bag) :
    EntIdxGatedBag<WorldComponents.IsDimensionScope, WorldComponents.IsLoaded>(bag);
