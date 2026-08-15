namespace Craftdig;

[Player]
public class PlayerIdentityPresentation(AppClientOptions clientOptions, AppStyle s)
{
    private readonly Dictionary<string, int> duplicateNames = new(StringComparer.Ordinal);
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
            return $"development player #{ShortId(snapshot.PlayerId)}";
        if (string.IsNullOrWhiteSpace(snapshot.Username))
        {
            string label = snapshot.Status switch
            {
                PlayerIdentityStatus.Invalid => "invalid identity",
                PlayerIdentityStatus.Stale => "stale identity",
                PlayerIdentityStatus.Verified => "verified identity",
                _ => "identity pending",
            };
            return $"{label} #{ShortId(snapshot.PlayerId)}";
        }

        return duplicateNames.GetValueOrDefault(snapshot.Username) > 1
            ? $"{snapshot.Username} #{ShortId(snapshot.PlayerId)}"
            : snapshot.Username;
    }

    public string StatusLabel(PlayerIdentityStatus status) => StatusStyle(status).Label;

    public Vec4 StatusColor(PlayerIdentityStatus status) => StatusStyle(status).Color;

    public string ConnectionLabel()
    {
        if (clientOptions.UseRawTcp)
            return "DEVELOPMENT CONNECTION: RAW TCP + NO AUTH - UNVERIFIED";
        if (clientOptions.NoAuthUser != null)
            return "DEVELOPMENT CONNECTION: NO AUTH - UNVERIFIED";

        return "VERIFIED marks a locally checked Identity proof";
    }

    public Vec4 ConnectionColor() =>
        clientOptions.UseRawTcp || clientOptions.NoAuthUser != null
            ? StatusColor(PlayerIdentityStatus.Unverified)
            : s.TextColorFaint;

    private static string ShortId(Guid playerId) => playerId.ToString("N")[..8];

    private static (string Label, Vec4 Color) StatusStyle(PlayerIdentityStatus status) => status switch
    {
        PlayerIdentityStatus.Unverified => ("UNVERIFIED", (0.75f, 0.55f, 1, 1)),
        PlayerIdentityStatus.Pending => ("PENDING", (1, 0.85f, 0.25f, 1)),
        PlayerIdentityStatus.Verified => ("VERIFIED", (0.3f, 1, 0.4f, 1)),
        PlayerIdentityStatus.Stale => ("STALE", (1, 0.55f, 0.15f, 1)),
        PlayerIdentityStatus.Invalid => ("INVALID", (1, 0.2f, 0.2f, 1)),
        _ => throw new UnreachableException(),
    };
}
