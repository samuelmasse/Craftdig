namespace Craftdig.Dimension;

[Dimension]
public class DimensionPlayerBagMut : EntIdxBagMut<DimensionComponents.IsPlayer>;

[Dimension]
public class DimensionPlayerBag(DimensionPlayerBagMut bag) : EntIdxBag<DimensionComponents.IsPlayer>(bag);
