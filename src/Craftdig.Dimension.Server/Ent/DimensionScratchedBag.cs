namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionScratchedBagMut : EntIdxBagMut<WorldServerComponents.IsScratched>;

[Dimension]
public class DimensionScratchedBag(DimensionScratchedBagMut bag) :
    EntIdxBag<WorldServerComponents.IsScratched>(bag);
