namespace Craftdig;

[Dimension]
public class DimensionSeerBagMut : EntIdxBagMut<DimensionComponents.IsSeer>;

[Dimension]
public class DimensionSeerBag(DimensionSeerBagMut bag) : EntIdxBag<DimensionComponents.IsSeer>(bag);
