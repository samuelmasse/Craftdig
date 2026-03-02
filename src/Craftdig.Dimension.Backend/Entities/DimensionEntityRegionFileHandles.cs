namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionEntityRegionFileHandles
{
    private readonly Dictionary<string, SafeFileHandle> handles = [];

    public SafeFileHandle this[string file]
    {
        get
        {
            if (!handles.TryGetValue(file, out var handle))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file)!);

                handle = File.OpenHandle(file, FileMode.OpenOrCreate,
                    FileAccess.ReadWrite, FileShare.Read, FileOptions.RandomAccess);

                handles.Add(file, handle);
            }

            return handle;
        }
    }
}
