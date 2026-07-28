namespace Craftdig;

[Root]
public class RootBootState(RootState state, RootScope scope, RootScripts scripts) : State
{
    public override void Load() => scope.Scope<AppScope>()
        .With(x => new AppMods(x.Get<AppModFinder>().Find()))
        .Run(x => x.Scope<AppLoaderScope>()
            .Run(x => x.Get<AppFrontendLoader>().Run()))
        .Run(x => scripts.Add(x.Get<AppScript>()))
        .Run(x => state.Current = x.New<AppInitializeState>());
}
