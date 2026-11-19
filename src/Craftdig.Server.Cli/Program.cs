var result = new Cli(args).Run();
if (result != null)
    return result.Value;

using var host = RootHost.Start();
var graph = host.Root.Get<InjectorScopeGraph>();
var app = graph.Scope<AppScope>(host.Root);
app.Add(new AppMods(app.Get<AppModFinder>().Find()));

var module = graph.Scope<ModuleScope>(app);
var world = graph.Scope<WorldScope>(module);
var server = graph.Scope<ServerScope>(world);
server.Get<ServerBoot>().Run(args);
server.Get<Server>().Run();

return 0;
