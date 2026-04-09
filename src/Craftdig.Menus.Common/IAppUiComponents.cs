namespace Craftdig.Menus.Common;

[Components(SkipBuilder = true)]
public interface IAppUiComponents
{
    /// <summary>Tooltip text displayed when hovering this node.</summary>
    UiText TooltipFV { get; set; }
    /// <summary>Root node for stack-based menu navigation.</summary>
    UiProp<EntMut> StackRootFV { get; set; }
    /// <summary>Method used to create a menu within the menu stack.</summary>
    UiCallback<Action<EntMut>> MenuOriginFV { get; set; }
    /// <summary>Player entity associated with this node.</summary>
    UiProp<EntMutIdx> PlayerFV { get; set; }
    /// <summary>Callback that returns the item slot value.</summary>
    UiCallback<Func<ItemSlot>?> GetSlotValueFV { get; set; }
    /// <summary>Callback that sets the item slot value.</summary>
    UiCallback<Action<ItemSlot>?> SetSlotValueFV { get; set; }
    /// <summary>String builder for text input content.</summary>
    UiProp<StringBuilder?> StringBuilderFV { get; set; }
    /// <summary>Maximum character length for text input.</summary>
    UiProp<int> MaxLengthFV { get; set; }
    /// <summary>Callback invoked when text input content changes.</summary>
    UiCallback<Action?> OnTextUpdatedFV { get; set; }
    /// <summary>Slot entity this node is associated with.</summary>
    UiProp<EntMut> SlotFV { get; set; }

    /// <summary>Whether a slot item was added this frame.</summary>
    bool SlotAddedR { get; set; }
    /// <summary>Whether this node was focused in the previous frame.</summary>
    bool WasFocusedR { get; set; }
    /// <summary>Timestamp when this node received focus.</summary>
    DateTime FocusStartR { get; set; }
    /// <summary>Text cursor character for text input.</summary>
    string CarretR { get; set; }
}
