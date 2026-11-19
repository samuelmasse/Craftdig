namespace Craftdig;

[Player]
public class PlayerMultiplayerNameplates(
    RootCanvas canvas,
    RootSprites sprites,
    AppMonocraft monocraft,
    WorldTick tick,
    DimensionRemoteInterpolation interpolation,
    PlayerIdentityCache identities,
    PlayerIdentityPresentation presentation,
    PlayerEntReplicas replicas,
    PlayerEnt player,
    PlayerCamera camera,
    PlayerPerspective perspective,
    PlayerCrouch crouch)
{
    private const float LineHeightInBlocks = 0.24f;
    private const float MaxDistanceInBlocks = 64;
    // Monocraft's 1080-unit em contains nine 120-unit design pixels.
    private readonly FontSize font = monocraft[9];
    private readonly List<(Guid PlayerId, string Text, float Width, Vec4 Color)> labels = [];
    private IReadOnlyDictionary<Guid, PlayerIdentitySnapshot>? presentedPlayers;

    public void Draw()
    {
        var players = identities.Players;

        if (!ReferenceEquals(players, presentedPlayers))
            Refresh(players);

        if (labels.Count == 0 || canvas.Size.X == 0 || canvas.Size.Y == 0)
            return;

        var origin = Vec3d.Lerp(player.PrevPosition, player.Position, (float)tick.Alpha);
        origin.Z += crouch.CameraOffset;
        var viewProjection = perspective.Projection * perspective.View;
        var halfCanvas = (Vec2)canvas.Size * 0.5f;
        float projectedHeight = (perspective.Projection * new Vec4(0, LineHeightInBlocks, 0, 0)).Y * halfCanvas.Y;

        foreach (var (id, text, width, color) in labels)
        {
            if (!replicas.Contains(id))
                continue;

            var remote = replicas.Ent(id);

            if (!remote.IsLoaded || !remote.IsPlayer || remote.IsCrouching || !remote.Has<Vec3d, DimensionComponents.Position>())
                continue;

            var anchor = (Vec3)(interpolation.Position(remote) - origin).Swizzle();
            var fromCamera = anchor - camera.Offset;

            if (Vec3.Dot(fromCamera, fromCamera) > MaxDistanceInBlocks * MaxDistanceInBlocks)
                continue;

            anchor.Y += (float)remote.HitBox.Max.Z + 0.12f;
            var clip = viewProjection * new Vec4(anchor, 1);

            if (clip.W <= 0 || Math.Abs(clip.Z) > clip.W)
                continue;

            float height = projectedHeight / clip.W;

            if (height < font.Metrics.Height)
                continue;

            var point = halfCanvas * (new Vec2(clip.X, -clip.Y) / clip.W + Vec2.One);
            DrawLabel(point, height, text, width, color);
        }
    }

    private void Refresh(IReadOnlyDictionary<Guid, PlayerIdentitySnapshot> players)
    {
        presentedPlayers = players;
        presentation.Observe(players);
        labels.Clear();

        foreach (var snapshot in players.Values)
        {
            if (!snapshot.EntPresent || snapshot.PlayerId == player.Id)
                continue;

            var (text, color) = presentation.NameLabel(presentation.DisplayName(snapshot), snapshot.Status);
            labels.Add((snapshot.PlayerId, text, sprites.Batch.Measure(font, text), color));
        }
    }

    private void DrawLabel(Vec2 anchor, float height, string text, float width, Vec4 color)
    {
        float pixelsPerTexel = height / font.Metrics.Height;
        float padding = height * 0.1f;
        var size = new Vec2(width * pixelsPerTexel, height) + new Vec2(padding * 2);
        var topLeft = anchor - size * (0.5f, 1);

        if (topLeft.X >= canvas.Size.X || topLeft.Y >= canvas.Size.Y || topLeft.X + size.X <= 0 || topLeft.Y + size.Y <= 0)
            return;

        sprites.Batch.Draw(topLeft, size, (0, 0, 0, 0.6f));
        var baseline = topLeft + new Vec2(padding, padding + height * 0.5f);
        sprites.Batch.Write(font, text, baseline / pixelsPerTexel, color, 1 / pixelsPerTexel);
    }
}
