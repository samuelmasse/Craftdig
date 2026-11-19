namespace Craftdig;

[Player]
public class PlayerIdentityPresentation(AppClientOptions clientOptions)
{
    private readonly Dictionary<string, int> duplicateNames = [];
    private IReadOnlyDictionary<Guid, PlayerIdentitySnapshot>? countedPlayers;

    public void Observe(IReadOnlyDictionary<Guid, PlayerIdentitySnapshot> players)
    {
        if (ReferenceEquals(players, countedPlayers))
            return;

        countedPlayers = players;
        duplicateNames.Clear();
        foreach (var snapshot in players.Values)
        {
            if (snapshot.Status == PlayerIdentityStatus.Unverified ||
                string.IsNullOrWhiteSpace(snapshot.Username))
                continue;

            duplicateNames.TryGetValue(snapshot.Username, out int count);
            duplicateNames[snapshot.Username] = count + 1;
        }
    }

    public string DisplayName(PlayerIdentitySnapshot snapshot)
    {
        if (snapshot.Status == PlayerIdentityStatus.Unverified)
            return snapshot.DevelopmentName ?? "Unknown player";

        if (string.IsNullOrWhiteSpace(snapshot.Username))
            return $"Player #{ShortId(snapshot.PlayerId)}";

        return duplicateNames.GetValueOrDefault(snapshot.Username) > 1
            ? $"{snapshot.Username} #{ShortId(snapshot.PlayerId)}"
            : snapshot.Username;
    }

    public (string Text, Vec4 Color) NameLabel(string name, PlayerIdentityStatus status) => status switch
    {
        PlayerIdentityStatus.Unverified => ($"%{name}%", (1, 0, 1, 1)),
        PlayerIdentityStatus.Pending => ($"?{name}?", (1, 0.85f, 0.25f, 1)),
        PlayerIdentityStatus.Verified => (name, (1, 1, 1, 1)),
        PlayerIdentityStatus.Stale => ($"~{name}~", (1, 0.55f, 0.15f, 1)),
        PlayerIdentityStatus.Invalid => ($"!{name}!", (1, 0.24f, 0.24f, 1)),
        _ => throw new UnreachableException(),
    };

    public string RosterTitle(int count) => clientOptions.UseRawTcp || clientOptions.NoAuthUser != null
        ? $"Players ({count}) - Development"
        : $"Players ({count})";

    private static string ShortId(Guid playerId) => playerId.ToString("N")[..8];
}
