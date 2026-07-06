namespace Craftdig.Menus.Common;

[App]
public class AppSettings
{
    private readonly RootUiScale scale;
    private readonly RootScreen screen;
    private readonly AppRenderDistance renderDistance;
    private readonly AppPaths paths;
    private readonly JsonSerializerOptions options;
    private readonly string file;

    private SettingsData data;

    public AppSettings(RootUiScale scale, RootScreen screen, AppRenderDistance renderDistance, AppPaths paths)
    {
        this.scale = scale;
        this.screen = screen;
        this.renderDistance = renderDistance;
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

    public bool Vsync
    {
        get => data.Vsync ?? screen.IsVSyncEnabled;
        set => Apply(data with { Vsync = value });
    }

    public bool Fullscreen
    {
        get => data.Fullscreen ?? screen.IsFullscreen;
        set => Apply(data with { Fullscreen = value });
    }

    public int RenderDistance
    {
        get => data.RenderDistance ?? renderDistance.Far;
        set => Apply(data with { RenderDistance = value });
    }

    private void Apply(SettingsData value)
    {
        var old = data;
        data = value;
        Save();

        if (Scale != old.Scale)
            scale.Scale = Scale;

        if (Vsync != old.Vsync)
            screen.IsVSyncEnabled = Vsync;

        if (Fullscreen != old.Fullscreen)
            screen.IsFullscreen = Fullscreen;

        if (RenderDistance != old.RenderDistance)
            renderDistance.Far = RenderDistance;
    }

    private void Save()
    {
        Directory.CreateDirectory(paths.GamePath);
        File.WriteAllText(file, JsonSerializer.Serialize(data, options));
    }

    private record class SettingsData
    {
        public float? Scale { get; set; }
        public bool? Vsync { get; set; }
        public bool? Fullscreen { get; set; }
        public int? RenderDistance { get; set; }
    }
}
