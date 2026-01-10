namespace TrogloUI;

[Root]
public class RootUiFocus(RootKeyboard keyboard)
{
    private readonly List<EntObj?> focusables = [];
    private HashSet<EntObj> inits = [];
    private HashSet<EntObj> newInits = [];
    private EntObj? focused;

    public void Focus(EntObj? ent)
    {
        focused?.IsFocusedR(false);
        ent?.IsFocusedR(true);
        focused = ent;
    }

    internal void Update(EntObj n)
    {
        (inits, newInits) = (newInits, inits);
        focusables.Clear();
        newInits.Clear();
        CollectFocusables(n);

        int index = focusables.IndexOf(focused);
        if (index < 0)
        {
            EntObj? target = null;

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

    private void CollectFocusables(EntObj n)
    {
        var isFocusable = Get(n.GetIsFocusableV(), n.GetIsFocusableF());
        var isInputDisabled = Get(n.GetIsInputDisabledV(), n.GetIsInputDisabledF());

        if (isFocusable && !isInputDisabled)
        {
            focusables.Add(n);

            if (Get(n.GetIsInitialFocusV(), n.GetIsInitialFocusF()))
                newInits.Add(n);
        }

        foreach (var c in n.GetNodesR().Span)
            CollectFocusables(c);
    }
}
