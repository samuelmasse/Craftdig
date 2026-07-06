namespace Craftdig.Player.Frontend;

[Player]
public class PlayerFov(PlayerEnt ent, PlayerPerspective perspective)
{
    private bool init;

    public void Update(double delta)
    {
        float fov = 70;
        if (ent.IsFlying)
            fov += 10;
        if (ent.IsSprinting)
            fov += 10;

        if (!init)
        {
            perspective.Fov = fov;
            init = true;
        }

        perspective.Fov = (float)Math.Clamp(double.Lerp(perspective.Fov, fov, delta * 10), 70, 90);
    }
}
