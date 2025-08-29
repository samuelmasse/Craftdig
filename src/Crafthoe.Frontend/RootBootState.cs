namespace Crafthoe.Frontend;

[Root]
public class RootBootState(RootState state, RootScope scope) : State
{
    public override void Load() =>
        state.Current = scope.Scope<AppScope>().New<AppInitializeState>();
}
