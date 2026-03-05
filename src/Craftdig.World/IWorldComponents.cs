namespace Craftdig.World;

[Components]
public interface IWorldComponents
{
    [ComponentToString]
    ulong WorldId { get; set; }

    int WorldEntPtrBagIndex { get; set; }
    int DimensionBagIndex { get; set; }

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
