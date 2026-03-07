namespace Craftdig.World;

[Components]
public interface IWorldComponents
{
    [ComponentToString] Guid Id { get; set; }
    HashSet<EntPtrIdx> ContextEntLiveSet { get; set; }
    bool IsLoaded { get; set; }

    bool IsDimensionScope { get; set; }

    Ent[] ArmorSlotEnts { get; set; }
    int[] ArmorSlotCounts { get; set; }
    Ent[] InventorySlotEnts { get; set; }
    int[] InventorySlotCounts { get; set; }
    Ent[] HotBarSlotEnts { get; set; }
    int[] HotBarSlotCounts { get; set; }

    int HotBarIndex { get; set; }

    Ent OffhandEnt { get; set; }
    int OffhandCount { get; set; }
}
