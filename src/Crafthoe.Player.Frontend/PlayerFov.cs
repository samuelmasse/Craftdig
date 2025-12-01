namespace Crafthoe.Player.Frontend;

[Player]
public class PlayerFov(PlayerEnt ent, PlayerPerspective perspective)
{
    public void Update(double delta)
    {
        float fov = 70;
        if (ent.Ent.IsFlying())
            fov += 10;
        if (ent.Ent.IsSprinting())
            fov += 10;

        perspective.Fov = (float)Math.Clamp(MathHelper.Lerp(perspective.Fov, fov, delta * 10), 70, 90);
    }
}
