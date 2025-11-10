namespace Crafthoe.Menus;

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
    AppMouseTrackMenu mouseTrackMenu,
    AppTooltipMenu tooltipMenu,
    AppReset reset,
    AppZoomMenu zoomMenu) : State
{
    public override void Load()
    {
        controlsToml.AddFromFile(files["Controls.toml"]);
        screen.Title = "Crafthoe";
        screen.Size = screen.MonitorSize / 4 * 3;

        scripts.Add(root.Get<RootUiScript>());
        ui.Nodes().Add(Node().Mut(mouseTrackMenu.Create));
        ui.Nodes().Add(Node().OrderValueV(2).Mut(tooltipMenu.Create));
        ui.Nodes().Add(Node().OrderValueV(5).Mut(zoomMenu.Create));

        var module = scope.Scope<ModuleScope>();
        module.Handler(module.Get<ModuleEntMutInjector>());

        var moduleLoaderScope = module.Scope<ModuleLoaderScope>();
        moduleLoaderScope.Get<ModuleLoader>().Run();
        moduleLoaderScope.Get<ModuleFrontendLoader>().Run();

        reset.Register(() => state.Current = module.New<ModuleMenuState>());
        reset.Run();
    }
}
