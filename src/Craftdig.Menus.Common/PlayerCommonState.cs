namespace Craftdig;

[Player]
public class PlayerCommonState(
    RootCanvas canvas,
    RootInput input,
    RootUi ui,
    AppMenuVisibility visibility,
    RootKeyboard keyboard,
    RootSprites sprites,
    DimensionContext dimension,
    DimensionFrontend dimensionFrontend,
    DimensionSharedVertexBufferMenu dimensionSharedVertexBufferMenu,
    PlayerEnt ent,
    PlayerRenderer playerRenderer,
    PlayerViewModelRenderer playerViewModelRenderer,
    PlayerConstruction construction,
    PlayerDebugMenu debugMenu,
    PlayerEscapeMenu escapeMenu,
    PlayerOverlayMenu playerOverlayMenu,
    PlayerHandMenu playerHandMenu,
    PlayerCreativeInventoryMenu creativeInventoryMenu,
    PlayerSurvivalInventoryMenu survivalInventoryMenu,
    AppStyle s) : State
{
    private readonly Dictionary<Keys, Action<EntMut>> keyMenus = new()
    {
        [Keys.G] = creativeInventoryMenu.Create,
        [Keys.E] = survivalInventoryMenu.Create,
    };
    private readonly EntMut menus = Node(visibility.Root)
        .OrderValueV(1)
        .InnerAlignmentSnapV(s.ItemSpacingXS);
    private readonly EntMut overlay = Node(visibility.Root)
        .InnerAlignmentSnapV(s.ItemSpacingXS)
        .Mutate(playerOverlayMenu.Create);
    private readonly EntMut hand = Node(visibility.Root)
        .OrderValueV(1.5f)
        .InnerAlignmentSnapV(s.ItemSpacingXS)
        .IsDisabledV(true)
        .Mutate(playerHandMenu.Create);
    private readonly EntMut dark = Node(visibility.Root)
        .ColorV((0.3f, 0.3f, 0.3f, 0.3f))
        .IsDisabledV(true);

    private Action<EntMut>? currentKeyMenu;
    private bool paused;
    private bool inv;

    public EntMut Overlay => overlay;
    public bool Paused => paused;
    public bool Playing => !visibility.Hidden && !paused && !inv;
    public bool ShowNameplates => !visibility.Hidden && !inv;

    public override void Load()
    {
        if (!ent.IsPlayer)
        {
            ent.IsPlayer = true;
            ent.IsRigid = true;
            ent.IsFlying = true;
            ent.HitBox = new Box3d((-0.3, -0.3, -1.62), (0.3, 0.3, 0.18));
            ent.Position = (15, 0, 120);
            ent.IsLoaded = true;
        }

        ent.IsSeer = true;
        ent.CanMove = true;
        ent.CanFly = true;
        ent.CanJump = true;
        ent.CanSprint = true;
        ent.PrevPosition = ent.Position;

        menus.Mutate().StackRootV(menus);
        Node(overlay).Mutate(debugMenu.Create);
        Node(overlay).Mutate(dimensionSharedVertexBufferMenu.Create);
    }

    public override void Unload()
    {
        SetMouseTracking(false);
        NodesRemove(visibility.Root, hand);
        NodesRemove(visibility.Root, menus);
        NodesRemove(visibility.Root, overlay);
        NodesRemove(visibility.Root, dark);
    }

    public override void Update(double time)
    {
        if (!visibility.Hidden)
            UpdateMenus();

        SetMouseTracking(Playing);
        if (Playing)
            ui.Focus.Focus(default, false);
        else construction.Reject();
    }

    private void SetMouseTracking(bool tracking)
    {
        input.Track = tracking;
        var mode = tracking ? CursorMode.Disabled : CursorMode.Normal;
        if (input.CursorMode != mode)
            input.CursorMode = mode;
    }

    private void UpdateMenus()
    {
        if (keyboard.IsKeyPressed(Keys.Escape))
        {
            if (NodeStackCount(menus) > 0)
                PopMenu(menus);
            else
            {
                paused = true;
                PushMenu(menus, escapeMenu.Create);
            }
        }

        foreach (var key in keyMenus.Keys)
        {
            if (keyboard.IsKeyPressed(key))
            {
                if (NodeStackCount(menus) > 0)
                {
                    if (inv && currentKeyMenu == keyMenus[key])
                    {
                        while (NodeStackCount(menus) > 0)
                            PopMenu(menus);

                        inv = false;
                        hand.Mutate().IsDisabledV(true);
                    }
                }
                else
                {
                    inv = true;
                    hand.Mutate().IsDisabledV(false);
                    currentKeyMenu = keyMenus[key];
                    PushMenu(menus, keyMenus[key]);
                }
            }
        }

        if (NodeStackCount(menus) > 0 && dark.IsDisabledFV.Resolve())
            dark.Mutate().IsDisabledV(false);

        if (NodeStackCount(menus) == 0 && !dark.IsDisabledFV.Resolve())
        {
            paused = false;
            inv = false;
            hand.Mutate().IsDisabledV(true);
            currentKeyMenu = null;
            dark.Mutate().IsDisabledV(true);
        }
    }

    public override void Render()
    {
        dimensionFrontend.Frame();
        dimension.Frame();
        playerRenderer.Render(canvas.Size);
        if (!visibility.Hidden)
            playerViewModelRenderer.Render(canvas.Size);
    }

    public override void Draw()
    {
        if (visibility.Hidden)
            return;

        float cht = 4 * ui.Scale.Scale;
        float chl = cht * 9;
        var c = (Vec2)canvas.Size / 2;

        sprites.Batch.Draw(c - (cht / 2, chl / 2), (cht, chl));
        sprites.Batch.Draw(c - (chl / 2, cht / 2), (chl, cht));
    }
}
