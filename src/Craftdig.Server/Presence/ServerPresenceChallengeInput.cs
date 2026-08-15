namespace Craftdig;

public readonly record struct ServerPresenceChallengeInput(
    ServerPresenceConnection Connection,
    PresenceChallenge Challenge,
    long ReceivedTimestamp);
