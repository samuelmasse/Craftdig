namespace Crafthoe.World;

[World]
public class WorldSpawnWrapper
{
    public const int Type = 2;

    public NetMessage Wrap()
    {
        return new(Type, []);
    }
}
