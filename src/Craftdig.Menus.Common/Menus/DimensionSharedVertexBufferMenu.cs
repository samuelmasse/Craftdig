namespace Craftdig.Menus.Common;

[Dimension]
public class DimensionSharedVertexBufferMenu(
    RootText text,
    RootKeyboard keyboard,
    RootRoboto roboto,
    RootSprites sprites,
    DimensionSharedVertexBuffer svb,
    AppStyle s)
{
    public void Create(EntObj root)
    {
        Node(root, out var menu)
            .Mut(s.VerticalList)
            .SizeRelativeV((1, 0))
            .AlignmentV(Alignment.Bottom)
            .ColorV((0, 0, 0, 1))
            .IsDisabledV(true)
            .OnDrawF((o) =>
            {
                var slots = svb.Allocator.AllocationSlots;

                foreach (var alloc in svb.Allocator.Allocations)
                {
                    var slot = slots[alloc];
                    if (slot.Index != 0)
                    {
                        sprites.Batch.Draw((
                            o.X + (slot.Index / (float)svb.Size) * menu.SizeR().X, o.Y),
                            ((slot.Size / (float)svb.Size) * menu.SizeR().X, menu.SizeR().Y), (1, 0, 0, 1));
                    }
                }
            });
        {
            Func<ReadOnlySpan<char>>[][] rows =
            [
                [
                    () => text.Format("Allocation Slots {0}", svb.Allocator.AllocationSlots.Length),
                    () => text.Format("Used {0}", svb.Allocator.Used),
                    () => text.Format("Free Blocks {0}", svb.Allocator.FreeBlockCount),
                    () => text.Format("Index Set Pool {0}", svb.Allocator.IndexSetPoolCount)
                ],
                [
                    () => text.Format("Allocations {0}", svb.Allocator.Allocations.Length),
                    () => text.Format("Size {0}", svb.Allocator.Size),
                    () => text.Format("Free Sizes {0}", svb.Allocator.FreeSizeCount),
                    () => text.Format("Resize Time {0}", svb.Allocator.ResizeTime)
                ]
            ];

            foreach (var row in rows)
            {
                Node(menu, out var hor)
                    .SizeInnerMaxRelativeV((0, 1))
                    .InnerLayoutV(InnerLayout.HorizontalList)
                    .InnerSizingV(InnerSizing.HorizontalWeight);
                foreach (var col in row)
                {
                    Node(hor)
                        .Mut(s.Label)
                        .FontV(roboto.Font)
                        .FontSizeV(24)
                        .TextAlignmentV(Alignment.Left)
                        .TextF(col);
                }
            }
        }

        Node(root).OnUpdateF(() =>
        {
            if (keyboard.IsKeyPressed(Keys.F7))
                menu.IsDisabledV() = !menu.IsDisabledV();
        });
    }
}
