namespace AlvorEngine.Loop;

[Root]
public class RootUiMouse(RootMouse mouse)
{
    private EntObj? prevHovered;
    private EntObj? pressed;
    private bool prevMouseDown;

    public void Update(Vector2 o, EntObj n)
    {
        var hovered = FindHovered(o, n);

        if (hovered != prevHovered)
        {
            hovered?.IsHoveredR(true);
            prevHovered?.IsHoveredR(false);
        }

        if (mouse.IsMainDown())
        {
            if (!prevMouseDown)
            {
                pressed = hovered;
                pressed?.OnPressF()?.Invoke();
            }

            prevMouseDown = true;
        }
        else
        {
            if (prevMouseDown)
            {

            }

            pressed = null;
            prevMouseDown = false;
        }

        prevHovered = hovered;
    }

    public EntObj? FindHovered(Vector2 o, EntObj n)
    {
        var box = new Box2(o + n.OffsetR(), o + n.OffsetR() + n.SizeR());

        EntObj? hovered = null;

        if (box.ContainsInclusive(mouse.Position))
            hovered = n;

        foreach (var c in n.Nodes())
        {
            var chovered = FindHovered(o + n.OffsetR(), c);
            if (chovered != null)
                hovered = chovered;
        }

        return hovered;
    }
}
