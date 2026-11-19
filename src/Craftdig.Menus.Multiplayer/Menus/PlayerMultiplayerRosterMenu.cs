namespace Craftdig;

[Player]
public class PlayerMultiplayerRosterMenu(
    AppStyle s,
    RootSprites sprites,
    RootUi ui,
    PlayerIdentityCache identityCache,
    PlayerPingReceiver pings,
    PlayerIdentityPresentation presentation)
{
    public void Create(EntMut root)
    {
        IReadOnlyDictionary<Guid, PlayerIdentitySnapshot>? presentedPlayers = null;
        List<EntMut> rosterRows = [];

        Node(root, out var layer)
            .SizeRelativeV((1, 1));
        {
            Node(layer, out var roster)
                .SizeRelativeV((0, 0))
                .SizeF(() =>
                {
                    float height = HeaderHeight() + rosterRows.Count * RowHeight() + s.ItemSpacingS;
                    var available = Vec2.Max(layer.SizeR - new Vec2(s.ItemSpacing * 2), Vec2.Zero);
                    return Vec2.Min(available, (s.ItemWidthL * 2, height));
                })
                .AlignmentV(Alignment.Top | Alignment.Horizontal)
                .OffsetV((0, s.ItemSpacing))
                .ColorV((0.04f, 0.04f, 0.06f, 0.94f))
                .IsDisabledF(() => !identityCache.IsPlayerListOpen);
            {
                EntMut title;
                Node(roster, out var header)
                    .SizeRelativeV((1, 0))
                    .SizeF(() => (-s.ItemSpacingXL * 2, HeaderHeight()))
                    .AlignmentV(Alignment.Horizontal);
                {
                    Node(header, out title)
                        .Mutate(NameColumn);

                    Node(header, out var pingTitle)
                        .Mutate(PingColumn);
                    SetText(pingTitle, "Server-reported ping", 24);
                }

                Node(roster, out var body)
                    .SizeRelativeV((1, 1))
                    .SizeF(() => (0, -HeaderHeight() - s.ItemSpacingS))
                    .OffsetF(() => (0, HeaderHeight()));
                s.Selector(body, out var list);

                roster.Mutate()
                    .RefreshIntervalV(TimeSpan.FromMilliseconds(200))
                    .OnRefreshF(() =>
                    {
                        var players = identityCache.Players;

                        if (ReferenceEquals(players, presentedPlayers))
                            return;

                        presentedPlayers = players;
                        presentation.Observe(players);

                        foreach (var row in rosterRows)
                            NodesRemove(list, row);

                        rosterRows.Clear();
                        var ordered = new (PlayerIdentitySnapshot Snapshot, string Name)[players.Count];
                        int index = 0;

                        foreach (var snapshot in players.Values)
                            ordered[index++] = (snapshot, presentation.DisplayName(snapshot));

                        Array.Sort(ordered, (left, right) =>
                        {
                            int byName = StringComparer.OrdinalIgnoreCase.Compare(left.Name, right.Name);
                            return byName != 0 ? byName : left.Snapshot.PlayerId.CompareTo(right.Snapshot.PlayerId);
                        });

                        SetText(title, presentation.RosterTitle(players.Count), s.FontSize);

                        foreach (var (snapshot, name) in ordered)
                        {
                            var (text, color) = presentation.NameLabel(name, snapshot.Status);
                            Node(list, out var row)
                                .SizeRelativeV((0, 0))
                                .SizeF(() => (Math.Max(0, body.SizeR.X - s.ItemSpacingXL * 2), RowHeight()));
                            {
                                Node(row, out var identity)
                                    .Mutate(NameColumn)
                                    .TextColorV(color);
                                SetText(identity, text, s.FontSize);

                                Node(row, out var ping)
                                    .Mutate(PingColumn);
                                SetText(ping, pings.Milliseconds(snapshot.PlayerId) switch
                                {
                                    null => "—",
                                    0 => "<1 ms",
                                    var ms => $"{ms} ms",
                                }, 24);
                            }
                            rosterRows.Add(row);
                        }
                    });
            }
        }

        bool StackColumns() => layer.SizeR.X < s.ItemWidth * 2;
        float RowHeight() => s.FontSize + s.ItemSpacing + (StackColumns() ? s.ItemSpacingXL : 0);
        float HeaderHeight() => RowHeight() + s.ItemSpacingS;

        void NameColumn(EntMut label) => label.Mutate()
            .Mutate(s.Text)
            .SizeRelativeF(() => StackColumns() ? (1, 0.5f) : (0.65f, 1))
            .TextAlignmentV(Alignment.Left | Alignment.Vertical);

        void PingColumn(EntMut label) => label.Mutate()
            .Mutate(s.Text)
            .SizeRelativeF(() => StackColumns() ? (1, 0.5f) : (0.35f, 1))
            .AlignmentV(Alignment.Right | Alignment.Bottom)
            .TextAlignmentV(Alignment.Right | Alignment.Vertical);

        void SetText(EntMut label, string value, int maxFontSize)
        {
            float previousWidth = -1;
            float previousScale = 0;
            int fontSize = maxFontSize;

            label.Mutate()
                .TextV(value)
                .TextGlyphAlignmentSnapV(1)
                .FontSizeF(() =>
                {
                    float width = label.SizeR.X - s.ItemSpacing;
                    float scale = ui.Scale.Scale;

                    if (width == previousWidth && scale == previousScale)
                        return fontSize;

                    previousWidth = width;
                    previousScale = scale;
                    fontSize = maxFontSize;

                    while (fontSize > 4)
                    {
                        var font = s.Font.Size(Math.Max(1, (int)(fontSize * scale)));

                        if (sprites.Batch.Measure(font, value, scale) <= width * scale)
                            break;

                        fontSize -= 4;
                    }

                    return fontSize;
                });
        }
    }
}
