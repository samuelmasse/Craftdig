using Craftdig.Dev;

new RootLoop(new()
{
    Window = new WindowOpenTK(new(new(), new() { StartVisible = false })),
    Driver = new GldOpenTK(),
    BootState = typeof(RootLoadNativeState)
}).Run();
