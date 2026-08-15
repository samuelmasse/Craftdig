namespace Craftdig;

public partial class AppStyle
{
    public void Slider(EntMut parent, int min, int max, Func<int> get, Action<int> set)
    {
        Node(parent, out var bar)
            .SizeV((0, ItemHeight))
            .SizeRelativeV((1, 0))
            .IsSelectableV(true)
            .ColorV((0, 0, 0, 0.3f));
        {
            float puckSize = ItemSpacingXS * 8;
            float pressMouseX = 0;
            int pressValue = 0;

            Node(bar, out var puck)
                .Mutate(Button)
                .SizeV((puckSize, 0))
                .SizeRelativeV((0, 1))
                .AlignmentV(Alignment.Left | Alignment.Vertical)
                .OffsetF(() =>
                {
                    float travel = bar.SizeR.X - puckSize;
                    if (travel <= 0)
                        return default;

                    float t = Math.Clamp((get() - min) / (float)(max - min), 0, 1);
                    return (t * travel, 0);
                })
                .CursorF(() => puck.IsPressedR ? CursorShape.ResizeHorizontal : CursorShape.Hand)
                .OnPressF(() =>
                {
                    pressMouseX = mouse.Position.X;
                    pressValue = get();
                })
                .OnUpdateF(() =>
                {
                    if (puck.IsPressedR)
                    {
                        float travel = bar.SizeR.X - puckSize;
                        if (travel <= 0)
                            return;

                        float dx = mouse.Position.X - pressMouseX;
                        float dt = dx / travel;
                        int dv = (int)Math.Round(dt * (max - min));
                        int newVal = Math.Clamp(pressValue + dv, min, max);

                        if (newVal != get())
                            set(newVal);

                        return;
                    }

                    if (puck.IsFocusedR)
                    {
                        if (keyboard.IsKeyPressedRepeated(Keys.Left) && get() > min)
                            set(get() - 1);

                        if (keyboard.IsKeyPressedRepeated(Keys.Right) && get() < max)
                            set(get() + 1);
                    }
                });
        }
    }
}
