namespace Craftdig;

[App]
public class AppInitializeState(
    RootScope root,
    RootState state,
    RootScreen screen,
    RootControlsToml controlsToml,
    RootScripts scripts,
    RootUi ui,
    RootUnload unload,
    AppScope scope,
    InjectorScopeGraph graph,
    AppFiles files,
    AppLoadIconAction loadIconAction,
    AppMouseTrackMenu mouseTrackMenu,
    AppTooltipMenu tooltipMenu,
    AppReset reset,
    AppZoomMenu zoomMenu,
    AppFpsMenu fpsMenu) : State
{
    public override void Load()
    {
        controlsToml.AddFromFile(files["Controls.toml"]);
        screen.Title = "Craftdig";
        loadIconAction.Run();
        screen.Size = screen.MonitorSize / 4u * 3u;

        scripts.Add(root.Get<RootUiScript>());
        Node(ui).Mutate(mouseTrackMenu.Create);
        Node(ui).OrderValueV(2).Mutate(tooltipMenu.Create);
        Node(ui).OrderValueV(5).Mutate(zoomMenu.Create);
        Node(ui).OrderValueV(5).Mutate(fpsMenu.Create);

        var module = graph.Scope<ModuleScope>(
            scope,
            "Craftdig module");
        unload.Add(() => graph.End(module, ending => ending.Get<ModuleEntsMut>().Unload()));
        unload.Add(module.Get<ModuleWorldSession>().Unload);
        module.Handler(module.Get<ModuleEntMutInjector>());
        graph.Run<ModuleLoaderScope>(
            module,
            "Module load",
            loader =>
            {
                loader.Get<ModuleLoader>().Run();
                loader.Get<ModuleFrontendLoader>().Run();
            });
        reset.Register(
            () => state.Current =
                module.New<ModuleMenuState>());

        reset.Run();
    }
}
