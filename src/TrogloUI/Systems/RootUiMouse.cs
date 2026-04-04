namespace TrogloUI;

[Root]
public class RootUiMouse(RootMouse mouse, RootUiScale scale, RootUiFocus focus, RootUiClipping clipping)
{
    private const long DoubleClickMs = 500;

    private Vector2 position;
    private EntMut prevHovered;
    private EntMut pressedMain;
    private EntMut pressedSecondary;
    private bool prevMainDown;
    private bool prevSecondaryDown;
    private EntMut lastClickTarget;
    private long lastClickTicks;

    public Vector2 Position => position;
    public EntMut Hovered => prevHovered;

    internal void Update(Vector2 o, EntMut n)
    {
        position = mouse.Position / scale.Scale;

        var scrolled = FindScrolled(null, o, n);
        if (mouse.Wheel != default)
            scrolled.OnScrollFDelegate?.Invoke(mouse.Wheel);

        var hovered = FindHovered(null, o, n);

        if (hovered != prevHovered)
        {
            prevHovered.IsHoveredR = false;
            hovered.IsHoveredR = true;
        }

        if (mouse.IsMainDown())
        {
            if (!prevMainDown)
            {
                pressedMain = hovered;
                if (pressedMain != default)
                    OnLeftPress(pressedMain);
            }

            prevMainDown = true;
        }
        else
        {
            if (prevMainDown && pressedMain != default && pressedMain == hovered)
                OnLeftClick(pressedMain);

            pressedMain.IsPressedR = false;
            pressedMain = default;
            prevMainDown = false;
        }

        if (mouse.IsSecondaryDown())
        {
            if (!prevSecondaryDown)
            {
                pressedSecondary = hovered;
                if (pressedSecondary != default)
                    OnRightPress(pressedSecondary);
            }

            prevSecondaryDown = true;
        }
        else
        {
            if (prevSecondaryDown && pressedSecondary != default && pressedSecondary == hovered)
                OnRightClick(pressedSecondary);

            pressedSecondary.IsSecondaryPressedR = false;
            pressedSecondary = default;
            prevSecondaryDown = false;
        }

        prevHovered = hovered;

        mouse.Cursor = hovered != default
            ? (Get(hovered.CursorV, hovered.CursorFDelegate) ?? MouseCursor.Default)
            : MouseCursor.Default;
    }

    private void OnLeftPress(EntMut e)
    {
        if (!InputEnabled(e))
            return;

        if (Get(e.IsFocusableV, e.IsFocusableFDelegate) || Get(e.IsSilentFocusableV, e.IsSilentFocusableFDelegate))
            focus.Focus(e, false);

        e.IsPressedR = true;
        e.OnPressFDelegate?.Invoke();
    }

    private void OnLeftClick(EntMut e)
    {
        if (!InputEnabled(e))
            return;

        var now = Environment.TickCount64;

        if (lastClickTarget == e && now - lastClickTicks <= DoubleClickMs)
        {
            e.OnDoubleClickFDelegate?.Invoke();
            lastClickTarget = default;
            lastClickTicks = 0;
        }
        else
        {
            e.OnClickFDelegate?.Invoke();
            lastClickTarget = e;
            lastClickTicks = now;
        }
    }

    private void OnRightPress(EntMut e)
    {
        if (!InputEnabled(e))
            return;

        e.IsSecondaryPressedR = true;
        e.OnSecondaryPressFDelegate?.Invoke();
    }

    private void OnRightClick(EntMut e)
    {
        if (!InputEnabled(e))
            return;

        e.OnSecondaryClickFDelegate?.Invoke();
    }

    private EntMut FindHovered(Box2? clip, Vector2 o, EntMut n)
    {
        var nOffset = o + n.OffsetR;
        var nSize = n.SizeR;
        var box = clipping.IntersectClips(clip, new Box2(nOffset, nOffset + nSize));

        EntMut hovered = default;

        if (box.ContainsInclusive(position) && Get(n.IsSelectableV, n.IsSelectableFDelegate))
            hovered = n;

        foreach (var c in n.NodesR.Span)
        {
            var child = FindHovered(box, nOffset, c);
            if (child != default)
                hovered = child;
        }

        return hovered;
    }

    private EntMut FindScrolled(Box2? clip, Vector2 o, EntMut n)
    {
        var nOffset = o + n.OffsetR;
        var nSize = n.SizeR;
        var box = clipping.IntersectClips(clip, new Box2(nOffset, nOffset + nSize));

        EntMut scrolled = default;

        if (box.ContainsInclusive(position) && Get(n.IsScrollableV, n.IsScrollableFDelegate))
            scrolled = n;

        foreach (var c in n.NodesR.Span)
        {
            var child = FindScrolled(box, nOffset, c);
            if (child != default)
                scrolled = child;
        }

        return scrolled;
    }

    private bool InputEnabled(EntMut n) => !Get(n.IsInputDisabledV, n.IsInputDisabledFDelegate);
}
