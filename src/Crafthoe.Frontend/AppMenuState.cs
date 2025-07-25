namespace Crafthoe.Frontend;

[App]
public class AppMenuState(
    RootState state,
    RootBackbuffer backbuffer,
    RootScreen screen,
    RootMouse mouse,
    RootSprites sprites,
    AppScope scope,
    AppFont font) : State
{
    private Stopwatch? watch;

    public override void Load()
    {
        screen.Title = "Crafthoe";
        screen.Size = (screen.MonitorSize / 4) * 3;
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
            var playerScope = worldScope.Scope<PlayerScope>();
            state.Current = playerScope.New<PlayerState>();
        }
    }

    public override void Render() => backbuffer.Clear();

    public override void Draw()
    {
        sprites.Batch.Draw(mouse.Position, (250, 500), (0, 1, 1, 1));
        sprites.Batch.Write(font.Value.Size(55), "Hello Crafthoe", (50, 50));
    }
}
