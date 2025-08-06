namespace Crafthoe.Player;

[Player]
public class PlayerContext(
    RootMouse mouse,
    RootKeyboard keyboard,
    ModuleEnts ents,
    DimensionAir air,
    DimensionBlocks blocks,
    DimensionRigidBag rigidBag,
    PlayerScope scope,
    PlayerCamera camera,
    PlayerEnt ent,
    PlayerSelected selected,
    PlayerMovement movement,
    PlayerRenderer renderer)
{
    private Ent[] buildableBlocks = [];
    private Ent hand;

    public PlayerScope Scope => scope;

    public void Load()
    {
        ent.Ent.Position() = (15, 0, 100);
        rigidBag.Add((EntMut)ent.Ent);

        var blocks = new List<Ent>();
        foreach (var ent in ents.Span)
        {
            if (ent.IsBlock() && ent.IsBuildable())
                blocks.Add(ent);
        }

        buildableBlocks = [.. blocks];
    }

    public void Tick() => movement.Tick();

    public void Update(double time)
    {
        mouse.Track = true;
        camera.Rotate(-mouse.Delta / 300);
        camera.PreventBackFlipsAndFrontFlips();

        for (int i = 0; i < 9; i++)
        {
            var key = Keys.D1 + i;
            if (keyboard.IsKeyPressed(key))
            {
                if (i < buildableBlocks.Length)
                    hand = buildableBlocks[i];
                else hand = default;
            }
        }

        if (selected.Loc != null)
        {
            if (mouse.IsMainPressed())
                blocks.TrySet(selected.Loc.Value, air.Block);
            if (selected.Normal != null && hand.IsBuildable() && mouse.IsSecondaryPressed())
                blocks.TrySet(selected.Loc.Value + selected.Normal.Value, hand);
        }
    }

    public void Render() => renderer.Render();
}
