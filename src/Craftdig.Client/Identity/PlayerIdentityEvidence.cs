namespace Craftdig;

internal readonly record struct PlayerIdentityEvidence
{
    public required bool HasVerifiableTransport { get; init; }
    public required bool IdentityExpected { get; init; }
    public required bool EntPresent { get; init; }
    public required bool Invalid { get; init; }
    public required bool ProofSeen { get; init; }
    public required bool VerifiedRoundUsable { get; init; }
    public required long MonotonicNow { get; init; }
    public required DateTimeOffset UtcNow { get; init; }
    public long? VerifiedDeadline { get; init; }
    public DateTimeOffset? VerifiedTicketExpiresAt { get; init; }
    public DateTimeOffset? CurrentTicketExpiresAt { get; init; }
}
