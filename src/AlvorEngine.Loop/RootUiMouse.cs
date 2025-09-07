namespace AlvorEngine.Loop;

[Root]
public class RootUiMouse(RootMouse mouse)
{
    private EntObj? prevHovered;
    private EntObj? pressed;
    private EntObj? secondaryPressed;
    private bool prevMouseDown;
    private bool prevSecondaryMouseDown;

    public EntObj? Hovered => prevHovered;

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
                if (pressed == hovered)
                    pressed?.OnClickF()?.Invoke();
            }

            pressed = null;
            prevMouseDown = false;
        }

        if (mouse.IsSecondaryDown())
        {
            if (!prevSecondaryMouseDown)
            {
                secondaryPressed = hovered;
                secondaryPressed?.OnSecondaryPressF()?.Invoke();
            }

            prevSecondaryMouseDown = true;
        }
        else
        {
            if (prevSecondaryMouseDown)
            {
                if (secondaryPressed == hovered)
                    secondaryPressed?.OnSecondaryClickF()?.Invoke();
            }

            secondaryPressed = null;
            prevSecondaryMouseDown = false;
        }

        prevHovered = hovered;
    }

    public EntObj? FindHovered(Vector2 o, EntObj n)
    {
        var box = new Box2(o + n.OffsetR(), o + n.OffsetR() + n.SizeR());

        EntObj? hovered = null;

        bool isSelectable = n.IsSelectableV();
        var f = n.IsSelectableF();
        if (f != null)
            isSelectable = f.Invoke();

        if (box.ContainsInclusive(mouse.Position) && isSelectable)
            hovered = n;

        foreach (var c in n.GetNodesR())
        {
            var chovered = FindHovered(o + n.OffsetR(), c);
            if (chovered != null)
                hovered = chovered;
        }

        return hovered;
    }
}
