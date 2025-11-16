namespace Crafthoe.Native;

[ModuleLoader]
public class ModuleNativeLoader(ModuleNative m) : ModLoader
{
    public override void Load()
    {
        m.OverworldDimension
            .IsDimension(true)
            .Air(m.AirBlock);

        LoadGameModes();
        LoadDifficulties();
        LoadBlocks();
    }

    private void LoadGameModes()
    {
        m.SurvivalGameMode
            .IsGameMode(true)
            .Name("Survival")
            .Order(0);

        m.CreativeGameMode
            .IsGameMode(true)
            .Name("Creative")
            .Order(1);

        m.HardcoreGameMode
            .IsGameMode(true)
            .Name("Hardcore")
            .Order(2)
            .LockedDifficulty(m.HardDifficulty);
    }

    private void LoadDifficulties()
    {
        m.NormalDifficulty
            .IsDifficulty(true)
            .Name("Normal")
            .Order(0);

        m.HardDifficulty
            .IsDifficulty(true)
            .Name("Hard")
            .Order(1);

        m.PeacefulDifficulty
            .IsDifficulty(true)
            .Name("Peaceful")
            .Order(2);

        m.EasyDifficulty
            .IsDifficulty(true)
            .Name("Easy")
            .Order(3);
    }

    private void LoadBlocks()
    {
        m.AirBlock
            .Name("Air")
            .IsBlock(true);

        m.GrassBlock
            .Name("Grass")
            .MaxStack(64)
            .IsBlock(true)
            .IsSolid(true)
            .IsBuildable(true);

        m.DirtBlock
            .Name("Dirt")
            .MaxStack(64)
            .IsBlock(true)
            .IsSolid(true)
            .IsBuildable(true);

        m.StoneBlock
            .Name("Stone")
            .MaxStack(64)
            .IsBlock(true)
            .IsSolid(true)
            .IsBuildable(true);
    }
}
