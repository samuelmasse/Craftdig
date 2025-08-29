namespace Crafthoe.Frontend;

[App]
public class AppInitializeState(
    RootState state,
    RootScreen screen,
    RootControlsToml controlsToml,
    AppScope scope,
    AppFiles files) : State
{
    public override void Load()
    {
        controlsToml.AddFromFile(files["Controls.toml"]);
        screen.Title = "Crafthoe";
        screen.Size = screen.MonitorSize / 4 * 3;

        state.Current = scope.New<AppMenuState>();
    }
}
