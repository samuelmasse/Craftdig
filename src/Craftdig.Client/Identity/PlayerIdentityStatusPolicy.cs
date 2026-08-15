namespace Craftdig;

internal static class PlayerIdentityStatusPolicy
{
    public static PlayerIdentityStatus Evaluate(in PlayerIdentityEvidence evidence)
    {
        if (!evidence.HasVerifiableTransport)
            return PlayerIdentityStatus.Unverified;
        if (evidence.Invalid)
            return PlayerIdentityStatus.Invalid;
        if (evidence.VerifiedDeadline is { } deadline &&
            evidence.MonotonicNow <= deadline &&
            evidence.VerifiedTicketExpiresAt > evidence.UtcNow &&
            evidence.VerifiedRoundUsable)
            return PlayerIdentityStatus.Verified;
        if (evidence.VerifiedDeadline != null || evidence.ProofSeen || evidence.CurrentTicketExpiresAt <= evidence.UtcNow)
            return PlayerIdentityStatus.Stale;
        return evidence.CurrentTicketExpiresAt != null || (evidence.IdentityExpected && evidence.EntPresent)
            ? PlayerIdentityStatus.Pending
            : PlayerIdentityStatus.Unverified;
    }
}
