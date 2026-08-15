namespace Craftdig;

public readonly record struct ServerPresenceInboxDepths(
    int Lifecycle,
    int Challenges,
    int Proofs);
