namespace Craftdig;

[Dimension]
public class DimensionIndexedComponentsMut(
    WorldComponentIndicesFile componentIndicesFile,
    WorldComponentIndicesMut componentIndices) :
    WorldIndexedComponentsMut(componentIndicesFile, componentIndices);

[Dimension]
public class DimensionIndexedComponents(DimensionIndexedComponentsMut indexedComponents) : WorldIndexedComponents(indexedComponents);
