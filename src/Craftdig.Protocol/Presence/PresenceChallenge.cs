namespace Craftdig;

public readonly record struct PresenceChallenge(ulong Sequence, Nonce256 Nonce);
