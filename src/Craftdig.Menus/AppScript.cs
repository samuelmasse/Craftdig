namespace Craftdig.Menus;

[App]
public class AppScript(AppScope scope) : Script
{
    public override void Unload() =>
        scope.Scope<AppLoaderScope>().Get<AppUnloader>().Run();
}
