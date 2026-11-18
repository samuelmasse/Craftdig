namespace Craftdig;

[App]
public class AppInitializeState(
    RootState state,
    RootScreen screen,
    RootControlsToml controlsToml,
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

        Node(ui.Default.Root).Mutate(mouseTrackMenu.Create);
        Node(ui.Default.Root).OrderValueV(2).Mutate(tooltipMenu.Create);
        Node(ui.Default.Root).OrderValueV(5).Mutate(zoomMenu.Create);
        Node(ui.Default.Root).OrderValueV(5).Mutate(fpsMenu.Create);

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
