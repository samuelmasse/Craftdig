new RootLoop(new()
{
    Window = new WindowOpenTK(new GameWindow(new(), new() { StartVisible = false })),
    Driver = new GldOpenTK(),
    BootState = typeof(RootBootState)
}).Run();
