namespace Craftdig.Server;

public readonly record struct ServerPresenceProofInput(
    ServerPresenceConnection Connection,
    PresenceProof Proof);
