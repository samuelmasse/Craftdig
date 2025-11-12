namespace Crafthoe.Server;

[Server]
public class ServerIdentities(ServerPaths paths)
{
    private readonly string file = Path.Join(paths.Root, "Identities.json");
    private Dictionary<string, string>? known;

    public void Verify(string email, string uid)
    {
        lock (this)
        {
            if (known == null)
                Read();

            if (known!.TryGetValue(email, out var knownUid))
            {
                if (uid != knownUid)
                    throw new Exception();
            }
            else
            {
                known.Add(email, uid);
                Persist();
            }
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
