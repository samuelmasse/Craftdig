namespace Craftdig.Module;

[Components]
public interface IModuleComponents
{
    // Common
    [ComponentToString] string ModuleName { get; set; }
    [ComponentToString] string Name { get; set; }
    int RuntimeIndex { get; set; }

    // Difficulty
    [ComponentToString] bool IsDifficulty { get; set; }
    float Order { get; set; }

    // Game mode
    [ComponentToString] bool IsGameMode { get; set; }
    Ent LockedDifficulty { get; set; }

    // Dimension
    [ComponentToString] bool IsDimension { get; set; }
    Ent Air { get; set; }
    bool HasSkyLight { get; set; }
    Type TerrainGeneratorType { get; set; }
    Type BiomeGeneraetorType { get; set; }

    // Block
    [ComponentToString] bool IsBlock { get; set; }
    bool IsBuildable { get; set; }
    bool IsSolid { get; set; }
    byte LightEmission { get; set; }
    byte LightOpacity { get; set; }
    int MaxStack { get; set; }
}
