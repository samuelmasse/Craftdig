namespace Craftdig.Server;

public readonly record struct ServerPresenceInboxDepths(
    int Lifecycle,
    int Challenges,
    int Proofs);
