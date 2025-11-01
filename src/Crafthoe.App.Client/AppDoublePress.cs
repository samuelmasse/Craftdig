namespace Crafthoe.App;

[App]
public class AppDoublePress(RootKeyboard keyboard)
{
    private readonly Stopwatch watch = Stopwatch.StartNew();
    private readonly double[] last = new double[0x200];

    public bool IsDoublePressed(Keys key)
    {
        if (!keyboard.IsKeyPressed(key))
            return false;

        double now = watch.Elapsed.TotalMilliseconds;
        ref double time = ref last[(int)key];

        double delta = now - time;
        if (delta < 250)
        {
            time = 0;
            return true;
        }
        else time = now;

        return false;
    }
}
