namespace Crafthoe.Frontend;

[App]
public class AppInitializeState(
    RootState state,
    RootScreen screen,
    RootControlsToml controlsToml,
    RootUi ui,
    AppScope scope,
    AppFiles files,
    AppTooltipMenu tooltipMenu) : State
{
    public override void Load()
    {
        controlsToml.AddFromFile(files["Controls.toml"]);
        screen.Title = "Crafthoe";
        screen.Size = screen.MonitorSize / 4 * 3;

        ui.Nodes().Add(tooltipMenu.Get());

        state.Current = scope.New<AppMenuState>();
    }
}
