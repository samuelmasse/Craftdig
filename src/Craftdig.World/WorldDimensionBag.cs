namespace Craftdig.World;

[World]
public class WorldDimensionBagMut : EntIdxBagMut<WorldComponents.IsDimensionScope>;

[World]
public class WorldDimensionBag(WorldDimensionBagMut bag) : EntIdxBag<WorldComponents.IsDimensionScope>(bag);
