namespace Craftdig;

[Root]
public class RootBootState(
    InjectorScopeGraph graph,
    RootState state,
    RootScope scope,
    RootScripts scripts) : State
{
    public override void Load()
    {
        graph.Label(scope, "Craftdig");

        var app = graph.Scope<AppScope>(
                scope,
                "Craftdig app")
            .With(x => new AppMods(x.Get<AppModFinder>().Find()));
        graph.Run<AppLoaderScope>(
            app,
            "App load",
            loader => loader.Get<AppFrontendLoader>().Run());
        scripts.Add(app.Get<AppScript>());
        state.Current = app.New<AppInitializeState>();
    }
}
