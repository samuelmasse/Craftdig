namespace Crafthoe.Frontend;

[App]
public class AppMenuState(
    RootState state,
    RootBackbuffer backbuffer,
    RootScreen screen,
    RootMouse mouse,
    RootRoboto roboto,
    RootSprites sprites,
    RootScale scale,
    RootControlsToml controlsToml,
    AppScope scope,
    AppFiles files) : State
{
    private Stopwatch? watch;

    public override void Load()
    {
        controlsToml.AddFromFile(files["Controls.toml"]);
        screen.Title = "Crafthoe";
        screen.Size = screen.MonitorSize / 4 * 3;
        watch = Stopwatch.StartNew();
    }

    public override void Update(double time)
    {
        if (watch?.ElapsedMilliseconds > 30)
            screen.IsVisible = true;

        if (watch?.ElapsedMilliseconds > 250)
        {
            var moduleScope = scope.Scope<ModuleScope>();
            var worldScope = moduleScope.Scope<WorldScope>();
            var dimensionScope = worldScope.Scope<DimensionScope>();
            var playerScope = dimensionScope.Scope<PlayerScope>();
            state.Current = playerScope.New<PlayerState>();
        }
    }

    public override void Render() => backbuffer.Clear();

    public override void Draw()
    {
        sprites.Batch.Draw(mouse.Position, (scale[125], scale[250]), (0, 1, 1, 1));
        sprites.Batch.Write(roboto[scale[27]], "Hello Crafthoe", (scale[25], scale[25]));
    }
}
