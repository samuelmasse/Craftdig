namespace Craftdig.Dimension;

[Dimension]
public class DimensionRigidBagMut : EntIdxBagMut<DimensionComponents.IsRigid>;

[Dimension]
public class DimensionRigidBag(DimensionRigidBagMut bag) : EntIdxBag<DimensionComponents.IsRigid>(bag);
