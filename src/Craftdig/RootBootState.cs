namespace Craftdig;

[Root]
public class RootBootState(
    Injector injector,
    RootState state,
    RootScope scope,
    RootScripts scripts) : State
{
    public override void Load()
    {
        var graph = new InjectorScopeGraph(
            scope,
            "Craftdig");
        injector.Add(graph);

        var app = graph.Scope<AppScope>(
                scope,
                "Craftdig app")
            .With(x => new AppMods(x.Get<AppModFinder>().Find()));
        graph.Run<AppLoaderScope>(
            app,
            loader => loader.Get<AppFrontendLoader>().Run(),
            "App load");
        scripts.Add(app.Get<AppScript>());
        state.Current = app.New<AppInitializeState>();
    }
}
