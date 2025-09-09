namespace Crafthoe.Frontend;

[Player]
public class PlayerHandMenu(RootUiMouse mouse, AppStyle s, PlayerEnt player)
{
    public void Create(EntObj root)
    {
        Node(root)
            .OffsetF(() => mouse.Position - (s.SlotSize / 2, s.SlotSize / 2))
            .SizeF(() => player.Ent.Offhand() == default ? (0, 0) : (s.SlotSize, s.SlotSize))
            .SizeRelativeV((0, 0))
            .ColorV(s.ButtonColor);
    }
}
