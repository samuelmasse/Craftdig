namespace Craftdig.Player.Frontend;

[Player]
public class PlayerTeleporter(
    RootKeyboard keyboard,
    PlayerEnt player)
{
    private readonly Random rng = new();
    private readonly List<Vec3d> history = [];
    private int index;

    public void Update()
    {
        if (history.Count == 0)
            history.Add(player.Position);

        if (keyboard.IsKeyPressedRepeated(Keys.T))
        {
            while (history.Count > index + 1)
                history.RemoveAt(history.Count - 1);

            player.Position = (
                rng.Next(-2_000_000, 2_000_000),
                rng.Next(-2_000_000, 2_000_000),
                player.Position.Z);

            history.Add(player.Position);
            index++;
        }

        if (keyboard.IsKeyPressedRepeated(Keys.R))
        {
            index--;
            if (index < 0)
                index = history.Count - 1;

            player.Position = history[index];
        }

        if (keyboard.IsKeyPressedRepeated(Keys.Y))
        {
            index++;
            if (index >= history.Count)
                index = 0;

            player.Position = history[index];
        }
    }
}
