namespace Crafthoe.Player;

[Player]
public class PlayerTeleporter(
    RootKeyboard keyboard,
    WorldMeta meta,
    PlayerEnt player)
{
    private readonly Random rng = new(meta.Seed);
    private readonly List<Vector3d> history = [];
    private readonly Stopwatch sw = Stopwatch.StartNew();
    private int index;

    public void Update()
    {
        if (history.Count == 0)
            history.Add(player.Ent.Position());

        if (keyboard.IsKeyPressedRepeated(Keys.T) || sw.Elapsed.TotalMilliseconds > 800)
        {
            sw.Restart();

            while (history.Count > index + 1)
                history.RemoveAt(history.Count - 1);

            player.Ent.Position() = (
                rng.Next(-500_000_000, 500_000_000),
                rng.Next(-500_000_000, 500_000_000),
                player.Ent.Position().Z);

            history.Add(player.Ent.Position());
            index++;
        }

        if (keyboard.IsKeyPressedRepeated(Keys.R))
        {
            index--;
            if (index < 0)
                index = history.Count - 1;

            player.Ent.Position() = history[index];
        }

        if (keyboard.IsKeyPressedRepeated(Keys.Y))
        {
            index++;
            if (index >= history.Count)
                index = 0;

            player.Ent.Position() = history[index];
        }
    }
}
