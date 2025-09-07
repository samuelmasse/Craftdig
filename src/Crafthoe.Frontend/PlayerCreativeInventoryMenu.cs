namespace Crafthoe.Frontend;

[Player]
public class PlayerCreativeInventoryMenu(ModuleEnts ents, AppStyle s, PlayerHand hand, PlayerEnt player)
{
    public EntObj Get()
    {
        var blocks = new Ent[6 * HotBarSlots.Count];
        int count = 0;

        foreach (var ent in ents.Span)
        {
            if (ent.IsBlock() && ent.IsBuildable())
                blocks[count++] = ent;
        }

        Node(out var menu).SizeRelativeV((1, 1));

        Node(menu, out var vert)
            .SizeInnerSumRelativeV((0, 1))
            .SizeInnerMaxRelativeV((1, 0))
            .PaddingV((32, 32, 32, 32))
            .InnerSpacingV(32)
            .ColorV((1, 0, 0, 1))
            .IsSelectableV(true)
            .InnerLayoutV(InnerLayout.VerticalList)
            .AlignmentV(Alignment.Center);

        Node(vert, out var title)
            .Mut(s.Label)
            .TextV("Building Blocks");

        Node(vert, out var blocksVert)
            .SizeInnerSumRelativeV((0, 1))
            .SizeInnerMaxRelativeV((1, 0))
            .InnerSpacingV(12)
            .InnerLayoutV(InnerLayout.VerticalList);
        for (int y = 0; y < 5; y++)
        {
            Node(blocksVert, out var blocksHor)
                .SizeInnerSumRelativeV((1, 0))
                .SizeInnerMaxRelativeV((0, 1))
                .InnerSpacingV(12)
                .InnerLayoutV(InnerLayout.HorizontalList);

            for (int x = 0; x < HotBarSlots.Count; x++)
            {
                Vector2i loc = (x, y);
                bool added = false;

                Node(blocksHor, out var square)
                    .Mut(s.Button)
                    .SizeV((120, 120))
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
            .SizeInnerSumRelativeV((1, 0))
            .SizeInnerMaxRelativeV((0, 1))
            .InnerSpacingV(12)
            .InnerLayoutV(InnerLayout.HorizontalList);
        for (int x = 0; x < HotBarSlots.Count; x++)
        {
            bool added = false;
            int i = x;

            Node(hotbar, out var square)
                .Mut(s.Button)
                .SizeV((120, 120))
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

        return menu;
    }
}
