namespace Craftdig;

internal static class IdentityTicketTestData
{
    public static ValidatedIdentityTicket Ticket(
        PlayerIdentitySession session,
        ServerContext context,
        int serial,
        DateTimeOffset issuedAt,
        Guid? playerId = null) =>
        new(
            Encoding.ASCII.GetBytes($"a.{serial}.c"),
            playerId ?? Guid.NewGuid(),
            $"player-{serial}",
            session.SessionId,
            context,
            session.PublicKey,
            "test-key",
            Guid.NewGuid(),
            issuedAt,
            issuedAt,
            issuedAt.AddMinutes(15));
}
