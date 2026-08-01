var result = new Cli(args).Run();
if (result != null)
    return result.Value;

using var logging = new LogRuntime();
logging.Start();

new Injector()
    .With(logging.Log)
    .Run(x => x.Add<Fn>(new FnBackend()))
    .Scope<AppScope>()
    .With(x => new AppMods(x.Get<AppModFinder>().Find()))
    .Run(x => x.Scope<ModuleScope>().Scope<WorldScope>().Scope<ServerScope>()
        .Run(x => x.Get<ServerBoot>().Run(args))
        .Run(x => x.Get<Server>().Run()));

return 0;
