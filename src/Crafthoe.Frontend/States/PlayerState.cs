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
    PlayerHand playerHand,
    PlayerContext player,
    PlayerDebugMenu debugMenu,
    PlayerEscapeMenu escapeMenu,
    PlayerOverlayMenu playerOverlayMenu,
    PlayerHandMenu playerHandMenu,
    PlayerCreativeInventoryMenu creativeInventoryMenu) : State
{
    private readonly EntObj menus = Node(ui).SizeRelativeV((1, 1)).OrderValueV(1);
    private readonly EntObj overlay = Node(ui).SizeRelativeV((1, 1)).Mut(playerOverlayMenu.Create);
    private readonly EntObj hand = Node(ui).SizeRelativeV((1, 1)).OrderValueV(1.5f).Mut(playerHandMenu.Create);
    private readonly EntObj dark = Node().SizeRelativeV((1, 1)).ColorV((0.3f, 0.3f, 0.3f, 0.3f));
    private bool paused;
    private bool inv;

    public override void Load()
    {
        player.Load();
        Node(menus).SizeRelativeV((1, 1)).Mut(debugMenu.Create);
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
                menus.NodeStack().Push(Node().SizeRelativeV((1, 1)).StackRootV(menus).Mut(escapeMenu.Create));
            }
        }

        if (keyboard.IsKeyPressed(Keys.E))
        {
            if (menus.NodeStack().Count > 0)
            {
                if (inv)
                {
                    while (menus.NodeStack().Count > 0)
                        menus.NodeStack().Pop();

                    inv = false;
                }
            }
            else
            {
                inv = true;
                menus.NodeStack().Push(Node().SizeRelativeV((1, 1)).Mut(creativeInventoryMenu.Create));
            }
        }

        if (menus.NodeStack().Count > 0 && !menus.Nodes().Contains(dark))
            menus.Nodes().Add(dark);

        if (menus.NodeStack().Count == 0 && menus.Nodes().Contains(dark))
        {
            paused = false;
            inv = false;
            playerHand.Ent = default;
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
        float cht = 4;
        float chl = cht * 9;
        var c = (canvas.Size / uiSystem.Scale) / 2;

        sprites.Batch.Draw(c - (cht / 2, chl / 2), (cht, chl));
        sprites.Batch.Draw(c - (chl / 2, cht / 2), (chl, cht));
    }
}
