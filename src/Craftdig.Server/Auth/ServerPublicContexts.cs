namespace Craftdig;

[Server]
public class ServerPublicContexts
{
    private readonly Log log;
    private readonly ServerContext[] contexts;

    public ServerPublicContexts(Log log, ServerConfig config)
    {
        this.log = log;
        contexts = new ServerContext[config.PublicServerContexts.Length];
        for (int i = 0; i < contexts.Length; i++)
        {
            if (!TryParse(config.PublicServerContexts[i], out var context))
                throw new InvalidDataException($"Invalid PublicServerContexts value: {config.PublicServerContexts[i]}");

            for (int j = 0; j < i; j++)
            {
                if (context == contexts[j])
                    throw new InvalidDataException($"Duplicate PublicServerContexts value: {config.PublicServerContexts[i]}");
            }

            contexts[i] = context;
        }

        if (contexts.Length == 0)
            log.Warn("No public Identity server contexts are configured; authenticated TLS joins will fail closed");
        else log.Info("Configured {0} public Identity server context(s)", contexts.Length);
    }

    public bool Allows(ServerContext context)
    {
        foreach (var allowed in contexts)
        {
            if (context == allowed)
            {
                return true;
            }
        }

        return false;
    }

    private bool TryParse(string value, [NotNullWhen(true)] out ServerContext? context)
    {
        context = null;
        if (string.IsNullOrEmpty(value) || value != value.Trim())
            return false;

        string host;
        string portText;
        if (value[0] == '[')
        {
            int separator = value.IndexOf("]:", StringComparison.Ordinal);
            if (separator < 2 || value.IndexOf('[', 1) >= 0 || value.IndexOf(']', separator + 1) >= 0)
                return false;

            host = value[1..separator];
            portText = value[(separator + 2)..];
        }
        else
        {
            int separator = value.LastIndexOf(':');
            if (separator < 1 || value[..separator].Contains(':'))
                return false;

            host = value[..separator];
            portText = value[(separator + 1)..];
        }

        if (!ushort.TryParse(portText, NumberStyles.None, CultureInfo.InvariantCulture, out ushort port) ||
            portText != port.ToString(CultureInfo.InvariantCulture))
            return false;

        return ServerContext.TryParseCanonical(host, port, out context);
    }
}
