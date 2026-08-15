namespace Craftdig;

public readonly record struct PresenceProof(Hash256 RoundHash, Hash256 TicketHash, P256Signature Signature);
