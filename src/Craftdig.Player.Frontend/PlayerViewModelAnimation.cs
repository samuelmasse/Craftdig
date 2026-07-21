namespace Craftdig.Player.Frontend;

[Player]
public class PlayerViewModelAnimation(WorldTick tick, PlayerEnt ent)
{
    private const double PrimaryDuration = 0.3;
    private const double SecondaryDuration = 0.3;
    private const double EquipHalfDuration = 0.15;
    private const double BobHalfLife = 0.08;
    private const double LookHalfLife = 0.065;
    private const double CrouchHalfLife = 0.06;
    private const double StridePhase = 0.6 * Math.PI;
    private const float EquipDropDistance = 0.6f;

    private static readonly Vec3 ArmSwingPivot = (0.64f, -0.6f, -0.72f);
    private static readonly Vec2 SwingPivotDelta = (
        ArmSwingPivot.X - PlayerViewModelMesher.ArmShoulderRest.X,
        ArmSwingPivot.Z - PlayerViewModelMesher.ArmShoulderRest.Z);

    private Ent displayedItem;
    private Ent pendingItem;
    private double elapsed;
    private double bobPhase;
    private double bobStrength;
    private double equip;
    private double primaryTime = PrimaryDuration;
    private double secondaryTime = SecondaryDuration;
    private double crouch;
    private Vec2 look;
    private bool initialized;
    private bool equipLowering;

    public Ent DisplayedItem => displayedItem;
    public PlayerViewModelPose Pose { get; private set; }
    public float BobPhase => (float)bobPhase;
    public float BobStrength => (float)bobStrength;

    public void Primary() => primaryTime = 0;
    public void Secondary() => secondaryTime = 0;

    public void Look(Vec2 delta)
    {
        look += delta * 0.0015f;
        look.X = Math.Clamp(look.X, -0.12f, 0.12f);
        look.Y = Math.Clamp(look.Y, -0.10f, 0.10f);
    }

    public void Update(double delta)
    {
        elapsed += delta;
        primaryTime = Math.Min(primaryTime + delta, PrimaryDuration);
        secondaryTime = Math.Min(secondaryTime + delta, SecondaryDuration);

        UpdateItem(delta);
        UpdateMovement(delta);

        float lookDecay = (float)Math.Pow(0.5, delta / LookHalfLife);
        look *= lookDecay;
        crouch = Approach(crouch, ent.IsCrouching ? 1 : 0, delta, CrouchHalfLife);

        Pose = ComputePose();
    }

    private void UpdateItem(double delta)
    {
        var item = ent.HotBarSlots[ent.HotBarIndex].Item;
        if (!initialized)
        {
            displayedItem = item;
            pendingItem = item;
            initialized = true;
        }
        else if (item != pendingItem)
        {
            pendingItem = item;
            equipLowering = true;
        }

        double step = delta / EquipHalfDuration;
        if (equipLowering)
        {
            equip = Math.Min(equip + step, 1);
            if (equip >= 1)
            {
                displayedItem = pendingItem;
                equipLowering = false;
            }
        }
        else equip = Math.Max(equip - step, 0);
    }

    private void UpdateMovement(double delta)
    {
        bool grounded = ent.CollisionNormal.Z == 1 && !ent.IsFlying;
        double target = grounded ? Math.Clamp(ent.Velocity.Xy.Length / 0.1, 0, 1) : 0;
        bobStrength = Approach(bobStrength, target, delta, BobHalfLife);

        double stride = (ent.Position - ent.PrevPosition).Xy.Length;
        bobPhase += stride / tick.Interval * delta * StridePhase;
    }

    private PlayerViewModelPose ComputePose()
    {
        float equipAmount = Smooth((float)equip);
        float crouchAmount = (float)crouch;

        var offset = new Vec3(0, 0, MathF.Sin((float)elapsed * 1.7f) * 0.006f);
        var rotation = new Vec3(0, MathF.Sin((float)elapsed * 1.2f) * 0.008f, 0);

        offset.X -= look.X * 0.06f;
        offset.Z += look.Y * 0.05f;
        rotation.X -= look.Y * 0.55f;
        rotation.Z -= look.X * 0.85f;

        offset.Z -= equipAmount * EquipDropDistance;
        offset.Z -= crouchAmount * 0.025f;

        if (displayedItem != default && displayedItem.IsBuildable)
        {
            ApplyBlockSwing(ref offset, ref rotation, primaryTime, PrimaryDuration);
            ApplyBlockSwing(ref offset, ref rotation, secondaryTime, SecondaryDuration);
        }
        else
        {
            ApplyArmSwing(ref offset, ref rotation, primaryTime, PrimaryDuration);
            ApplyArmSwing(ref offset, ref rotation, secondaryTime, SecondaryDuration);
        }
        return new(offset, rotation);
    }

    private void ApplyArmSwing(ref Vec3 offset, ref Vec3 rotation, double time, double duration)
    {
        if (time >= duration)
            return;

        float t = (float)(time / duration);
        float arc = MathF.Sin(MathF.Sqrt(t) * float.Pi);
        float lift = MathF.Sin(MathF.Sqrt(t) * float.Tau);
        float push = MathF.Sin(t * float.Pi);
        float twist = MathF.Sin(t * t * float.Pi);
        float yaw = arc * Radians(70);

        offset.X -= arc * 0.3f;
        offset.Y -= push * 0.4f;
        offset.Z += lift * 0.4f;
        rotation.Y += twist * Radians(20);
        rotation.Z -= yaw;

        float sinYaw = MathF.Sin(yaw);
        float versYaw = 1 - MathF.Cos(yaw);
        offset.X += SwingPivotDelta.X * versYaw - SwingPivotDelta.Y * sinYaw;
        offset.Y += SwingPivotDelta.Y * versYaw + SwingPivotDelta.X * sinYaw;
    }

    private void ApplyBlockSwing(ref Vec3 offset, ref Vec3 rotation, double time, double duration)
    {
        if (time >= duration)
            return;

        float t = (float)(time / duration);
        float arc = MathF.Sin(MathF.Sqrt(t) * float.Pi);
        float lift = MathF.Sin(MathF.Sqrt(t) * float.Tau);
        float push = MathF.Sin(t * float.Pi);
        float twist = MathF.Sin(t * t * float.Pi);

        offset.X -= arc * 0.4f;
        offset.Y -= push * 0.2f;
        offset.Z += lift * 0.2f;
        rotation.X += arc * Radians(80);
        rotation.Y += arc * Radians(20);
        rotation.Z += twist * Radians(20);
    }

    private double Approach(double value, double target, double delta, double halfLife) =>
        target + (value - target) * Math.Pow(0.5, delta / halfLife);

    private float Smooth(float value) => value * value * (3 - 2 * value);

    private static float Radians(float degrees) => degrees * (float.Pi / 180);
}
