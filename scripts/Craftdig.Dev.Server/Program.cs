var appScope = new Injector()
    .Scope<AppScope>();

appScope.Add(new AppMods([
    new(typeof(ModuleNativeLoader), null),
    new(typeof(ModuleNativeBackendLoader), null)
]));

appScope.Scope<AppLoaderScope>().Get<AppLoader>().Run();

var serverScope = appScope
    .Scope<ModuleScope>()
    .Scope<WorldScope>()
    .Scope<ServerScope>();

serverScope.Get<ServerBoot>().Run([
    $"--RootPath", Path.Join(AppContext.BaseDirectory, "Data"),
    "--LogLevel", "Trace",
    "--PublicServer", "true"]);

serverScope.Add(new ServerDefaults()
{
    NoAuth = true,
    DisableTls = false,
    EnableRawTcp = true
});

serverScope.Get<Server>().Run();
appScope.Scope<AppLoaderScope>().Get<AppUnloader>().Run();
