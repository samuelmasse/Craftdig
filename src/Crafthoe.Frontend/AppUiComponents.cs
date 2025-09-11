namespace Crafthoe.Frontend;

[Components]
file record AppUiComponents(
    string? TooltipV,
    Func<ReadOnlySpan<char>>? TooltipF,
    EntObj? StackRootV,
    EntMut PlayerV,
    Func<Ent>? GetSlotValueF,
    Action<Ent>? SetSlotValueF,
    EntObj SlotV,
    bool SlotAddedV);
