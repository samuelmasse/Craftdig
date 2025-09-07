namespace Crafthoe.Frontend;

[App]
public class AppTooltipMenu(RootMouse mouse, RootUiMouse uiMouse, RootUiSystem uiSystem, AppStyle s)
{
    public EntObj Get()
    {
        Node(out var menu).SizeRelativeV((1, 1)).OrderValueV(2);

        Node(menu, out var text)
            .Mut(s.Label)
            .OffsetF(() => mouse.Position + (32, -64))
            .TextF(() =>
            {
                var hovered = uiMouse.Hovered;
                if (hovered == null)
                    return string.Empty;

                if (!hovered.HasTooltipV() && !hovered.HasTooltipF())
                    return string.Empty;

                return uiSystem.Get(hovered.TooltipV() ?? string.Empty, hovered.TooltipF());
            })
            .ColorV((0.5f, 0.28f, 1, 1));

        return menu;
    }
}
