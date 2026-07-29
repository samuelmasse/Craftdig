namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionFrontend(
    DimensionBlockParticles blockParticles,
    DimensionSectionRequester sectionRequester,
    DimensionSectionReceiver sectionReceiver,
    DimensionSectionInvalidation sectionInvalidation)
{
    public void Frame()
    {
        blockParticles.Frame();
        sectionInvalidation.Frame();
        sectionRequester.Frame();
        sectionReceiver.Frame();
    }
}
