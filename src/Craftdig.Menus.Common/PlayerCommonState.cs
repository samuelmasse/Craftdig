namespace Craftdig.Menus.Common;

[Player]
public class PlayerCommonState(
    RootCanvas canvas,
    RootMouse mouse,
    RootKeyboard keyboard,
    RootSprites sprites,
    RootUi ui,
    RootUiScale scale,
    DimensionContext dimension,
    DimensionFrontend dimensionFrontend,
    DimensionSharedVertexBufferMenu dimensionSharedVertexBufferMenu,
    PlayerEnt ent,
    PlayerRenderer playerRenderer,
    PlayerConstruction construction,
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
    private readonly EntObj overlay = Node(ui).Mutate(playerOverlayMenu.Create);
    private readonly EntObj hand = Node(ui).OrderValueV(1.5f).Mutate(playerHandMenu.Create);
    private readonly EntObj dark = Node().ColorV((0.3f, 0.3f, 0.3f, 0.3f));

    private Action<EntObj>? currentKeyMenu;
    private bool paused;
    private bool inv;

    public EntObj Menus => menus;
    public bool Paused => paused;
    public bool Inv => inv;

    public override void Load()
    {
        ent.IsRigid = true;
        ent.HitBox = new Box3d((-0.3, -0.3, -1.62), (0.3, 0.3, 0.18));
        ent.Position = (15, 0, 120);
        ent.IsFlying = true;
        ent.CanMove = true;
        ent.CanFly = true;
        ent.CanJump = true;
        ent.CanSprint = true;

        Node(menus).Mutate(debugMenu.Create);
        Node(menus).Mutate(dimensionSharedVertexBufferMenu.Create);
    }

    public override void Unload()
    {
        ui.Nodes.Remove(hand);
        ui.Nodes.Remove(menus);
        ui.Nodes.Remove(overlay);
    }

    public override void Update(double time)
    {
        if (keyboard.IsKeyPressed(Keys.Escape))
        {
            if (menus.NodeStack.Count > 0)
                menus.NodeStack.Pop();
            else
            {
                paused = true;
                menus.NodeStack.Push(Node().StackRootV(menus).Mutate(escapeMenu.Create));
            }
        }

        foreach (var key in keyMenus.Keys)
        {
            if (keyboard.IsKeyPressed(key))
            {
                if (menus.NodeStack.Count > 0)
                {
                    if (inv && currentKeyMenu == keyMenus[key])
                    {
                        while (menus.NodeStack.Count > 0)
                            menus.NodeStack.Pop();

                        inv = false;
                    }
                }
                else
                {
                    inv = true;
                    currentKeyMenu = keyMenus[key];
                    menus.NodeStack.Push(Node().Mutate(keyMenus[key]));
                }
            }
        }

        if (menus.NodeStack.Count > 0 && !menus.Nodes.Contains(dark))
            menus.Nodes.Add(dark);

        if (menus.NodeStack.Count == 0 && menus.Nodes.Contains(dark))
        {
            paused = false;
            inv = false;
            currentKeyMenu = null;
            ent.SetOffhand(default);
            menus.Nodes.Remove(dark);
        }

        mouse.Track = !paused && !inv;
        if (!mouse.Track)
            construction.Reject();
    }

    public override void Render()
    {
        dimensionFrontend.Frame();
        dimension.Frame();
        playerRenderer.Render();
    }

    public override void Draw()
    {
        float cht = 4 * scale.Scale;
        float chl = cht * 9;
        var c = canvas.Size / 2;

        sprites.Batch.Draw(c - (cht / 2, chl / 2), (cht, chl));
        sprites.Batch.Draw(c - (chl / 2, cht / 2), (chl, cht));
    }
}
