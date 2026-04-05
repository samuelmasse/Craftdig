namespace Craftdig.Menus;

[App]
public class AppTooltipMenu(RootUiMouse uiMouse, AppStyle s)
{
    public void Create(EntMut root)
    {
        Node(root, out var text)
            .Mutate(s.Label)
            .OffsetF(() => uiMouse.Position + (s.ItemSpacing, -s.ItemSpacingXL))
            .TextF(Value)
            .IsDisabledF(() => Value().Length == 0)
            .ColorV(s.TooltipColor);

        ReadOnlySpan<char> Value()
        {
            var hovered = uiMouse.Hovered;
            return hovered.TooltipFV.Resolve();
        }
    }
}
