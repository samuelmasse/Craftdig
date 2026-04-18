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
    /// <summary>Method used to create the companion node for this menu stack entry.</summary>
    UiCallback<Action<EntMut>?> CompanionOriginFV { get; set; }
    /// <summary>Whether this menu stack entry blocks Escape from popping it.</summary>
    UiProp<bool> IsModalFV { get; set; }
    /// <summary>Player entity associated with this node.</summary>
    UiProp<EntMutIdx> PlayerFV { get; set; }
    /// <summary>Callback that returns the item slot value.</summary>
    UiCallback<Func<ItemSlot>?> GetSlotValueFV { get; set; }
    /// <summary>Callback that sets the item slot value.</summary>
    UiCallback<Action<ItemSlot>?> SetSlotValueFV { get; set; }
    /// <summary>Slot entity this node is associated with.</summary>
    UiProp<EntMut> SlotFV { get; set; }

    /// <summary>Whether a slot item was added this frame.</summary>
    bool SlotAddedR { get; set; }
}
