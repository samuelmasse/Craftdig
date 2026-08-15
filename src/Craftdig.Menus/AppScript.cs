namespace Craftdig;

[App]
public class AppScript(Log log) : Script
{
    public override void Unload() => log.Info("Unloading app");
}
