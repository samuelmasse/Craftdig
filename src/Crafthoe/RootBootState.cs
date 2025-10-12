namespace Crafthoe;

[Root]
public class RootBootState(RootState state, RootScope scope) : State
{
    public override void Load()
    {
        var app = scope.Scope<AppScope>();

        // TODO: Find a way to search for mods to load
        app.Add(new AppMods([]));

        state.Current = app.New<AppInitializeState>();
    }
}
