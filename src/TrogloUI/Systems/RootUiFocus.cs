namespace TrogloUI;

[Root]
public class RootUiFocus(RootKeyboard keyboard)
{
    private readonly List<EntMut> focusables = [];
    private HashSet<EntMut> inits = [];
    private HashSet<EntMut> newInits = [];
    private EntMut focused;

    public void Focus(EntMut ent)
    {
        var defer = Get(ent.DeferFocusV, ent.DeferFocusFDelegate);
        if (defer != default && DeferFocus(defer))
            return;

        focused.IsFocusedR = false;
        ent.IsFocusedR = true;
        focused = ent;

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

    internal void Update(EntMut n)
    {
        (inits, newInits) = (newInits, inits);
        focusables.Clear();
        newInits.Clear();
        CollectFocusables(n);

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

            Focus(target);
        }

        if (focusables.Count > 0 && keyboard.IsKeyPressedRepeated(Keys.Tab))
        {
            index = (index + 1) % focusables.Count;
            Focus(focusables[index]);
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
}
