namespace Craftdig.Identity;

public class IdentityJwksCache(AppLog log, string endpointUrl)
{
    private const int MaxJwksBytes = 64 * 1024;
    private const int RequiredKeyCount = 1;
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan DefaultFreshness = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MaximumFreshness = TimeSpan.FromHours(1);
    private static readonly TimeSpan MaximumStalePeriod = TimeSpan.FromHours(1);
    private static readonly TimeSpan MinimumRefreshInterval = TimeSpan.FromSeconds(30);

    private readonly AppLog log = log;
    private readonly HttpClient http = new();
    private readonly SemaphoreSlim refreshGate = new(1, 1);
    private readonly Uri endpoint = ParseEndpoint(endpointUrl);
    private IReadOnlyDictionary<string, SecurityKey> keys = new Dictionary<string, SecurityKey>(StringComparer.Ordinal);
    private DateTimeOffset freshUntil = DateTimeOffset.MinValue;
    private DateTimeOffset staleUntil = DateTimeOffset.MinValue;
    private DateTimeOffset nextRefreshAt = DateTimeOffset.MinValue;
    private DateTimeOffset nextEarlyRefreshAt = DateTimeOffset.MinValue;
    private Task? earlyRefreshTask;

    internal IdentityJwksCache(
        AppLog log,
        string endpointUrl,
        IReadOnlyDictionary<string, SecurityKey> initialKeys) : this(log, endpointUrl)
    {
        keys = initialKeys;
        freshUntil = DateTimeOffset.MaxValue;
        staleUntil = DateTimeOffset.MaxValue;
    }

    // Dev-only: a cache permanently seeded with a locally trusted key and no network endpoint.
    public static IdentityJwksCache Seeded(AppLog log, SecurityKey key) =>
        new(log, "https://craftdig.io/.well-known/jwks.json",
            new Dictionary<string, SecurityKey>(StringComparer.Ordinal) { [key.KeyId] = key });

    public SecurityKey? GetKey(string keyId, CancellationToken cancellationToken = default) =>
        GetKeyAsync(keyId, cancellationToken).GetAwaiter().GetResult();

    private async Task<SecurityKey?> GetKeyAsync(string keyId, CancellationToken cancellationToken)
    {
        var (key, earlyRefresh, needsRefresh) = Decide(keyId);
        if (earlyRefresh != null)
            return await ResolveAfterEarlyRefreshAsync(keyId, earlyRefresh, cancellationToken).ConfigureAwait(false);
        if (!needsRefresh)
            return key;

        return await RefreshAndResolveAsync(keyId, cancellationToken).ConfigureAwait(false);
    }

    private (SecurityKey? Key, Task? EarlyRefresh, bool NeedsRefresh) Decide(string keyId)
    {
        var now = DateTimeOffset.UtcNow;
        lock (this)
        {
            keys.TryGetValue(keyId, out var cached);
            if (now < freshUntil)
            {
                if (cached != null)
                    return (cached, null, false);
                if (earlyRefreshTask != null)
                    return (null, earlyRefreshTask, false);
                if (now >= nextEarlyRefreshAt)
                {
                    nextEarlyRefreshAt = now + MinimumRefreshInterval;
                    earlyRefreshTask = RefreshEarlyAsync(freshUntil);
                    return (null, earlyRefreshTask, false);
                }

                return (null, null, false);
            }

            if (now < nextRefreshAt)
                return (cached != null && now < staleUntil ? cached : null, null, false);

            return (null, null, true);
        }
    }

    private async Task<SecurityKey?> ResolveAfterEarlyRefreshAsync(
        string keyId,
        Task earlyRefresh,
        CancellationToken cancellationToken)
    {
        await earlyRefresh.WaitAsync(cancellationToken).ConfigureAwait(false);
        lock (this)
        {
            return DateTimeOffset.UtcNow < staleUntil && keys.TryGetValue(keyId, out var refreshed)
                ? refreshed
                : null;
        }
    }

    private async Task<SecurityKey?> RefreshAndResolveAsync(string keyId, CancellationToken cancellationToken)
    {
        await refreshGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var now = DateTimeOffset.UtcNow;
            lock (this)
            {
                keys.TryGetValue(keyId, out var cached);
                if (now < freshUntil)
                    return cached;
                if (now < nextRefreshAt)
                    return cached != null && now < staleUntil ? cached : null;

                nextRefreshAt = now + MinimumRefreshInterval;
            }

            return await FetchAndInstallAsync(keyId, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            refreshGate.Release();
        }
    }

