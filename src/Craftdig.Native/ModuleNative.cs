namespace Craftdig.Native;

[Module]
public record class ModuleNative(
    EntMut OverworldDimension,

    EntMut SurvivalGameMode,
    EntMut CreativeGameMode,
    EntMut HardcoreGameMode,

    EntMut NormalDifficulty,
    EntMut HardDifficulty,
    EntMut PeacefulDifficulty,
    EntMut EasyDifficulty,

    EntMut GrassFace,
    EntMut GrassSideFace,
    EntMut StoneFace,
    EntMut DirtFace,

    EntMut AirBlock,
    EntMut GrassBlock,
    EntMut DirtBlock,
    EntMut StoneBlock) : EntMutList;
