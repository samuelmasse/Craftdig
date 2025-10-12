namespace Crafthoe.Dev;

[Root]
public class RootLoadNativeState(RootState state, RootScope scope) : State
{
    public override void Load()
    {
        var app = scope.Scope<AppScope>();
        app.Add(new AppMods([new(typeof(ModuleNativeLoader))]));

        state.Current = app.New<AppInitializeState>();
    }
}
