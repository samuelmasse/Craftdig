namespace Craftdig;

[ModuleLoader]
public class ModuleNativeBackendLoader(ModuleNative m) : ModLoader
{
    public override void Load()
    {
        m.OverworldDimension.Mutate()
            .TerrainGeneratorType(typeof(DimensionNativeTerrainGenerator))
            .BiomeGeneraetorType(typeof(DimensionNativeBiomeGenerator));
    }
}
