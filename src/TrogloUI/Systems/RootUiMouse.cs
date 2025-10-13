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

        HandleButton(
            isDown: mouse.IsMainDown(),
            ref prevMainDown,
            ref pressedMain,
            hovered,
            onPress: e =>
            {
                if (!InputEnabled(e))
                    return;

                if (Get(e.IsFocusableV(), e.IsFocusableF()))
                    focus.Focus(e);

                e.OnPressF()?.Invoke();
            },
            onClick: e =>
            {
                if (!InputEnabled(e))
                    return;

                e.OnClickF()?.Invoke();
            }
        );

        HandleButton(
            isDown: mouse.IsSecondaryDown(),
            ref prevSecondaryDown,
            ref pressedSecondary,
            hovered,
            onPress: e =>
            {
                if (!InputEnabled(e))
                    return;

                e.OnSecondaryPressF()?.Invoke();
            },
            onClick: e =>
            {
                if (!InputEnabled(e))
                    return;

                e.OnSecondaryClickF()?.Invoke();
            }
        );

        prevHovered = hovered;

        mouse.Cursor = hovered != null
            ? (Get(hovered.GetCursorV(), hovered.GetCursorF()) ?? MouseCursor.Default)
            : MouseCursor.Default;
    }

    private void HandleButton(
        bool isDown,
        ref bool prevDown,
        ref EntObj? pressed,
        EntObj? hovered,
        Action<EntObj> onPress,
        Action<EntObj> onClick)
    {
        if (isDown)
        {
            if (!prevDown)
            {
                pressed = hovered;
                if (pressed != null)
                    onPress(pressed);
            }

            prevDown = true;
            return;
        }

        if (prevDown && pressed != null && pressed == hovered)
            onClick(pressed);

        pressed = null;
        prevDown = false;
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
