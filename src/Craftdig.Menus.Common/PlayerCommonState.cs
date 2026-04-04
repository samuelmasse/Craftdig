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
    private readonly Dictionary<Keys, Action<EntMut>> keyMenus = new()
    {
        [Keys.Tab] = creativeInventoryMenu.Create,
        [Keys.E] = survivalInventoryMenu.Create,
    };
    private readonly EntMut menus = Node(ui).OrderValueV(1);
    private readonly EntMut overlay = Node(ui).Mutate(playerOverlayMenu.Create);
    private readonly EntMut hand = Node(ui).OrderValueV(1.5f).Mutate(playerHandMenu.Create);
    private readonly EntMut dark = Node(ui).ColorV((0.3f, 0.3f, 0.3f, 0.3f)).IsDisabledV(true);

    private Action<EntMut>? currentKeyMenu;
    private bool paused;
    private bool inv;

    public EntMut Menus => menus;
    public bool Paused => paused;
    public bool Inv => inv;

    public override void Load()
    {
        if (!ent.IsPlayer)
        {
            ent.IsPlayer = true;
            ent.IsRigid = true;
            ent.IsFlying = true;
            ent.HitBox = new Box3d((-0.3, -0.3, -1.62), (0.3, 0.3, 0.18));
            ent.Position = (15, 0, 120);
        }

        ent.IsSeer = true;
        ent.CanMove = true;
        ent.CanFly = true;
        ent.CanJump = true;
        ent.CanSprint = true;
        ent.PrevPosition = ent.Position;

        Node(menus).Mutate(debugMenu.Create);
        Node(menus).Mutate(dimensionSharedVertexBufferMenu.Create);
    }

    public override void Unload()
    {
        ui.Nodes.Remove(hand);
        ui.Nodes.Remove(menus);
        ui.Nodes.Remove(overlay);
        ui.Nodes.Remove(dark);
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
                NodeStack(menus).StackRootV(menus).Mutate(escapeMenu.Create);
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
                    NodeStack(menus).Mutate(keyMenus[key]);
                }
            }
        }

        if (menus.NodeStack.Count > 0 && dark.IsDisabledFV.Resolve())
            dark.Mutate().IsDisabledV(false);

        if (menus.NodeStack.Count == 0 && !dark.IsDisabledFV.Resolve())
        {
            paused = false;
            inv = false;
            currentKeyMenu = null;
            ent.Offhand = default;
            dark.Mutate().IsDisabledV(true);
        }

        mouse.Track = !paused && !inv;
        if (!mouse.Track)
            construction.Reject();
    }

    public override void Render()
    {
        dimensionFrontend.Frame();
        dimension.Frame();
        playerRenderer.Render(canvas.Size);
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
