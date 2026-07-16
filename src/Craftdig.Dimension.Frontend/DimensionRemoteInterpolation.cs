namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionRemoteInterpolation(WorldTick tick)
{
    private readonly double interval = Stopwatch.Frequency * tick.Interval;
    private long timestamp;

    public void Frame() => timestamp = Stopwatch.GetTimestamp();

    public Vec3d Position(Ent ent)
    {
        var interpolation = ent.RemotePosition;
        return Vec3d.Lerp(interpolation.From, ent.Position, Alpha(interpolation.StartedAt));
    }

    public Vec3 LookAt(Ent ent)
    {
        var interpolation = ent.RemoteLookAt;
        return Vec3.Lerp(interpolation.From, ent.LookAt, Alpha(interpolation.StartedAt))
            .NormalizedOr(ent.LookAt);
    }

    public void StartPosition(EntMutIdx ent, Vec3d from) =>
        ent.RemotePosition = new(from, timestamp);

    public void StartLookAt(EntMutIdx ent, Vec3 from) =>
        ent.RemoteLookAt = new(from, timestamp);

    public void SnapPosition(EntMutIdx ent) =>
        ent.RemotePosition = new(ent.Position, timestamp);

    public void SnapLookAt(EntMutIdx ent) =>
        ent.RemoteLookAt = new(ent.LookAt, timestamp);

    private float Alpha(long startedAt) =>
        (float)Math.Min((timestamp - startedAt) / interval, 1);
}
