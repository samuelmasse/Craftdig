namespace Craftdig;

[App]
public class AppTooltipMenu(RootUi ui, AppStyle s)
{
    public void Create(EntMut root)
    {
        Node(root, out var text)
            .Mutate(s.Label)
            .OffsetF(() => ui.Mouse.Position + (s.ItemSpacing, -s.ItemSpacingXL))
            .TextF(Value)
            .IsDisabledF(() => Value().Length == 0)
            .ColorV(s.TooltipColor);

        ReadOnlySpan<char> Value()
        {
            var hovered = ui.Mouse.Hovered;
            return hovered.TooltipFV.Resolve();
        }
    }
}
