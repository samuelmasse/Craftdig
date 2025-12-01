namespace Crafthoe.World;

[Components]
file record WorldComponents(
    [ComponentToString] ulong WorldId,
    int WorldEntPtrBagIndex,
    int DimensionBagIndex,

    ArmorSlots ArmorSlots,
    InventorySlots InventorySlots,

    HotBarSlots HotBarSlots,
    int HotBarIndex,

    ItemSlot Offhand
);
