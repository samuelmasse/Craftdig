namespace AlvorEngine;

[Root]
public class RootState
{
    private State current = new();

    public State Current
    {
        get => current;
        set
        {
            current.Unload();
            current = value;
            current.Load();
        }
    }
}
