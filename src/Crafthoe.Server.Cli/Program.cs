var rootArg = new Argument<DirectoryInfo?>("root")
{
    Description = "Root directory of the server",
    Arity = ArgumentArity.ZeroOrOne
};

var rootCommand = new RootCommand("Crafthoe Server") { rootArg };

rootCommand.SetAction(parseResult =>
{
    var dir = parseResult.GetValue(rootArg);

    var appScope = new Injector()
        .Scope<RootScope>()
        .Scope<AppScope>();

    appScope.Add(new AppMods(
        appScope.Scope<AppLoaderScope>().Get<AppModFinder>().Find()));

    var serverScope = appScope
        .Scope<ModuleScope>()
        .Scope<WorldScope>()
        .Scope<ServerScope>();

    serverScope.Get<ServerBoot>().Run(dir?.FullName);
    serverScope.Get<Server>().Run();
});

return await rootCommand.Parse(args).InvokeAsync();
