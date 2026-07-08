namespace Craftdig.Dimension;

[Dimension]
public class DimensionRigidBagMut : EntIdxGatedBagMut<DimensionComponents.IsRigid, WorldComponents.IsLoaded>;

[Dimension]
public class DimensionRigidBag(DimensionRigidBagMut bag) :
    EntIdxGatedBag<DimensionComponents.IsRigid, WorldComponents.IsLoaded>(bag);
