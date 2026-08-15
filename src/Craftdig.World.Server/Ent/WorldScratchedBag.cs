namespace Craftdig;

[World]
public class WorldScratchedBagMut : EntIdxBagMut<WorldServerComponents.IsScratched>;

[World]
public class WorldScratchedBag(WorldScratchedBagMut bag) : EntIdxBag<WorldServerComponents.IsScratched>(bag);
