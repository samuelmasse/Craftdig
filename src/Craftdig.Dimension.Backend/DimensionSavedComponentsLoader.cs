namespace Craftdig.Dimension.Backend;

[DimensionLoader]
public class DimensionSavedComponentsLoader(WorldIndexedComponentsMut indexedComponents)
{
    public void Run()
    {
        indexedComponents.AddSaved<DimensionComponents>();
    }
}
