namespace Crafthoe.Frontend;

[App]
public class AppZoomMenu(RootKeyboard keyboard, RootText text, RootUiScale scale, AppStyle s)
{
    public void Create(EntObj root)
    {
        int zoom = (int)(scale.Scale * 8);
        var sw = Stopwatch.StartNew();

        float last = scale.Scale;
        bool initial = true;

        Node(root)
            .OnUpdateF(() =>
            {
                if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyPressedRepeated(Keys.Equal) && zoom < 32)
                    zoom++;

                if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyPressedRepeated(Keys.Minus) && zoom > 1)
                    zoom--;

                scale.Scale = zoom / 8f;
            });

        Node(root)
            .Mut(s.Label)
            .AlignmentV(Alignment.Top | Alignment.Right)
            .TextF(() => text.Format("{0}%", scale.Scale * 100))
            .OnUpdateF(() =>
            {
                if (scale.Scale != last)
                {
                    initial = false;
                    last = scale.Scale;
                    sw.Restart();
                }
            })
            .ColorF(() => s.TooltipColor * (1, 1, 1, 0.5f * Opacity()))
            .TextColorF(() => s.TextColor * (1, 1, 1, Opacity()))
            .SizeV((s.ItemSpacing, s.ItemSpacing));

        float Opacity() => initial ? 0 : Math.Clamp(3 - (float)sw.Elapsed.TotalSeconds * 4, 0, 1);
    }
}
