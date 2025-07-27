namespace AlvorEngine;

[Root]
public class RootScale(RootScreen screen)
{
    private int numerator = (int)Math.Round(screen.MonitorScale * 4);
    private int denominator = 4;

    public ref int Numerator => ref numerator;
    public ref int Denominator => ref denominator;

    public float Scale => Numerator / (float)Denominator;

    public int this[int value] => (int)(value * Scale);
}
