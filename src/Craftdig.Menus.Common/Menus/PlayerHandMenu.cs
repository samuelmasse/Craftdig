namespace Craftdig.Menus.Common;

[Player]
public class PlayerHandMenu(RootUiMouse mouse, AppStyle s, PlayerEnt player)
{
    public void Create(EntMut root)
    {
        Node(root)
            .Mutate(s.Slot)
            .ColorV(default)
            .TextureV(null)
            .GetSlotValueF(() => player.Offhand)
            .OffsetF(() =>
            {
                var pos = mouse.Position - (s.SlotSize / 2, s.SlotSize / 2);
                return (Snap.Round(pos.X, s.ItemSpacingXS), Snap.Round(pos.Y, s.ItemSpacingXS));
            })
            .SizeF(() => player.Offhand == default ? (0, 0) : (s.SlotSize, s.SlotSize))
            .TextColorF(() => player.Offhand == default ? (0, 0, 0, 0) : s.TextColor);
    }
}
