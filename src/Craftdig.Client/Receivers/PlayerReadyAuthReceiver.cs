namespace Craftdig.Client;

[Player]
public class PlayerReadyAuthReceiver(AppLog log)
{
    public void Receive(ReadyAuthCommand cmd, ReadOnlySpan<byte> data)
    {
        var nonce = Encoding.UTF8.GetString(data);
        log.Warn("Nonce {0}", nonce);

        // TODO: handle auth
        /*
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", credentials.GetFreshToken());

        log.Warn(credentials.GetFreshToken());

        var post = http.PostAsJsonAsync($"https://dev.craftdig.io/api/GetToken", new { host, nonce });
        post.Wait();
        var res = post.Result;
        var bodyTask = res.Content.ReadFromJsonAsync<GetTokenResponse>();
        bodyTask.Wait();
        var body = bodyTask.Result;

        log.Warn("Http call done {0} {1}", res.StatusCode, res.Content.ToString());
        */
    }

    private record GetTokenResponse(string Jwt);
}
