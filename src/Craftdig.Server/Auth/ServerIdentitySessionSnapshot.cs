namespace Craftdig;

public sealed record ServerIdentitySessionSnapshot(
    long ConnectionGeneration,
    ulong TicketRevision,
    ValidatedIdentityTicket Ticket);
