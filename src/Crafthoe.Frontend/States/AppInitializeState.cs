namespace Crafthoe.Frontend;

[App]
public class AppInitializeState(
    RootScope root,
    RootState state,
    RootScreen screen,
    RootControlsToml controlsToml,
    RootScripts scripts,
    RootUi ui,
    AppScope scope,
    AppFiles files,
    AppTooltipMenu tooltipMenu,
    AppZoomMenu zoomMenu,
    AppTestClientAction testClientAction) : State
{
    public override void Load()
    {
        controlsToml.AddFromFile(files["Controls.toml"]);
        screen.Title = "Crafthoe";
        screen.Size = screen.MonitorSize / 4 * 3;

        scripts.Add(root.Get<RootUiScript>());
        ui.Nodes().Add(Node().OrderValueV(2).Mut(tooltipMenu.Create));
        ui.Nodes().Add(Node().OrderValueV(5).Mut(zoomMenu.Create));

        var module = scope.Scope<ModuleScope>();
        module.Handler(module.Get<ModuleEntMutInjector>());
        module.Scope<ModuleLoaderScope>().Get<ModuleLoader>().Run();

        state.Current = module.New<ModuleMenuState>();

        testClientAction.Run();
    }
}
