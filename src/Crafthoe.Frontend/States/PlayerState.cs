namespace Crafthoe.Frontend;

[Player]
public class PlayerState(
    RootCanvas canvas,
    RootMouse mouse,
    RootKeyboard keyboard,
    RootSprites sprites,
    RootUi ui,
    RootUiSystem uiSystem,
    WorldTick tick,
    DimensionMetrics dimensionMetrics,
    DimensionContext dimension,
    PlayerEnt ent,
    PlayerContext player,
    PlayerDebugMenu debugMenu,
    PlayerEscapeMenu escapeMenu,
    PlayerOverlayMenu playerOverlayMenu,
    PlayerHandMenu playerHandMenu,
    PlayerCreativeInventoryMenu creativeInventoryMenu,
    PlayerSurvivalInventoryMenu survivalInventoryMenu) : State
{
    private readonly Dictionary<Keys, Action<EntObj>> keyMenus = new()
    {
        [Keys.Tab] = creativeInventoryMenu.Create,
        [Keys.E] = survivalInventoryMenu.Create,
    };
    private readonly EntObj menus = Node(ui).OrderValueV(1);
    private readonly EntObj overlay = Node(ui).Mut(playerOverlayMenu.Create);
    private readonly EntObj hand = Node(ui).OrderValueV(1.5f).Mut(playerHandMenu.Create);
    private readonly EntObj dark = Node().ColorV((0.3f, 0.3f, 0.3f, 0.3f));

    private Action<EntObj>? currentKeyMenu;
    private bool paused;
    private bool inv;

    public override void Load()
    {
        player.Load();
        Node(menus).Mut(debugMenu.Create);
    }

    public override void Unload()
    {
        ui.Nodes().Remove(hand);
        ui.Nodes().Remove(menus);
        ui.Nodes().Remove(overlay);
    }

    public override void Update(double time)
    {
        if (keyboard.IsKeyPressed(Keys.Escape))
        {
            if (menus.NodeStack().Count > 0)
                menus.NodeStack().Pop();
            else
            {
                paused = true;
                menus.NodeStack().Push(Node().StackRootV(menus).Mut(escapeMenu.Create));
            }
        }

        foreach (var key in keyMenus.Keys)
        {
            if (keyboard.IsKeyPressed(key))
            {
                if (menus.NodeStack().Count > 0)
                {
                    if (inv && currentKeyMenu == keyMenus[key])
                    {
                        while (menus.NodeStack().Count > 0)
                            menus.NodeStack().Pop();

                        inv = false;
                    }
                }
                else
                {
                    inv = true;
                    currentKeyMenu = keyMenus[key];
                    menus.NodeStack().Push(Node().Mut(keyMenus[key]));
                }
            }
        }

        if (menus.NodeStack().Count > 0 && !menus.Nodes().Contains(dark))
            menus.Nodes().Add(dark);

        if (menus.NodeStack().Count == 0 && menus.Nodes().Contains(dark))
        {
            paused = false;
            inv = false;
            currentKeyMenu = null;
            ent.Ent.Offhand() = default;
            menus.Nodes().Remove(dark);
        }

        mouse.Track = false;
        if (!paused)
        {
            int ticks = tick.Update(time);
            while (ticks > 0)
            {
                dimensionMetrics.TickMetric.Start();

                if (!inv)
                    player.Tick();

                dimension.Tick();
                ticks--;

                dimensionMetrics.TickMetric.End();
            }

            if (!inv)
                player.Update(time);
        }

        mouse.CursorState = mouse.Track ? CursorState.Grabbed : CursorState.Normal;
    }

    public override void Render()
    {
        dimension.Frame();
        player.Render();
    }

    public override void Draw()
    {
        float cht = 4 * uiSystem.Scale;
        float chl = cht * 9;
        var c = canvas.Size / 2;

        sprites.Batch.Draw(c - (cht / 2, chl / 2), (cht, chl));
        sprites.Batch.Draw(c - (chl / 2, cht / 2), (chl, cht));
    }
}
