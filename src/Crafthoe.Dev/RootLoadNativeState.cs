namespace Crafthoe.Dev;

using Crafthoe.Menus.Multiplayer;

[Root]
public class RootLoadNativeState(RootState state, RootScope scope) : State
{
    public override void Load()
    {
        var app = scope.Scope<AppScope>();
        app.Add(new AppMods([new(typeof(ModuleNativeLoader))]));

        var user = "testuser";
        var options = new AppClientOptions()
        {
            AllowRawTcp = true,
            AllowNoAuth = true,
            UseRawTcp = true,
            DefaultNoAuthUser = user,
            NoAuthUser = user
        };

        app.Add(options);

        state.Current = app.New<AppInitializeState>();
    }
}
