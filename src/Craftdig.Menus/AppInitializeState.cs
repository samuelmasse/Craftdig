namespace Craftdig.Menus;

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
        screen.Title = "Craftdig";
        screen.Size = screen.MonitorSize / 4 * 3;

        scripts.Add(root.Get<RootUiScript>());
        Node(ui).Mutate(mouseTrackMenu.Create);
        Node(ui).OrderValueV(2).Mutate(tooltipMenu.Create);
        Node(ui).OrderValueV(5).Mutate(zoomMenu.Create);

        scope.Scope<ModuleScope>()
            .Run(x => x.Handler(x.Get<ModuleEntMutInjector>()))
            .Run(x => x.Scope<ModuleLoaderScope>()
                .Run(x => x.Get<ModuleLoader>().Run())
                .Run(x => x.Get<ModuleFrontendLoader>().Run()))
            .Run(x => reset.Register(() => state.Current = x.New<ModuleMenuState>()));

        reset.Run();
    }
}
