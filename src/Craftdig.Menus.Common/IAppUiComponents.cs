namespace Craftdig.Menus.Common;

[Components]
public interface IAppUiComponents
{
    string? TooltipV { get; set; }
    Func<ReadOnlySpan<char>>? TooltipF { get; set; }
    EntObj StackRootV { get; set; }
    EntMutIdx PlayerV { get; set; }
    Func<ItemSlot>? GetSlotValueF { get; set; }
    Action<ItemSlot>? SetSlotValueF { get; set; }
    StringBuilder? StringBuilderV { get; set; }
    int MaxLengthV { get; set; }
    EntObj SlotV { get; set; }
    bool SlotAddedV { get; set; }
    bool WasFocusedR { get; set; }
    DateTime FocusStartR { get; set; }
    string CarretR { get; set; }
}
