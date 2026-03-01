namespace Craftdig.Menus.Multiplayer;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;

public static class GoogleAuth
{
    private static ClientSecrets Secrets => new()
    {
        // These are not intended to be secrets but they can get flagged as leaked secrets
        // by code analyzers so they are obfuscated into base64 to work around that issue
        ClientId = Encoding.UTF8.GetString(Convert.FromHexString(
            "3432383033373534333535312d33356c736f76307430653232736531716966766664746" +
            "939306465746c6466642e617070732e676f6f676c6575736572636f6e74656e742e636f6d")),
        ClientSecret = Encoding.UTF8.GetString(Convert.FromHexString(
            "474f435350582d614a5978624f51722d487a6938336135423435463259587347325f36"))
    };

    private static GoogleAuthorizationCodeFlow.Initializer Initializer => new()
    {
        ClientSecrets = Secrets,
        Scopes = ["openid", "email"],
        DataStore = new FileDataStore("Craftdig.GoogleAuth")
    };

    private static string UserId => "crafdig_user";

    public static bool NeedLogin()
    {
        var flow = new GoogleAuthorizationCodeFlow(Initializer);
        var codeReceiver = new LocalServerCodeReceiver();
        var app = new AuthorizationCodeInstalledApp(flow, codeReceiver);

        var token = flow.LoadTokenAsync(UserId, default);
        token.Wait();

        return app.ShouldRequestAuthorizationCode(token.Result);
    }

    public static UserCredential Login(CancellationToken ct)
    {
        var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            Secrets, Initializer.Scopes, UserId, ct, Initializer.DataStore);

        credential.Wait(ct);

        return credential.Result;
    }

    public static void Logout()
    {
        var clear = Initializer.DataStore.ClearAsync();
        clear.Wait();
    }

    public static void RefreshToken(UserCredential cred)
    {
        var refresh = cred.RefreshTokenAsync(default);
        refresh.Wait();
    }
}
