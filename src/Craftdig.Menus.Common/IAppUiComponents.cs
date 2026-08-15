namespace Craftdig;

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
    /// <summary>Player Ent associated with this node.</summary>
    UiProp<EntMutIdx> PlayerFV { get; set; }
    /// <summary>Callback that returns the item slot value.</summary>
    UiCallback<Func<ItemSlot>?> GetSlotValueFV { get; set; }
    /// <summary>Callback that sets the item slot value.</summary>
    UiCallback<Action<ItemSlot>?> SetSlotValueFV { get; set; }
    /// <summary>Callback that reports a completed inventory click.</summary>
    UiCallback<Action<InventoryClick>?> InventoryClickFV { get; set; }
    /// <summary>Slot Ent this node is associated with.</summary>
    UiProp<EntMut> SlotFV { get; set; }
}
