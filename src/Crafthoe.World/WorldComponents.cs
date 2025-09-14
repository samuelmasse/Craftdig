namespace Crafthoe.World;

[Components]
file record WorldComponents(
    int WorldEntPtrBagIndex,

    ArmorSlots ArmorSlots,
    InventorySlots InventorySlots,

    HotBarSlots HotBarSlots,
    int HotBarIndex,

    ItemSlot Offhand
);
