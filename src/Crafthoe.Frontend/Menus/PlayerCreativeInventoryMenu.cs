namespace Crafthoe.Frontend;

[Player]
public class PlayerCreativeInventoryMenu(ModuleEnts ents, AppStyle s, PlayerHand hand, PlayerEnt player)
{
    public void Create(EntObj root)
    {
        int rows = 5;
        var blocks = new Ent[(rows + 1) * HotBarSlots.Count];
        int count = 0;

        foreach (var ent in ents.Span)
        {
            if (ent.IsBlock() && ent.IsBuildable())
                blocks[count++] = ent;
        }

        Node(root, out var vert)
            .SizeInnerSumRelativeV(s.Vertical)
            .SizeInnerMaxRelativeV(s.Horizontal)
            .PaddingV((s.ItemSpacing, s.ItemSpacing, s.ItemSpacing, s.ItemSpacing))
            .InnerSpacingV(s.ItemSpacing)
            .ColorV(s.BoardColor)
            .IsSelectableV(true)
            .InnerLayoutV(InnerLayout.VerticalList)
            .AlignmentV(Alignment.Center);

        Node(vert, out var title)
            .Mut(s.Label)
            .TextV("Building Blocks");

        Node(vert, out var blocksVert)
            .SizeInnerSumRelativeV(s.Vertical)
            .SizeInnerMaxRelativeV(s.Horizontal)
            .InnerSpacingV(s.ItemSpacingS)
            .InnerLayoutV(InnerLayout.VerticalList);
        for (int y = 0; y < rows; y++)
        {
            Node(blocksVert, out var blocksHor)
                .SizeInnerSumRelativeV(s.Horizontal)
                .SizeInnerMaxRelativeV(s.Vertical)
                .InnerSpacingV(s.ItemSpacingS)
                .InnerLayoutV(InnerLayout.HorizontalList);

            for (int x = 0; x < HotBarSlots.Count; x++)
            {
                Vector2i loc = (x, y);
                bool added = false;

                Node(blocksHor, out var square)
                    .Mut(s.Button)
                    .SizeV((s.SlotSize, s.SlotSize))
                    .OnPressF(() =>
                    {
                        if (hand.Ent == default)
                        {
                            hand.Ent = blocks[loc.Y * HotBarSlots.Count + loc.X];
                            added = true;
                        }
                    })
                    .OnClickF(() =>
                    {
                        if (!added)
                            hand.Ent = default;

                        added = false;
                    })
                    .OnSecondaryPressF(square.OnPressF())
                    .OnSecondaryClickF(square.OnClickF())
                    .TooltipF(() => hand.Ent == default ?
                        blocks[loc.Y * HotBarSlots.Count + loc.X].Name() : null);
            }
        }

        Node(vert, out var hotbar)
            .SizeInnerSumRelativeV(s.Horizontal)
            .SizeInnerMaxRelativeV(s.Vertical)
            .InnerSpacingV(s.ItemSpacingS)
            .InnerLayoutV(InnerLayout.HorizontalList);
        for (int x = 0; x < HotBarSlots.Count; x++)
        {
            bool added = false;
            int i = x;

            Node(hotbar, out var square)
                .Mut(s.Button)
                .SizeV((s.SlotSize, s.SlotSize))
                .OnPressF(() =>
                {
                    if (hand.Ent == default)
                    {
                        hand.Ent = player.Ent.HotBarSlots()[i];
                        player.Ent.HotBarSlots()[i] = default;
                        added = true;
                    }
                })
                .OnClickF(() =>
                {
                    if (!added)
                        (hand.Ent, player.Ent.HotBarSlots()[i]) = (player.Ent.HotBarSlots()[i], hand.Ent);

                    added = false;
                })
                .OnSecondaryPressF(square.OnPressF())
                .OnSecondaryClickF(square.OnSecondaryClickF())
                .TooltipF(() => hand.Ent == default ? player.Ent.HotBarSlots()[i].Name() : null);
        }
    }
}
