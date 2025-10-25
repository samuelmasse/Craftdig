using Crafthoe.Server;

new Injector()
    .Scope<RootScope>()
    .Scope<AppScope>()
    .Get<AppServer>().Run();
