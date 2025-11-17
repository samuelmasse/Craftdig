string[] help = ["-?", "-h", "--help"];

if (help.Any(x => args.Contains(x)))
{
    Console.WriteLine();
    Console.WriteLine($"Usage:");
    Console.WriteLine(" CrafthoeServer [options]");
    Console.WriteLine();
    Console.WriteLine("Run a Crafthoe server");
    Console.WriteLine();
    Console.WriteLine("Options:");

    var type = typeof(ServerConfig);
    var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

    foreach (var item in properties)
        Console.WriteLine($" --{item.Name}");

    return;
}

var appScope = new Injector()
    .Scope<RootScope>()
    .Scope<AppScope>();

appScope.Add(new AppMods(
    appScope.Scope<AppLoaderScope>().Get<AppModFinder>().Find()));

var serverScope = appScope
    .Scope<ModuleScope>()
    .Scope<WorldScope>()
    .Scope<ServerScope>();

serverScope.Get<ServerBoot>().Run(args);
serverScope.Get<Server>().Run();
