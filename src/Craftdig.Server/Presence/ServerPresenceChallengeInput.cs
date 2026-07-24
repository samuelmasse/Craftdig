namespace Craftdig.Server;

public readonly record struct ServerPresenceChallengeInput(
    ServerPresenceConnection Connection,
    PresenceChallenge Challenge,
    long ReceivedTimestamp);
