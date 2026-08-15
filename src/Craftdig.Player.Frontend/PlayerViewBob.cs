namespace Craftdig;

[Player]
public class PlayerViewBob(PlayerViewModelAnimation animation)
{
    private const float Amplitude = 0.1f;
    private const float RollScale = 3 * float.Pi / 180;
    private const float PitchScale = 5 * float.Pi / 180;

    public void Apply(ref Mat4 view)
    {
        float amount = animation.BobStrength * Amplitude;
        if (amount < 0.0001f)
            return;

        float phase = animation.BobPhase;
        float sway = MathF.Sin(phase) * amount;
        float bounce = MathF.Abs(MathF.Cos(phase)) * amount;
        view = Mat4.CreateTranslation((sway * 0.5f, -bounce, 0)) *
            Mat4.CreateRotationZ(sway * RollScale) *
            Mat4.CreateRotationX(bounce * PitchScale) *
            view;
    }
}
