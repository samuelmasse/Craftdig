namespace Craftdig.Native;

[ModuleLoader]
public class ModuleNativeLoader(ModuleNative m) : ModLoader
{
    public override void Load()
    {
        m.OverworldDimension.Mutate()
            .IsDimension(true)
            .Air(m.AirBlock);

        LoadGameModes();
        LoadDifficulties();
        LoadBlocks();
    }

    private void LoadGameModes()
    {
        m.SurvivalGameMode.Mutate()
            .IsGameMode(true)
            .Name("Survival")
            .Order(0);

        m.CreativeGameMode.Mutate()
            .IsGameMode(true)
            .Name("Creative")
            .Order(1);

        m.HardcoreGameMode.Mutate()
            .IsGameMode(true)
            .Name("Hardcore")
            .Order(2)
            .LockedDifficulty(m.HardDifficulty);
    }

    private void LoadDifficulties()
    {
        m.NormalDifficulty.Mutate()
            .IsDifficulty(true)
            .Name("Normal")
            .Order(0);

        m.HardDifficulty.Mutate()
            .IsDifficulty(true)
            .Name("Hard")
            .Order(1);

        m.PeacefulDifficulty.Mutate()
            .IsDifficulty(true)
            .Name("Peaceful")
            .Order(2);

        m.EasyDifficulty.Mutate()
            .IsDifficulty(true)
            .Name("Easy")
            .Order(3);
    }

    private void LoadBlocks()
    {
        m.AirBlock.Mutate()
            .Name("Air")
            .IsBlock(true);

        m.GrassBlock.Mutate()
            .Name("Grass")
            .MaxStack(64)
            .IsBlock(true)
            .IsSolid(true)
            .IsBuildable(true);

        m.DirtBlock.Mutate()
            .Name("Dirt")
            .MaxStack(64)
            .IsBlock(true)
            .IsSolid(true)
            .IsBuildable(true);

        m.StoneBlock.Mutate()
            .Name("Stone")
            .MaxStack(64)
            .IsBlock(true)
            .IsSolid(true)
            .IsBuildable(true);
    }
}
