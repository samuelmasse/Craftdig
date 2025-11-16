namespace Crafthoe;

[Root]
public class RootBootState(RootState state, RootScope scope) : State
{
    public override void Load()
    {
        var app = scope.Scope<AppScope>();
        var appLoader = app.Scope<AppLoaderScope>();

        app.Add(new AppMods(appLoader.Get<AppModFinder>().Find()));
        appLoader.Get<AppFrontendLoader>().Run();

        state.Current = app.New<AppInitializeState>();
    }
}
