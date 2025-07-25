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
    private int ticks;

    public override void Load()
    {
        screen.Title = "Crafthoe";
        screen.Size = (screen.MonitorSize / 4) * 3;
        screen.IsVsyncOn = true;
    }

    public override void Update(double time)
    {
        if (ticks > 20)
        {
            var moduleScope = scope.Scope<ModuleScope>();
            var worldScope = moduleScope.Scope<WorldScope>();
            var playerScope = worldScope.Scope<PlayerScope>();
            state.Current = playerScope.New<PlayerState>();
        }
        else if (ticks > 0)
            screen.IsVisible = true;

        ticks++;
    }

    public override void Render() => backbuffer.Clear();

    public override void Draw()
    {
        sprites.Batch.Draw(mouse.Position, (250, 500), (0, 1, 1, 1));
        sprites.Batch.Write(font.Value.Size(55), "Hello Crafthoe", (50, 50));
    }
}
