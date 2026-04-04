namespace TrogloUI;

[Root]
public class RootUiClipping
{
    public Box2 IntersectClips(Box2? current, Box2 next)
    {
        if (current is not Box2 existing)
            return next;

        var min = Vector2.ComponentMax(existing.Min, next.Min);
        var max = Vector2.ComponentMin(existing.Max, next.Max);

        if (max.X < min.X || max.Y < min.Y)
            return default;

        return new(min, max);
    }
}
