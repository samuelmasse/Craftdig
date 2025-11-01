namespace Crafthoe.Menus.Common;

[Components]
file record AppUiComponents(
    [ComponentToString] string? TagV,
    string? TooltipV,
    Func<ReadOnlySpan<char>>? TooltipF,
    EntObj? StackRootV,
    EntMut PlayerV,
    Func<ItemSlot>? GetSlotValueF,
    Action<ItemSlot>? SetSlotValueF,
    StringBuilder? StringBuilderV,
    int MaxLengthV,
    EntObj SlotV,
    bool SlotAddedV,
    bool WasFocusedR,
    DateTime FocusStartR,
    string CarretR);