    private async Task<SecurityKey?> FetchAndInstallAsync(string keyId, CancellationToken cancellationToken)
    {
        try
        {
            var fetched = await FetchAsync(cancellationToken).ConfigureAwait(false);
            lock (this)
            {
                Install(fetched);
                return keys.TryGetValue(keyId, out var refreshed) ? refreshed : null;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e) when (IsRefreshFailure(e))
        {
            log.Warn("Identity JWKS refresh failed", e);
            lock (this)
            {
                if (keys.TryGetValue(keyId, out var stale) && DateTimeOffset.UtcNow < staleUntil)
                {
                    log.Warn("Identity signing key {0} is using the stale JWKS cache after refresh failure", keyId);
                    return stale;
                }

                log.Warn("Identity signing key {0} is unavailable after JWKS refresh failure", keyId);
                return null;
            }
        }
    }

    private async Task RefreshEarlyAsync(DateTimeOffset observedFreshUntil)
    {
        // Yield so the finally below can never clear earlyRefreshTask before Decide assigns it.
        await Task.Yield();
        bool acquired = false;
        try
        {
            await refreshGate.WaitAsync().ConfigureAwait(false);
            acquired = true;
            lock (this)
            {
                if (freshUntil != observedFreshUntil)
                    return;
            }

            try
            {
                var fetched = await FetchAsync(default).ConfigureAwait(false);
                lock (this)
                    Install(fetched);
            }
            catch (Exception e) when (IsRefreshFailure(e))
            {
                log.Warn("Identity JWKS early key refresh failed", e);
            }
        }
        finally
        {
            lock (this)
                earlyRefreshTask = null;
            if (acquired)
                refreshGate.Release();
        }
    }

    private void Install(JwksFetch fetched)
    {
        keys = fetched.Keys;
        freshUntil = fetched.FreshUntil;
        staleUntil = fetched.StaleUntil;
        nextEarlyRefreshAt = DateTimeOffset.UtcNow + MinimumRefreshInterval;
        log.Info(
            "Installed {0} Identity signing key(s); fresh for {1:N0}s and stale fallback for {2:N0}s",
            keys.Count,
            Math.Max(0, (freshUntil - DateTimeOffset.UtcNow).TotalSeconds),
            Math.Max(0, (staleUntil - freshUntil).TotalSeconds));
    }

    private async Task<JwksFetch> FetchAsync(CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(RequestTimeout);
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        if (response.Content.Headers.ContentLength > MaxJwksBytes)
            throw new InvalidDataException("Identity JWKS response exceeded its size limit.");

        await response.Content.LoadIntoBufferAsync(MaxJwksBytes, timeout.Token).ConfigureAwait(false);
        string json = await response.Content.ReadAsStringAsync(timeout.Token).ConfigureAwait(false);
        var fetchedKeys = ParseKeys(json);

        var fetchedAt = DateTimeOffset.UtcNow;
        var freshness = response.Headers.CacheControl?.MaxAge ??
            (response.Content.Headers.Expires is { } expires ? expires - fetchedAt : DefaultFreshness);
        if (freshness < TimeSpan.Zero)
            freshness = TimeSpan.Zero;
        if (freshness > MaximumFreshness)
            freshness = MaximumFreshness;

        var fetchedFreshUntil = fetchedAt + freshness;
        return new(fetchedKeys, fetchedFreshUntil, fetchedFreshUntil + MaximumStalePeriod);
    }

    internal static IReadOnlyDictionary<string, SecurityKey> ParseKeys(string json)
    {
        var parsed = new JsonWebKeySet(json);
        if (parsed.Keys.Count != RequiredKeyCount)
            throw new InvalidDataException("Identity JWKS must contain exactly one signing key.");

        var fetchedKeys = new Dictionary<string, SecurityKey>(StringComparer.Ordinal);
        foreach (var key in parsed.Keys)
        {
            if (key.Kty != "RSA" || string.IsNullOrEmpty(key.Kid) || !IdentityTicketFormat.IsKeyId(key.Kid) ||
                (key.Use != null && key.Use != "sig") ||
                (key.Alg != null && key.Alg != SecurityAlgorithms.RsaSha256) ||
                string.IsNullOrEmpty(key.N) || string.IsNullOrEmpty(key.E) ||
                !string.IsNullOrEmpty(key.D) || !string.IsNullOrEmpty(key.P) || !string.IsNullOrEmpty(key.Q) ||
                !string.IsNullOrEmpty(key.DP) || !string.IsNullOrEmpty(key.DQ) || !string.IsNullOrEmpty(key.QI))
                continue;

            if (!StrictJson.TryDecodeCanonicalBase64Url(key.N, out byte[]? modulus) ||
                !StrictJson.TryDecodeCanonicalBase64Url(key.E, out byte[]? exponent))
                continue;

            if (modulus.Length is < 256 or > 512 || exponent.Length is < 1 or > 8 ||
                exponent[0] == 0 || (exponent[^1] & 1) == 0 ||
                !fetchedKeys.TryAdd(key.Kid, key))
                throw new InvalidDataException("Identity JWKS contains an invalid or duplicate signing key.");
        }

        if (fetchedKeys.Count != RequiredKeyCount)
            throw new InvalidDataException("Identity JWKS contains no usable public RS256 signing key.");
        return fetchedKeys;
    }

    private static bool IsRefreshFailure(Exception e) => e is
        HttpRequestException or
        TaskCanceledException or
        JsonException or
        InvalidDataException or
        ArgumentException or
        SecurityTokenException;

    private static Uri ParseEndpoint(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps ||
            string.IsNullOrEmpty(uri.Host) ||
            !string.IsNullOrEmpty(uri.UserInfo) ||
            !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment))
            throw new InvalidDataException("The Identity JWKS URL must be an absolute HTTPS URL without user info, query, or fragment.");

        return uri;
    }

    private sealed record JwksFetch(
        IReadOnlyDictionary<string, SecurityKey> Keys,
        DateTimeOffset FreshUntil,
        DateTimeOffset StaleUntil);
}
