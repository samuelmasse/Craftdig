namespace Craftdig.Menus.Common;

[App]
public class AppReset
{
    private readonly List<Action> actions = [];

    public void Run()
    {
        foreach (var action in actions)
            action.Invoke();
    }

    public void Register(Action action) => actions.Add(action);
}
