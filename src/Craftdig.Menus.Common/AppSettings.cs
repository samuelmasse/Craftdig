namespace Craftdig.Menus.Common;

[App]
public class AppSettings
{
    private readonly RootUiScale scale;
    private readonly AppPaths paths;
    private readonly JsonSerializerOptions options;
    private readonly string file;

    private SettingsData data;

    public AppSettings(RootUiScale scale, AppPaths paths)
    {
        this.scale = scale;
        this.paths = paths;

        data = new();
        file = Path.Join(paths.GamePath, "Settings.json");

        options = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        try
        {
            var json = File.ReadAllText(file);
            Apply(JsonSerializer.Deserialize<SettingsData>(json) ?? new());
        }
        catch { }
    }

    public float Scale
    {
        get => data.Scale ?? scale.Scale;
        set => Apply(data with { Scale = value });
    }

    private void Apply(SettingsData newData)
    {
        if (newData.Scale != data.Scale)
            scale.Scale = newData.Scale.GetValueOrDefault();

        data = newData;
        Save();
    }

    private void Save()
    {
        Directory.CreateDirectory(paths.GamePath);
        File.WriteAllText(file, JsonSerializer.Serialize(data, options));
    }

    private record class SettingsData
    {
        public float? Scale { get; set; }
    }
}
