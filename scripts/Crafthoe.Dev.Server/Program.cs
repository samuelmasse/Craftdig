var appScope = new Injector()
    .Scope<RootScope>()
    .Scope<AppScope>();

appScope.Add(new AppMods([
    new(typeof(ModuleNativeLoader), null),
    new(typeof(ModuleNativeBackendLoader), null)
]));

var serverScope = appScope
    .Scope<ModuleScope>()
    .Scope<WorldScope>()
    .Scope<ServerScope>();

serverScope.Get<ServerBoot>().Run(Path.Join(AppContext.BaseDirectory, "Data"));

serverScope.Add(new ServerDefaults()
{
    Allowlist = ["samuelmasse4@gmail.com"],
    NoAuth = true,
    DisableTls = true,
    EnableRawTcp = true
});
serverScope.Get<Server>().Run();
