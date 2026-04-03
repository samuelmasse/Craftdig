namespace TrogloUI;

[Root]
public class RootUiMouse(RootMouse mouse, RootUiScale scale, RootUiFocus focus, RootUiClipping clipping)
{
    private const long DoubleClickMs = 500;

    private Vector2 position;
    private EntObj? prevHovered;
    private EntObj? pressedMain;
    private EntObj? pressedSecondary;
    private bool prevMainDown;
    private bool prevSecondaryDown;
    private EntObj? lastClickTarget;
    private long lastClickTicks;

    public Vector2 Position => position;
    public EntObj? Hovered => prevHovered;

    internal void Update(Vector2 o, EntObj n)
    {
        position = mouse.Position / scale.Scale;

        if (Environment.TickCount64 - lastClickTicks > DoubleClickMs)
            lastClickTarget = null;

        var hovered = FindHovered(null, o, n);

        if (hovered != prevHovered)
        {
            prevHovered?.IsHoveredR = false;
            hovered?.IsHoveredR = true;
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
            ? (Get(hovered.CursorV, hovered.CursorFDelegate) ?? MouseCursor.Default)
            : MouseCursor.Default;
    }

    private void OnLeftPress(EntObj e)
    {
        if (!InputEnabled(e))
            return;

        if (Get(e.IsFocusableV, e.IsFocusableFDelegate))
            focus.Focus(e);

        e.OnPressFDelegate?.Invoke();
    }

    private void OnLeftClick(EntObj e)
    {
        if (!InputEnabled(e))
            return;

        var now = Environment.TickCount64;

        if (lastClickTarget == e && now - lastClickTicks <= DoubleClickMs)
        {
            e.OnDoubleClickFDelegate?.Invoke();
            lastClickTarget = null;
            lastClickTicks = 0;
        }
        else
        {
            e.OnClickFDelegate?.Invoke();
            lastClickTarget = e;
            lastClickTicks = now;
        }
    }

    private void OnRightPress(EntObj e)
    {
        if (!InputEnabled(e))
            return;

        e.OnSecondaryPressFDelegate?.Invoke();
    }

    private void OnRightClick(EntObj e)
    {
        if (!InputEnabled(e))
            return;

        e.OnSecondaryClickFDelegate?.Invoke();
    }

    private EntObj? FindHovered(Box2? clip, Vector2 o, EntObj n)
    {
        var nOffset = o + n.OffsetR;
        var nSize = n.SizeR;
        var box = clipping.IntersectClips(clip, new Box2(nOffset, nOffset + nSize));

        EntObj? hovered = null;

        if (box.ContainsInclusive(position) && Get(n.IsSelectableV, n.IsSelectableFDelegate))
            hovered = n;

        foreach (var c in n.NodesR.Span)
        {
            var child = FindHovered(box, nOffset, c);
            if (child != null)
                hovered = child;
        }

        return hovered;
    }

    private bool InputEnabled(EntObj n) => !Get(n.IsInputDisabledV, n.IsInputDisabledFDelegate);
}
