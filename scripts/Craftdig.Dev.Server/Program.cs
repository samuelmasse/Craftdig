new Injector().Scope<AppScope>()
    .With(new AppMods([new(typeof(ModuleNativeLoader), null), new(typeof(ModuleNativeBackendLoader), null)]))
    .Run(x => x.Scope<AppLoaderScope>().Get<AppLoader>().Run())
    .Run(x => x.Scope<ModuleScope>().Scope<WorldScope>().Scope<ServerScope>()
        .With(new ServerDefaults()
        {
            NoAuth = true,
            DisableTls = false,
            EnableRawTcp = true
        })
        .Run(x => x.Get<ServerBoot>().Run([
            $"--RootPath", Path.Join(AppContext.BaseDirectory, "Data"),
            "--LogLevel", "Trace",
            "--PublicServer", "true"]))
        .Run(x => x.Get<Server>().Run()))
    .Run(x => x.Scope<AppLoaderScope>().Get<AppUnloader>().Run());
