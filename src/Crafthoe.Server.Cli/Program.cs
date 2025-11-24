var result = new Cli(args).Run();
if (result != null)
    return result.Value;

var appScope = new Injector()
    .Scope<RootScope>()
    .Scope<AppScope>();

appScope.Add(new AppMods(
    appScope.Scope<AppLoaderScope>().Get<AppModFinder>().Find()));

appScope.Scope<AppLoaderScope>().Get<AppLoader>().Run();

var serverScope = appScope
    .Scope<ModuleScope>()
    .Scope<WorldScope>()
    .Scope<ServerScope>();

serverScope.Get<ServerBoot>().Run(args);
serverScope.Get<Server>().Run();
appScope.Scope<AppLoaderScope>().Get<AppUnloader>().Run();

return 0;
