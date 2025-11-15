var appScope = new Injector()
    .Scope<RootScope>()
    .Scope<AppScope>();

appScope.Add(new AppMods([new(typeof(ModuleNativeLoader))]));

var moduleScope = appScope.Scope<ModuleScope>();
moduleScope.Handler(moduleScope.Get<ModuleEntMutInjector>());
moduleScope.Scope<ModuleLoaderScope>().Get<ModuleLoader>().Run();

var native = moduleScope.Get<ModuleNative>();
var worldScope = moduleScope.Scope<WorldScope>();
var exe = Assembly.GetExecutingAssembly().Location;
var exeDir = Path.GetDirectoryName(exe)!;
var serverDir = Path.Join(exeDir, "Data");
var worldDir = Path.Join(serverDir, "World");
worldScope.Add(new WorldPaths(worldDir));

var serverScope = worldScope.Scope<ServerScope>();
serverScope.Add(new ServerPaths(serverDir));
serverScope.Add(new ServerDefaults()
{
    Name = "Server",
    Difficulty = native.NormalDifficulty,
    GameMode = native.SurvivalGameMode,
    Allowlist = ["samuelmasse4@gmail.com"],
    NoAuth = true
});
serverScope.Get<Server>().Run();
