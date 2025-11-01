namespace Crafthoe.Dimension.Frontend;

[Dimension]
public class DimensionClientContext(
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
