namespace Crafthoe.Dev;

[Root]
public class RootLoadNativeState(RootState state, RootScope scope) : State
{
    public override void Load()
    {
        var app = scope.Scope<AppScope>();
        app.Add(new AppMods([
            new(typeof(ModuleNativeLoader), null),
            new(typeof(ModuleNativeBackendLoader), null),
            new(typeof(ModuleNativeFrontendLoader), null)
        ]));

        var user = "testuser";
        var options = new AppClientOptions()
        {
            AllowRawTcp = true,
            AllowNoAuth = true,
            UseRawTcp = true,
            DefaultNoAuthUser = user,
            NoAuthUser = user
        };

        app.Add(options);

        var files = app.Get<AppFiles>();
        var res = Path.Join(
            Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(files.Root))))!, "res");

        foreach (var dir in Directory.GetDirectories(res))
            files.AddRoot(dir);

        state.Current = app.New<AppInitializeState>();
    }
}
