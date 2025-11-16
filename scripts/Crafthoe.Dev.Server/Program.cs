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

var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
serverScope.Get<ServerBoot>().Run(Path.Join(exeDir, "Data"));

serverScope.Add(new ServerDefaults()
{
    Allowlist = ["samuelmasse4@gmail.com"],
    NoAuth = true,
    DisableTls = true,
    EnableRawTcp = true
});
serverScope.Get<Server>().Run();
