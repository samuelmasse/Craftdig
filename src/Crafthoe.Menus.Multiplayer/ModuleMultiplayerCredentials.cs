namespace Crafthoe.Menus.Multiplayer;

using System.IdentityModel.Tokens.Jwt;
using Google.Apis.Auth.OAuth2;

[Module]
public class ModuleMultiplayerCredentials
{
    private Thread? thread;
    private CancellationTokenSource? cts;
    private UserCredential? creds;
    private string? email;

    public bool NeedLogin => GoogleAuth.NeedLogin();
    public string? Email => email;

    public void StartLogin()
    {
        if (thread?.IsAlive == true)
            StopLogin();

        cts = new();
        thread = new(() =>
        {
            try
            {
                creds = GoogleAuth.Login(cts.Token);
                var token = new JwtSecurityTokenHandler().ReadJwtToken(creds.Token.IdToken);
                email = token.Claims.First((x) => x.Type == "email").Value;
            }
            catch { }
        });
        thread.Start();
    }

    public void WaitLogin()
    {
        thread?.Join();
    }

    public void StopLogin()
    {
        cts?.Cancel();
        cts?.Dispose();
        thread?.Join();
        cts = null;
        thread = null;
    }

    public string GetFreshToken()
    {
        if (creds == null)
            throw new Exception();

        GoogleAuth.RefreshToken(creds);
        return creds.Token.IdToken;
    }

    public void Logout()
    {
        creds = null;
        email = null;
        GoogleAuth.Logout();
    }
}
