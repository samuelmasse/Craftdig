namespace Crafthoe.Frontend;

[App]
public class AppTooltipMenu(RootUiMouse uiMouse, RootUiSystem uiSystem, AppStyle s)
{
    public void Create(EntObj root)
    {
        Node(root, out var text)
            .Mut(s.Label)
            .OffsetF(() => uiMouse.Position + (s.ItemSpacing, -s.ItemSpacingXL))
            .TextF(() =>
            {
                var hovered = uiMouse.Hovered;
                if (hovered == null)
                    return string.Empty;

                if (!hovered.HasTooltipV() && !hovered.HasTooltipF())
                    return string.Empty;

                return uiSystem.Get(hovered.TooltipV() ?? string.Empty, hovered.TooltipF());
            })
            .ColorV(s.TooltipColor);
    }
}
