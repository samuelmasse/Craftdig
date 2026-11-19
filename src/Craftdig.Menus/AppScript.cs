namespace Craftdig;

[App]
public class AppScript(Log log, AppMenuVisibility menus) : Script
{
    // Let UI dispatch the restore press while its controls are still hidden.
    public override float Order => 1;

    public override void Update(double time) => menus.Update();

    public override void Unload() => log.Info("Unloading app");
}
