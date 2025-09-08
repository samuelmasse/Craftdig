namespace Crafthoe.Frontend;

[App]
public class AppZoomMenu(RootKeyboard keyboard, RootText text, RootUiSystem uiSystem, AppStyle s)
{
    public void Create(EntObj root)
    {
        int zoom = (int)(uiSystem.Scale * 8);

        Node(root)
            .OnUpdateF(() =>
            {
                if (keyboard.IsKeyPressedRepeated(Keys.Equal) && zoom < 32)
                    zoom++;

                if (keyboard.IsKeyPressedRepeated(Keys.Minus) && zoom > 1)
                    zoom--;

                uiSystem.Scale = zoom / 8f;
            });

        Node(root)
            .Mut(s.Label)
            .AlignmentV(Alignment.Top | Alignment.Right)
            .TextF(() => text.Format("{0}%", uiSystem.Scale * 100))
            .ColorV(s.TooltipColor * (1, 1, 1, 0.5f))
            .SizeV((s.ItemSpacing, s.ItemSpacing));
    }
}
