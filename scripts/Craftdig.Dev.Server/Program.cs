using var host = RootHost.Start();
var graph = host.Root.Get<InjectorScopeGraph>();
var app = graph.Scope<AppScope>(host.Root);
app.Add(new AppMods([new(typeof(ModuleNativeLoader), null), new(typeof(ModuleNativeBackendLoader), null)]));
// Trust the local dev issuer key so clients launched with CRAFTDIG_DEV_IDENTITY connect as verified
// accounts without Google. Dev-only; a production server never sets this and trusts craftdig.io.
app.Add(new AppDevIdentityConfig() { Enabled = true });

var module = graph.Scope<ModuleScope>(app);
var world = graph.Scope<WorldScope>(module);
var server = graph.Scope<ServerScope>(world);
server.Add(new ServerDefaults()
{
    NoAuth = true,
    DisableTls = false,
    EnableRawTcp = true
});
server.Get<ServerBoot>().Run([
    "--RootPath", Path.Join(AppContext.BaseDirectory, "Data"),
    "--LogLevel", "Trace",
    "--PublicServer", "true",
    "--PublicServerContexts:0", "127.0.0.1:36676"]);
server.Get<Server>().Run();
