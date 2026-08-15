namespace Craftdig;

[Player]
public class PlayerMultiplayerNameplatesMenu(
    RootCanvas canvas,
    RootUiScale scale,
    AppStyle s,
    WorldTick tick,
    DimensionRemoteInterpolation remoteInterpolation,
    PlayerIdentityCache identityCache,
    PlayerIdentityPresentation presentation,
    PlayerEntReplicas replicas,
    PlayerEnt player,
    PlayerPerspective perspective,
    PlayerCrouch crouch)
{
    public void Create(EntMut root)
    {
        const float nameplateHeight = 0.45f;

        IReadOnlyDictionary<Guid, PlayerIdentitySnapshot>? presentedPlayers = null;
        Dictionary<Guid, EntMut> nameplateNodes = [];

        Node(root, out var nameplates)
            .SizeRelativeV((1, 1));

        Node(root).OnUpdateF(() =>
        {
            var players = identityCache.Players;
            if (ReferenceEquals(players, presentedPlayers))
                return;

            presentedPlayers = players;
            presentation.Observe(players);
            Refresh(players);
        });

        void Refresh(IReadOnlyDictionary<Guid, PlayerIdentitySnapshot> players)
        {
            foreach (Guid playerId in nameplateNodes.Keys.ToArray())
            {
                if (players.TryGetValue(playerId, out var snapshot) && snapshot.EntPresent)
                    continue;

                NodesRemove(nameplates, nameplateNodes[playerId]);
                nameplateNodes.Remove(playerId);
            }

            foreach (var snapshot in players.Values)
            {
                if (!snapshot.EntPresent)
                    continue;

                if (!nameplateNodes.TryGetValue(snapshot.PlayerId, out var label))
                {
                    Guid playerId = snapshot.PlayerId;
                    Node(nameplates, out label)
                        .Mutate(s.Label)
                        .PaddingV((s.ItemSpacingXS, s.ItemSpacingXS, s.ItemSpacingXS, s.ItemSpacingXS))
                        .ColorV((0, 0, 0, 0.6f))
                        .AlignmentV(Alignment.Top | Alignment.Left)
                        .IsDisabledF(() => !TryNameplatePosition(playerId, out _))
                        .OffsetF(() =>
                        {
                            if (!TryNameplatePosition(playerId, out var point))
                                return default;

                            return point - label.SizeR * (0.5f, 1);
                        });
                    nameplateNodes.Add(playerId, label);
                }

                label.Mutate()
                    .TextColorV(presentation.StatusColor(snapshot.Status))
                    .TextV($"[{presentation.StatusLabel(snapshot.Status)}] {presentation.DisplayName(snapshot)}");
            }
        }

        bool TryNameplatePosition(Guid playerId, out Vec2 point)
        {
            point = default;
            if (!replicas.Contains(playerId) || replicas.IsOwner(playerId))
                return false;

            var remote = replicas.Ent(playerId);
            if (remote.Id != playerId ||
                !identityCache.TryGet(remote.Id, out var snapshot) ||
                !snapshot.EntPresent ||
                !remote.IsLoaded || !remote.IsPlayer ||
                !remote.Has<Vec3d, DimensionComponents.Position>())
                return false;

            var origin = Vec3d.Lerp(player.PrevPosition, player.Position, (float)tick.Alpha);
            origin.Z += crouch.CameraOffset;
            var renderPosition = (Vec3)(remoteInterpolation.Position(remote) - origin).Swizzle();
            renderPosition.Y += nameplateHeight;

            var clip = perspective.Projection * perspective.View * new Vec4(renderPosition, 1);
            if (clip.W <= 0)
                return false;

            var normalized = new Vec3(clip.X, clip.Y, clip.Z) / clip.W;
            if (normalized.X is < -1 or > 1 ||
                normalized.Y is < -1 or > 1 ||
                normalized.Z is < -1 or > 1)
                return false;

            var canvasSize = (Vec2)canvas.Size;
            point = (
                (normalized.X + 1) * 0.5f * canvasSize.X / scale.Scale,
                (1 - (normalized.Y + 1) * 0.5f) * canvasSize.Y / scale.Scale);
            return true;
        }
    }
}
