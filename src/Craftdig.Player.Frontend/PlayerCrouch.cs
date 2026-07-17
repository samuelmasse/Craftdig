namespace Craftdig.Player.Frontend;

[Player]
public class PlayerCrouch(PlayerEnt ent)
{
    private const double CameraDrop = 0.35;
    private const double CameraHalfLife = 0.05;

    private double cameraOffset;

    public double CameraOffset => cameraOffset;

    public void Update(double delta)
    {
        double target = ent.IsCrouching ? -CameraDrop : 0;
        cameraOffset = target + (cameraOffset - target) * Math.Pow(0.5, delta / CameraHalfLife);
    }
}
