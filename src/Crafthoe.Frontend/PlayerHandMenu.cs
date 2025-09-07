namespace Crafthoe.Frontend;

[Player]
public class PlayerHandMenu(RootMouse mouse, PlayerHand hand)
{
    public EntObj Get()
    {
        Node(out var menu).SizeRelativeV((1, 1)).OrderValueV(1.5f);

        Node(menu)
            .OffsetF(() => mouse.Position - (60, 60))
            .SizeF(() => hand.Ent == default ? (0, 0) : (120, 120))
            .ColorV((1, 0, 1, 1));

        return menu;
    }
}
