namespace Craftdig.Player.Frontend;

[Player]
public class PlayerConstruction(RootMouse mouse, WorldModuleIndices moduleIndices, PlayerEnt ent, PlayerControls controls)
{
    private bool reject;
    private bool drop;
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

        var hand = ent.GetHotBarSlot(ent.HotBarIndex).Item;

        if (drop)
        {
            if (hand.IsBuildable)
                ent.Construction = new() { Action = ConstructionAction.Drop, Arg = moduleIndices[hand] };

            drop = false;
            return;
        }

        if (mouse.IsMainDown() && mainCooldown <= 0)
        {
            ent.Construction = new() { Action = ConstructionAction.Remove };
            mainCooldown = 5;
        }

        if (hand.IsBuildable && mouse.IsSecondaryDown() && secondaryCooldown <= 0)
        {
            ent.Construction = new() { Action = ConstructionAction.Place, Arg = moduleIndices[hand] };
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

        if (controls.DropItem.Run())
            drop = true;
    }
}
