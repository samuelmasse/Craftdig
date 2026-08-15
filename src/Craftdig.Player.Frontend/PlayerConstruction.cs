namespace Craftdig;

[Player]
public class PlayerConstruction(
    RootMouse mouse,
    WorldModuleIndices moduleIndices,
    PlayerEnt ent,
    PlayerControls controls,
    PlayerViewModelAnimation viewModelAnimation)
{
    private bool reject = true;
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

        var hand = ent.HotBarSlots[ent.HotBarIndex].Item;

        if (drop)
        {
            if (hand.IsBuildable)
                ent.Drop = new() { Action = DropAction.DropTest, Arg = moduleIndices[hand] };

            drop = false;
        }

        if (mouse.IsMainDown() && mainCooldown <= 0)
        {
            ent.Construction = new() { Action = ConstructionAction.Remove };
            viewModelAnimation.Primary();
            mainCooldown = 5;
        }

        if (hand.IsBuildable && mouse.IsSecondaryDown() && secondaryCooldown <= 0)
        {
            ent.Construction = new() { Action = ConstructionAction.Place, Arg = moduleIndices[hand] };
            viewModelAnimation.Secondary();
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
