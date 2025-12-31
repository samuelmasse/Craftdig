namespace Craftdig.Server;

[Server]
public class ServerIdentities(ServerConfig config)
{
    private readonly string file = Path.Join(config.RootPath, "Identities.json");
    private Dictionary<string, string>? known;

    public bool Verify(string email, string uid)
    {
        lock (this)
        {
            if (known == null)
                Read();

            if (known!.TryGetValue(email, out var knownUid))
            {
                if (uid != knownUid)
                    return false;
            }
            else
            {
                known.Add(email, uid);
                Persist();
            }

            return true;
        }
    }

    private void Read()
    {
        if (!File.Exists(file))
            Persist();

        var json = File.ReadAllText(file);
        known = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
    }

    private void Persist()
    {
        known ??= [];
        var json = JsonSerializer.Serialize(known, new JsonSerializerOptions() { WriteIndented = true });
        File.WriteAllText(file, json);
    }
}
