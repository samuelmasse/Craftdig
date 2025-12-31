namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionFrontend(
    DimensionSectionRequester sectionRequester,
    DimensionSectionReceiver sectionReceiver,
    DimensionSectionInvalidation sectionInvalidation)
{
    public void Frame()
    {
        sectionInvalidation.Frame();
        sectionRequester.Frame();
        sectionReceiver.Frame();
    }
}
