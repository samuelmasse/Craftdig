namespace AlvorEngine.Loop;

public class RootLoop(RootArgs args)
{
    public void Run()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        using var manager = new WindowManager(args.Window);
        var injector = new Injector();
        var root = injector.Scope<RootScope>();
        var glw = new RootGlw(args.Driver);

        root.Add(args);
        root.Add(glw);
        root.Add(new RootSprites(new(glw)));
        root.Add(new RootCanvas(manager));
        root.Add(new RootControls(manager));
        root.Add(new RootKeyboard(manager));
        root.Add(new RootMouse(manager));
        root.Add(new RootScreen(manager));

        root.Get<RootState>().Current = (State)root.New(args.BootState);
        var engine = root.Get<RootEngine>();

        manager.Update += engine.Update;
        manager.Render += engine.Render;
        manager.Unload += engine.Unload;
        manager.Run();
    }
}
