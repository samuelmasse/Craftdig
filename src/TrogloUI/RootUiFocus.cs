namespace TrogloUI;

[Root]
public class RootUiFocus(RootKeyboard keyboard)
{
    private readonly List<EntObj?> focusables = [];
    private EntObj? focused;

    public void Focus(EntObj? ent)
    {
        focused?.IsFocusedR(false);
        ent?.IsFocusedR(true);
        focused = ent;
    }

    public void Update(EntObj n)
    {
        focusables.Clear();
        CollectFocusables(n);

        int index = focusables.IndexOf(focused);
        if (index < 0)
            Focus(null);

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
            focusables.Add(n);

        foreach (var c in n.GetNodesR())
            CollectFocusables(c);
    }
}
