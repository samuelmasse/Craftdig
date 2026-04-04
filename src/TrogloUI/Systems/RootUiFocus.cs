namespace TrogloUI;

[Root]
public class RootUiFocus(RootKeyboard keyboard)
{
    private readonly List<EntMut> focusables = [];
    private HashSet<EntMut> inits = [];
    private HashSet<EntMut> newInits = [];
    private EntMut focused;
    private bool tabMode;

    public void Focus(EntMut ent, bool tab)
    {
        tabMode = tab;

        var defer = Get(ent.DeferFocusV, ent.DeferFocusFDelegate);
        if (defer != default && DeferFocus(defer))
            return;

        focused.IsFocusedR = false;
        ent.IsFocusedR = true;
        focused = ent;

        var focusGroup = Get(ent.FocusGroupV, ent.FocusGroupFDelegate);
        if (focusGroup != default)
        {
            RemoveSelect(focusGroup);
            focusGroup.SelectedR = ent;
        }
        else RemoveSelect(ent);

        ent.IsSelectedR = true;
        ent.OnFocusFDelegate?.Invoke();
    }

    private bool DeferFocus(EntMut ent)
    {
        if (ent == focused)
            return true;

        foreach (var c in ent.NodesR.Span)
        {
            if (DeferFocus(c))
                return true;
        }

        return false;
    }

    private void RemoveSelect(EntMut ent)
    {
        if (ent.IsSelectedR)
        {
            ent.IsSelectedR = false;
            ent.OnUnselectFDelegate?.Invoke();
        }

        foreach (var c in ent.NodesR.Span)
            RemoveSelect(c);
    }

    internal void Update(EntMut n)
    {
        (inits, newInits) = (newInits, inits);
        focusables.Clear();
        newInits.Clear();
        CollectFocusables(n);
        UnselectUnselectables(n);

        int index = focusables.IndexOf(focused);
        if (index < 0)
        {
            EntMut target = default;

            foreach (var ent in newInits)
            {
                if (!inits.Contains(ent))
                {
                    target = ent;
                    break;
                }
            }

            if (target == default && tabMode && focusables.Count > 0)
                target = focusables[0];

            Focus(target, tabMode);
        }

        var focusGroup = Get(focused.FocusGroupV, focused.FocusGroupFDelegate);

        if (focusables.Count > 0 && keyboard.IsKeyPressedRepeated(Keys.Tab))
        {
            int offset = 1;

            while (offset < focusables.Count)
            {
                var focusable = focusables[(index + offset) % focusables.Count];
                var nextFocuseGroup = Get(focusable.FocusGroupV, focusable.FocusGroupFDelegate);

                if (nextFocuseGroup == default || nextFocuseGroup != focusGroup)
                    break;

                offset++;
            }

            if (offset < focusables.Count)
            {
                var focusable = focusables[(index + offset) % focusables.Count];
                var nextFocuseGroup = Get(focusable.FocusGroupV, focusable.FocusGroupFDelegate);

                if (nextFocuseGroup != default)
                {
                    var selected = FindSelected(nextFocuseGroup);
                    if (selected != default)
                        focusable = selected;
                }

                Focus(focusable, true);
            }
        }
    }

    private void CollectFocusables(EntMut n)
    {
        var isFocusable = Get(n.IsFocusableV, n.IsFocusableFDelegate);
        var isInputDisabled = Get(n.IsInputDisabledV, n.IsInputDisabledFDelegate);

        if (isFocusable && !isInputDisabled)
        {
            focusables.Add(n);

            if (Get(n.IsInitialFocusV, n.IsInitialFocusFDelegate))
                newInits.Add(n);
        }

        foreach (var c in n.NodesR.Span)
            CollectFocusables(c);
    }

    private void UnselectUnselectables(EntMut n)
    {
        if (n.SelectedR != default)
        {
            if (!HasChild(n, n.SelectedR))
            {
                n.SelectedR.Mutate().IsSelectedR(false);
                n.SelectedR.OnUnselectFDelegate?.Invoke();
                n.SelectedR = default;
            }
        }

        foreach (var c in n.NodesR.Span)
            UnselectUnselectables(c);
    }

    private bool HasChild(EntMut n, EntMut child)
    {
        if (n == child)
            return true;

        foreach (var c in n.NodesR.Span)
        {
            if (HasChild(c, child))
                return true;
        }

        return false;
    }

    private EntMut FindSelected(EntMut ent)
    {
        if (ent.IsSelectedR)
            return ent;

        foreach (var c in ent.NodesR.Span)
        {
            var selected = FindSelected(c);
            if (selected != default)
                return selected;
        }

        return default;
    }
}
