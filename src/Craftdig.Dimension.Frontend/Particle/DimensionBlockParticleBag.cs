namespace Craftdig;

[Dimension]
public class DimensionBlockParticleBagMut :
    EntIdxBagMut<DimensionFrontendComponents.IsBlockParticle>;

[Dimension]
public class DimensionBlockParticleBag(DimensionBlockParticleBagMut bag) :
    EntIdxBag<DimensionFrontendComponents.IsBlockParticle>(bag);
