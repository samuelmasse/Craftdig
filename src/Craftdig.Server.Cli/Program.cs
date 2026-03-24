var result = new Cli(args).Run();
if (result != null)
    return result.Value;

new Injector().Scope<AppScope>()
    .With(x => new AppMods(x.Get<AppModFinder>().Find()))
    .Run(x => x.Scope<AppLoaderScope>().Get<AppLoader>().Run())
    .Run(x => x.Scope<ModuleScope>().Scope<WorldScope>().Scope<ServerScope>()
        .Run(x => x.Get<ServerBoot>().Run(args))
        .Run(x => x.Get<Server>().Run()))
    .Run(x => x.Scope<AppLoaderScope>().Get<AppUnloader>().Run());

return 0;
