RootLoop.Run(() => new()
{
    Window = new WindowOpenTK(new(new(), new() { StartVisible = false })),
    Driver = new GlwDriverOpenTK(),
    BootState = typeof(RootBootState)
});
