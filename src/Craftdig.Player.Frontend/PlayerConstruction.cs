namespace Craftdig.Player.Frontend;

[Player]
public class PlayerConstruction(RootMouse mouse, WorldModuleIndices moduleIndices, PlayerEnt ent)
{
    private bool reject;
    private int mainCooldown;
    private int secondaryCooldown;

    public void Reject() => reject = true;

    public void Tick()
    {
        if (mainCooldown > 0)
            mainCooldown--;

        if (secondaryCooldown > 0)
            secondaryCooldown--;

        if (reject)
            return;

        if (mouse.IsMainDown() && mainCooldown <= 0)
        {
            ent.Ent.Construction() = new() { Action = ConstructionAction.Remove };
            mainCooldown = 5;
        }

        var hand = ent.Ent.HotBarSlots()[ent.Ent.HotBarIndex()].Item;
        if (hand.IsBuildable() && mouse.IsSecondaryDown() && secondaryCooldown <= 0)
        {
            ent.Ent.Construction() = new() { Action = ConstructionAction.Place, Arg = moduleIndices[hand] };
            secondaryCooldown = 4;
        }
    }

    public void Input()
    {
        if (!mouse.IsMainDown())
            mainCooldown = 0;

        if (!mouse.IsSecondaryDown())
            secondaryCooldown = 0;

        if (!mouse.IsMainDown() && !mouse.IsSecondaryDown())
            reject = false;
    }
}
