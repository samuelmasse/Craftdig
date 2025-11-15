namespace TrogloUI;

[Root]
public class RootUiMouse(RootMouse mouse, RootUiScale scale, RootUiFocus focus)
{
    private Vector2 position;
    private EntObj? prevHovered;
    private EntObj? pressedMain;
    private EntObj? pressedSecondary;
    private bool prevMainDown;
    private bool prevSecondaryDown;
    private int c;

    public Vector2 Position => position;
    public EntObj? Hovered => prevHovered;

    internal void Update(Vector2 o, EntObj n)
    {
        position = mouse.Position / scale.Scale;

        var hovered = FindHovered(o, n);

        if (hovered != prevHovered)
        {
            prevHovered?.IsHoveredR(false);
            hovered?.IsHoveredR(true);
        }

        if (mouse.IsMainDown())
        {
            if (!prevMainDown)
            {
                pressedMain = hovered;
                if (pressedMain != null)
                    OnLeftPress(pressedMain);
            }

            prevMainDown = true;
        }
        else
        {
            if (prevMainDown && pressedMain != null && pressedMain == hovered)
                OnLeftClick(pressedMain);

            pressedMain = null;
            prevMainDown = false;
        }

        if (mouse.IsSecondaryDown())
        {
            if (!prevSecondaryDown)
            {
                pressedSecondary = hovered;
                if (pressedSecondary != null)
                    OnRightPress(pressedSecondary);
            }

            prevSecondaryDown = true;
        }
        else
        {
            if (prevSecondaryDown && pressedSecondary != null && pressedSecondary == hovered)
                OnRightClick(pressedSecondary);

            pressedSecondary = null;
            prevSecondaryDown = false;
        }

        prevHovered = hovered;

        mouse.Cursor = hovered != null
            ? (Get(hovered.GetCursorV(), hovered.GetCursorF()) ?? MouseCursor.Default)
            : MouseCursor.Default;
    }

    private void OnLeftPress(EntObj e)
    {
        if (!InputEnabled(e))
            return;

        if (Get(e.IsFocusableV(), e.IsFocusableF()))
            focus.Focus(e);

        e.OnPressF()?.Invoke();
    }

    private void OnLeftClick(EntObj e)
    {
        if (!InputEnabled(e))
            return;

        e.OnClickF()?.Invoke();
    }

    private void OnRightPress(EntObj e)
    {
        if (!InputEnabled(e))
            return;

        e.OnSecondaryPressF()?.Invoke();
    }

    private void OnRightClick(EntObj e)
    {
        if (!InputEnabled(e))
            return;

        e.OnSecondaryClickF()?.Invoke();
    }

    private EntObj? FindHovered(Vector2 o, EntObj n)
    {
        var nOffset = o + n.OffsetR();
        var nSize = n.SizeR();
        var box = new Box2(nOffset, nOffset + nSize);

        EntObj? hovered = null;

        if (box.ContainsInclusive(position) && Get(n.GetIsSelectableV(), n.GetIsSelectableF()))
            hovered = n;

        foreach (var c in n.GetNodesR())
        {
            var child = FindHovered(nOffset, c);
            if (child != null)
                hovered = child;
        }

        return hovered;
    }

    private bool InputEnabled(EntObj n) => !Get(n.IsInputDisabledV(), n.IsInputDisabledF());
}
