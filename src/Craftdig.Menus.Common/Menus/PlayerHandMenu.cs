namespace Craftdig.Menus.Common;

[Player]
public class PlayerHandMenu(RootUiMouse mouse, AppStyle s, PlayerEnt player)
{
    public void Create(EntObj root)
    {
        Node(root)
            .Mutate(s.Slot)
            .TextureV(null)
            .GetSlotValueF(() => player.GetOffhand())
            .OffsetF(() => mouse.Position - (s.SlotSize / 2, s.SlotSize / 2))
            .SizeF(() => player.GetOffhand() == default ? (0, 0) : (s.SlotSize, s.SlotSize))
            .TextColorF(() => player.GetOffhand() == default ? (0, 0, 0, 0) : s.TextColor);
    }
}
