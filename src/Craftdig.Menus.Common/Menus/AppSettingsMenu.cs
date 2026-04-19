namespace Craftdig.Menus.Common;

[App]
public class AppSettingsMenu(RootText text, AppSettings settings, AppStyle s)
{
    public void Create(EntMut root)
    {
        Node(root, out var list)
            .Mutate(s.VerticalList)
            .SizeV((s.ItemWidthL, 0))
            .AlignmentV(Alignment.Horizontal)
            .InnerSpacingV(s.ItemSpacing)
            .PaddingV((s.ItemSpacing, s.ItemSpacing, s.ItemSpacing, s.ItemSpacing))
            .AlignmentV(Alignment.Center)
            .ColorV(s.BoardColor);
        {
            Node(list)
                .Mutate(s.LabelDark)
                .TextV("Settings")
                .AlignmentV(Alignment.Horizontal);

            Node(list)
                .Mutate(s.LabelDark)
                .TextF(() => text.Format("GUI Scale: {0}%", settings.Scale * 100))
                .AlignmentV(Alignment.Horizontal);

            Node(list, out var row)
                .Mutate(s.ButtonRow);
            {
                Node(row)
                    .Mutate(s.Button)
                    .TextV("-")
                    .OnPressF(() =>
                    {
                        int zoom = (int)Math.Round(settings.Scale * 8);
                        if (zoom > 1)
                            settings.Scale = (zoom - 1) / 8f;
                    });

                Node(row)
                    .Mutate(s.Button)
                    .TextV("+")
                    .OnPressF(() =>
                    {
                        int zoom = (int)Math.Round(settings.Scale * 8);
                        if (zoom < 32)
                            settings.Scale = (zoom + 1) / 8f;
                    });
            }

            Node(list)
                .Mutate(s.Button)
                .OnPressF(() => settings.Vsync = !settings.Vsync)
                .TextF(() => text.Format("VSync: {0}", settings.Vsync ? "On" : "Off"));

            Node(list)
                .Mutate(s.Button)
                .OnPressF(() => settings.Fullscreen = !settings.Fullscreen)
                .TextF(() => text.Format("Fullscreen: {0}", settings.Fullscreen ? "On" : "Off"));

            Node(list)
                .Mutate(s.Button)
                .OnPressF(() => PopMenu(root))
                .TextV("Back");
        }
    }
}
