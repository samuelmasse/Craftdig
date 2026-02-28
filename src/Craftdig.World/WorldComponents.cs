namespace Craftdig.World;

[Components]
file record WorldComponents(
    [ComponentToString] ulong WorldId,
    int WorldEntPtrBagIndex,
    int DimensionBagIndex,

    Ent[] ArmorSlotEntities,
    int[] ArmorSlotCounts,
    Ent[] InventorySlotEntities,
    int[] InventorySlotCounts,
    Ent[] HotBarSlotEntities,
    int[] HotBarSlotCounts,

    int HotBarIndex,

    Ent OffhandEntity,
    int OffhandCount
);
