namespace Crafthoe.Player.Frontend;

[Player]
public class PlayerConstruction(RootMouse mouse, WorldModuleIndices moduleIndices, PlayerEnt ent)
{
    private int mainCooldown;
    private int secondaryCooldown;

    public void Tick()
    {
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
    }
}
