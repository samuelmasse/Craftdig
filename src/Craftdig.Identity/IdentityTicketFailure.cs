namespace Craftdig.Identity;

public enum IdentityTicketFailure
{
    None,
    Invalid,
    ContextNotAllowed,
    Lifetime,
    SigningKeyUnavailable,
    Signature,
}
