namespace Crafthoe;

[Root]
public class RootBootState(RootState state, RootScope scope, RootScripts scripts) : State
{
    public override void Load()
    {
        var app = scope.Scope<AppScope>();
        var appLoader = app.Scope<AppLoaderScope>();

        app.Add(new AppMods(appLoader.Get<AppModFinder>().Find()));
        appLoader.Get<AppLoader>().Run();
        appLoader.Get<AppFrontendLoader>().Run();

        scripts.Add(app.Get<AppScript>());
        state.Current = app.New<AppInitializeState>();
    }
}
