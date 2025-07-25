namespace AlvorEngine.Loop;

public class State
{
    public virtual void Load() { }
    public virtual void Unload() { }
    public virtual void Update(double time) { }
    public virtual void Render() { }
    public virtual void Draw() { }
}
