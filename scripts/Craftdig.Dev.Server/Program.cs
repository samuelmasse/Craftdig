using var logging = new LogRuntime();
logging.Start();

new Injector()
    .With(logging.Log)
    .Run(x => x.Add<Fn>(new FnBackend()))
    .Scope<AppScope>()
    .With(new AppMods([new(typeof(ModuleNativeLoader), null), new(typeof(ModuleNativeBackendLoader), null)]))
    // Trust the local dev issuer key so clients launched with CRAFTDIG_DEV_IDENTITY connect as verified
    // accounts without Google. Dev-only; a production server never sets this and trusts craftdig.io.
    .With(new DevIdentityConfig() { Enabled = true })
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
            "--PublicServer", "true",
            "--PublicServerContexts:0", "127.0.0.1:36676"]))
        .Run(x => x.Get<Server>().Run()));
