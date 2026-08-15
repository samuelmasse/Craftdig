namespace Craftdig;

internal readonly record struct PlayerIdentityLedgerChange(bool Stored, Hash256? EvictedTicket, IReadOnlyList<Guid> InvalidPlayers)
{
    public static PlayerIdentityLedgerChange None => new(false, null, []);

    public static PlayerIdentityLedgerChange Invalidating(Guid first, Guid second) => new(false, null, [first, second]);
}
