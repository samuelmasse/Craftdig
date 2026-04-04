namespace Craftdig.Menus.Common;

[Player]
public class PlayerHandMenu(RootUiMouse mouse, AppStyle s, PlayerEnt player)
{
    public void Create(EntMut root)
    {
        Node(root)
            .Mutate(s.Slot)
            .TextureV(null)
            .GetSlotValueF(() => player.Offhand)
            .OffsetF(() => mouse.Position - (s.SlotSize / 2, s.SlotSize / 2))
            .SizeF(() => player.Offhand == default ? (0, 0) : (s.SlotSize, s.SlotSize))
            .TextColorF(() => player.Offhand == default ? (0, 0, 0, 0) : s.TextColor);
    }
}
