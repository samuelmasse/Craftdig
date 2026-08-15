namespace Craftdig;

[App]
public class AppZoomMenu(RootKeyboard keyboard, RootText text, AppSettings settings, AppStyle s)
{
    public void Create(EntMut root)
    {
        var sw = Stopwatch.StartNew();

        float last = settings.Scale;
        bool initial = true;

        Node(root)
            .OnUpdateF(() =>
            {
                int zoom = (int)Math.Round(settings.Scale * 8);
                int oldZoom = zoom;

                if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyPressedRepeated(Keys.Equal) && zoom < 32)
                    zoom++;

                if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyPressedRepeated(Keys.Minus) && zoom > 1)
                    zoom--;

                if (zoom != oldZoom)
                    settings.Scale = zoom / 8f;
            });

        Node(root)
            .Mutate(s.Label)
            .AlignmentV(Alignment.Top | Alignment.Right)
            .TextF(() => text.Format("{0}%", settings.Scale * 100))
            .TextAlignmentV(Alignment.Center)
            .OnUpdateF(() =>
            {
                if (settings.Scale != last)
                {
                    initial = false;
                    last = settings.Scale;
                    sw.Restart();
                }
            })
            .ColorF(() => s.TooltipColor * (1, 1, 1, 0.5f * Opacity()))
            .TextColorF(() => s.TextColor * (1, 1, 1, Opacity()))
            .SizeV((s.ItemSpacing, s.ItemSpacing));

        float Opacity() => initial ? 0 : Math.Clamp(3 - (float)sw.Elapsed.TotalSeconds * 4, 0, 1);
    }
}
