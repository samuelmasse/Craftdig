namespace Craftdig.App.Frontend;

[App]
public class AppFiles
{
    private readonly List<string> roots = [AppDomain.CurrentDomain.BaseDirectory];

    public string Root => roots[^1];

    public string this[string path]
    {
        get
        {
            for (int i = roots.Count - 1; i >= 0; i--)
            {
                var root = roots[i];
                string rpath = Path.Join(root, path);
                if (File.Exists(rpath))
                    return rpath;
            }

            throw new Exception($"File not found {path}");
        }
    }

    public void AddRoot(string root) => roots.Add(root);
}
