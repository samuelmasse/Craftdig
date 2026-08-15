namespace Craftdig;

[Player]
public class PlayerMultiplayerRosterMenu(
    AppStyle s,
    PlayerIdentityCache identityCache,
    PlayerIdentityPresentation presentation)
{
    public void Create(EntMut root)
    {
        const float rosterHeaderHeight = 112;

        IReadOnlyDictionary<Guid, PlayerIdentitySnapshot>? presentedPlayers = null;
        List<EntMut> rosterRows = [];
        EntMut rosterTitle;
        EntMut rosterList;
        bool rosterDirty = true;

        Node(root, out var layer)
            .SizeRelativeV((1, 1));

        Node(layer, out var roster)
            .SizeRelativeV((0.8f, 1))
            .SizeV((0, -s.ItemSpacingXL * 2))
            .AlignmentV(Alignment.Top | Alignment.Horizontal)
            .OffsetV((0, s.ItemSpacingXL))
            .ColorV((0.04f, 0.04f, 0.06f, 0.94f))
            .IsDisabledF(() => !identityCache.IsPlayerListOpen);
        {
            Node(roster, out var header)
                .Mutate(s.VerticalList)
                .SizeRelativeV((1, 0))
                .SizeV((0, rosterHeaderHeight))
                .PaddingV((s.ItemSpacing, s.ItemSpacingS, s.ItemSpacing, s.ItemSpacingS));
            {
                Node(header, out rosterTitle)
                    .Mutate(s.Label)
                    .AlignmentV(Alignment.Horizontal)
                    .TextV("MULTIPLAYER PLAYERS (0)");

                Node(header)
                    .Mutate(s.Label)
                    .AlignmentV(Alignment.Horizontal)
                    .TextColorV(presentation.ConnectionColor())
                    .TextV(presentation.ConnectionLabel());
            }

            Node(roster, out var rosterBody)
                .SizeRelativeV((1, 1))
                .SizeV((0, -rosterHeaderHeight))
                .OffsetV((0, rosterHeaderHeight));
            {
                s.Selector(rosterBody, out rosterList);
            }

            Node(layer).OnUpdateF(() =>
            {
                var players = identityCache.Players;
                if (!ReferenceEquals(players, presentedPlayers))
                {
                    presentedPlayers = players;
                    presentation.Observe(players);
                    rosterDirty = true;
                }

                if (identityCache.IsPlayerListOpen && rosterDirty)
                    RebuildRoster(presentedPlayers ?? identityCache.Players);
            });

            void RebuildRoster(IReadOnlyDictionary<Guid, PlayerIdentitySnapshot> players)
            {
                foreach (var row in rosterRows)
                    NodesRemove(rosterList, row);
                rosterRows.Clear();

                var ordered = new (PlayerIdentitySnapshot Snapshot, string DisplayName)[players.Count];
                int index = 0;
                foreach (var snapshot in players.Values)
                    ordered[index++] = (snapshot, presentation.DisplayName(snapshot));

                Array.Sort(ordered, (left, right) =>
                {
                    int nameOrder = StringComparer.OrdinalIgnoreCase.Compare(left.DisplayName, right.DisplayName);
                    return nameOrder != 0
                        ? nameOrder
                        : left.Snapshot.PlayerId.CompareTo(right.Snapshot.PlayerId);
                });

                rosterTitle.Mutate().TextV($"MULTIPLAYER PLAYERS ({ordered.Length})");
                foreach (var (Snapshot, DisplayName) in ordered)
                {
                    var snapshot = Snapshot;
                    string location = snapshot.EntPresent ? "nearby" : "off-chunk";
                    Node(rosterList, out var row)
                        .Mutate(s.Label)
                        .SizeTextRelativeV((0, 1))
                        .SizeF(() => (rosterBody.SizeR.X - s.ItemSpacingXL * 2, 0))
                        .TextColorV(presentation.StatusColor(snapshot.Status))
                        .TextV($"{DisplayName}  [{presentation.StatusLabel(snapshot.Status)}]  {location}");
                    rosterRows.Add(row);
                }

                rosterDirty = false;
            }
        }
    }
}
