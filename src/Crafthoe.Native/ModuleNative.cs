namespace Crafthoe.Native;

[Module]
public record class ModuleNative(
    EntMut OverworldDimension,

    EntMut GrassFace,
    EntMut GrassSideFace,
    EntMut StoneFace,
    EntMut DirtFace,

    EntMut AirBlock,
    EntMut GrassBlock,
    EntMut DirtBlock,
    EntMut StoneBlock) : EntMutList;
