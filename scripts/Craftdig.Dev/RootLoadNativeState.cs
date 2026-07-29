namespace Craftdig.Dev;

[Root]
public class RootLoadNativeState(
    Log log,
    Injector injector,
    RootState state,
    RootScope scope,
    RootScripts scripts) : State
{
    public override void Load()
    {
        // Set CRAFTDIG_DEV_IDENTITY=alice (any 4-35 char alphanumeric name starting with a letter) to
        // connect as a verified dev account with no Google login. Launch several instances with different
        // names and connect each to 127.0.0.1 to test multiple verified accounts. Leave it unset for the
        // usual raw-TCP no-auth development transport.
        string? devIdentity = Environment.GetEnvironmentVariable("CRAFTDIG_DEV_IDENTITY");

        var liveCode = new RootLiveCode(
            injector,
            scope,
            scripts,
            new("Craftdig.Dev"));
        var graph = liveCode.Enable();
        if (RootLivePatch.IsProfilerConfigured)
        {
            _ = new RootLivePatch(
                injector,
                scope,
                scripts,
                graph,
                liveCode.Bridges).Enable();
        }

        log.Info(
            $"Craftdig LiveCode {liveCode.Session.Name}"
            + $" {liveCode.Session.SessionId}");
        graph.Scope<AppScope>(
                scope,
                "Craftdig app")
            .With(new AppMods([
                new(typeof(ModuleNativeLoader), null),
                new(typeof(ModuleNativeBackendLoader), null),
                new(typeof(ModuleNativeFrontendLoader), null)]))
            .With(new DevIdentityConfig
            {
                Enabled = devIdentity != null,
                Name = devIdentity,
            })
            .With(new AppClientOptions()
            {
                AllowRawTcp = true,
                AllowNoAuth = true,
                UseRawTcp = devIdentity == null,
                DefaultNoAuthUser = "testuser",
                NoAuthUser = devIdentity == null ? "testuser" : null,
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
            .Run(x => scripts.Add(x.Get<AppScript>()))
            .Run(x => state.Current = x.New<AppInitializeState>());
    }
}
