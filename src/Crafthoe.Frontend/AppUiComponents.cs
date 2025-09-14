namespace Crafthoe.Frontend;

[Components]
file record AppUiComponents(
    string? TooltipV,
    Func<ReadOnlySpan<char>>? TooltipF,
    EntObj? StackRootV,
    EntMut PlayerV,
    Func<ItemSlot>? GetSlotValueF,
    Action<ItemSlot>? SetSlotValueF,
    EntObj SlotV,
    bool SlotAddedV);
