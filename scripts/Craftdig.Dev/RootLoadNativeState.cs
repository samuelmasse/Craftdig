namespace Craftdig.Dev;

[Root]
public class RootLoadNativeState(RootState state, RootScope scope, RootScripts scripts) : State
{
    public override void Load() => scope.Scope<AppScope>()
        .With(new AppMods([
            new(typeof(ModuleNativeLoader), null),
            new(typeof(ModuleNativeBackendLoader), null),
            new(typeof(ModuleNativeFrontendLoader), null)]))
        .With(new AppClientOptions()
        {
            AllowRawTcp = true,
            AllowNoAuth = true,
            UseRawTcp = true,
            DefaultNoAuthUser = "testuser",
            NoAuthUser = "testuser"
        })
        .Run(x =>
        {
            var files = x.Get<AppFiles>();
            var res = Path.Join(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(
                            Path.GetDirectoryName(files.Root))))!, "res");

            foreach (var dir in Directory.GetDirectories(res))
                files.AddRoot(dir);
        })
        .Run(x => x.Scope<AppLoaderScope>()
            .Run(x => x.Get<AppLoader>().Run()))
        .Run(x => scripts.Add(x.Get<AppScript>()))
        .Run(x => state.Current = x.New<AppInitializeState>());
}
