namespace Craftdig.Client;

public sealed record PlayerIdentitySnapshot(
    Guid PlayerId,
    string? Username,
    PlayerIdentityStatus Status,
    bool EntPresent);
