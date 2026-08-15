namespace Craftdig;

public enum IdentityTicketFailure
{
    None,
    Invalid,
    ContextNotAllowed,
    Lifetime,
    SigningKeyUnavailable,
    Signature,
}
