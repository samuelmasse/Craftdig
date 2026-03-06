namespace Craftdig.World;

[Components]
public interface IWorldComponents
{
    HashSet<EntPtrIdx> ContextEntLiveSet { get; set; }

    bool IsDimensionScope { get; set; }

    Ent[] ArmorSlotEntities { get; set; }
    int[] ArmorSlotCounts { get; set; }
    Ent[] InventorySlotEntities { get; set; }
    int[] InventorySlotCounts { get; set; }
    Ent[] HotBarSlotEntities { get; set; }
    int[] HotBarSlotCounts { get; set; }

    int HotBarIndex { get; set; }

    Ent OffhandEntity { get; set; }
    int OffhandCount { get; set; }
}
