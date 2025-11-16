namespace Crafthoe.Native;

[ModuleLoader]
public class ModuleNativeBackendLoader(ModuleNative m) : ModLoader
{
    public override void Load()
    {
        m.OverworldDimension
            .TerrainGeneratorType(typeof(DimensionNativeTerrainGenerator))
            .BiomeGeneraetorType(typeof(DimensionNativeBiomeGenerator));
    }
}
