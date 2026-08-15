namespace Craftdig;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct InventoryOperation(
    InventoryActionKind Kind,
    InventoryContainer Container,
    byte Slot,
    InventoryClick Click,
    int ModuleIndex)
{
    public static InventoryOperation ClickSlot(
        InventoryContainer container,
        int slot,
        InventoryClick click) =>
        new(InventoryActionKind.Click, container, (byte)slot, click, 0);

    public static InventoryOperation ClickCreative(int moduleIndex, InventoryClick click) =>
        new(InventoryActionKind.CreativeClick, default, 0, click, moduleIndex);

    public static InventoryOperation SelectHotBar(int slot) =>
        new(InventoryActionKind.SelectHotBar, default, (byte)slot, default, 0);
}
