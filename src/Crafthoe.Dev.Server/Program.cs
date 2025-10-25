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
var worldDir = Path.Join(exeDir, "Data", "World");
worldScope.Add(new WorldPaths(worldDir));
worldScope.Add(new WorldDefaults()
{
    Name = "Server",
    Difficulty = native.NormalDifficulty,
    GameMode = native.SurvivalGameMode
});
worldScope.Get<WorldServer>().Run();
