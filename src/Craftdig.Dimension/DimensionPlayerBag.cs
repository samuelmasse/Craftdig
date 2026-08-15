namespace Craftdig;

[Dimension]
public class DimensionPlayerBagMut : EntIdxGatedBagMut<DimensionComponents.IsPlayer, WorldComponents.IsLoaded>;

[Dimension]
public class DimensionPlayerBag(DimensionPlayerBagMut bag) :
    EntIdxGatedBag<DimensionComponents.IsPlayer, WorldComponents.IsLoaded>(bag);
