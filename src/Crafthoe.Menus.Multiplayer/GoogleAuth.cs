namespace Crafthoe.Menus.Multiplayer;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;

public static class GoogleAuth
{
    private static ClientSecrets Secrets => new()
    {
        ClientId = "253499777147-87jfh5dlbotv4nenq0bd4vtlrd20jo0j.apps.googleusercontent.com",
        ClientSecret = "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"
    };

    private static GoogleAuthorizationCodeFlow.Initializer Initializer => new()
    {
        ClientSecrets = Secrets,
        Scopes = ["openid", "email"],
        DataStore = new FileDataStore("Crafthoe.GoogleAuth")
    };

    private static string UserId => "crafhoe_user";

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
