namespace Craftdig;

public readonly record struct ServerPresenceProofInput(
    ServerPresenceConnection Connection,
    PresenceProof Proof);
