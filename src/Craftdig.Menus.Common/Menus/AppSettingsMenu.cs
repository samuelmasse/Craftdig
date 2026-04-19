namespace Craftdig.Menus.Common;

[App]
public class AppSettingsMenu(RootUiScale scale, RootText text, AppStyle s)
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
                .TextF(() => text.Format("GUI Scale: {0}%", scale.Scale * 100))
                .AlignmentV(Alignment.Horizontal);

            Node(list, out var row)
                .Mutate(s.ButtonRow);
            {
                Node(row)
                    .Mutate(s.Button)
                    .TextV("-")
                    .OnPressF(() =>
                    {
                        int zoom = (int)Math.Round(scale.Scale * 8);
                        if (zoom > 1)
                            scale.Scale = (zoom - 1) / 8f;
                    });

                Node(row)
                    .Mutate(s.Button)
                    .TextV("+")
                    .OnPressF(() =>
                    {
                        int zoom = (int)Math.Round(scale.Scale * 8);
                        if (zoom < 32)
                            scale.Scale = (zoom + 1) / 8f;
                    });
            }

            Node(list)
                .Mutate(s.Button)
                .OnPressF(() => PopMenu(root))
                .TextV("Back");
        }
    }
}
