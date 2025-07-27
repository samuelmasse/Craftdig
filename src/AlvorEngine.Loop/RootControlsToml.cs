namespace AlvorEngine.Loop;

[Root]
public class RootControlsToml(RootControls controls)
{
    public void AddFromFile(string file)
    {
        var text = File.ReadAllText(file);
        var model = Toml.ToModel<Dictionary<string, KeyBinding>>(text, null, new() { ConvertPropertyName = (s) => s });

        foreach (var key in model.Keys)
        {
            var control = controls[key.Split('-')[0]];
            control.Bind(model[key]);
        }
    }
}
