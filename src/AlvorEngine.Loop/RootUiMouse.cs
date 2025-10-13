namespace AlvorEngine.Loop;

[Root]
public class RootUiMouse(RootMouse mouse, RootUiSystem uiSystem, RootUiFocus focus)
{
    private Vector2 position;
    private EntObj? prevHovered;
    private EntObj? pressed;
    private EntObj? secondaryPressed;
    private bool prevMouseDown;
    private bool prevSecondaryMouseDown;

    public Vector2 Position => position;
    public EntObj? Hovered => prevHovered;

    public void Update(Vector2 o, EntObj n)
    {
        position = mouse.Position / uiSystem.Scale;

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
                if (pressed != null && !Get(pressed.IsInputDisabledV(), pressed.IsInputDisabledF()))
                {
                    if (Get(pressed.IsFocuseableV(), pressed.IsFocuseableF()))
                        focus.Focus(pressed);
                    pressed.OnPressF()?.Invoke();
                }
            }

            prevMouseDown = true;
        }
        else
        {
            if (prevMouseDown)
            {
                if (pressed == hovered)
                {
                    if (pressed != null && !Get(pressed.IsInputDisabledV(), pressed.IsInputDisabledF()))
                        pressed.OnClickF()?.Invoke();
                }
            }

            pressed = null;
            prevMouseDown = false;
        }

        if (mouse.IsSecondaryDown())
        {
            if (!prevSecondaryMouseDown)
            {
                secondaryPressed = hovered;
                if (secondaryPressed != null && !Get(secondaryPressed.IsInputDisabledV(), secondaryPressed.IsInputDisabledF()))
                    secondaryPressed.OnSecondaryPressF()?.Invoke();
            }

            prevSecondaryMouseDown = true;
        }
        else
        {
            if (prevSecondaryMouseDown)
            {
                if (secondaryPressed == hovered)
                {
                    if (secondaryPressed != null && !Get(secondaryPressed.IsInputDisabledV(), secondaryPressed.IsInputDisabledF()))
                        secondaryPressed.OnSecondaryClickF()?.Invoke();
                }
            }

            secondaryPressed = null;
            prevSecondaryMouseDown = false;
        }

        prevHovered = hovered;

        if (hovered != null)
        {
            var cursor = Get(hovered.CursorV(), hovered.CursorF());
            mouse.Cursor = cursor ?? MouseCursor.Default;
        }
        else mouse.Cursor = MouseCursor.Default;
    }

    public EntObj? FindHovered(Vector2 o, EntObj n)
    {
        var box = new Box2(o + n.OffsetR(), o + n.OffsetR() + n.SizeR());

        EntObj? hovered = null;

        bool isSelectable = n.IsSelectableV();
        var f = n.IsSelectableF();
        if (f != null)
            isSelectable = f.Invoke();

        if (box.ContainsInclusive(position) && isSelectable)
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
