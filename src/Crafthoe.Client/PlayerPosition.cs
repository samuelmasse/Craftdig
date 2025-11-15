namespace Crafthoe.Client;

[Player]
public class PlayerPosition(
    PlayerSocket socket,
    PlayerEnt ent,
    PlayerPositionUpdateReceiver positionUpdateReceiver,
    PlayerSlowDownReceiver slowDownReceiver)
{
    private readonly List<PositionUpdateCommand> expected = [];
    private readonly int tolerance = 12;
    private int slowdown;
    private int c;

    public void Tick()
    {
        if (slowDownReceiver.ShouldSlowDown())
        {
            expected.Clear();
            slowdown = tolerance;
        }

        expected.Add(new()
        {
            Position = ent.Ent.Position(),
            Velocity = ent.Ent.Velocity(),
            IsFlying = ent.Ent.IsFlying(),
            IsSprinting = ent.Ent.IsSprinting()
        });

        if (expected.Count > tolerance)
            expected.RemoveAt(0);

        if (expected.Count >= tolerance)
        {
            var latest = positionUpdateReceiver.Latest;

            if (!HasMatchingCommand(latest))
            {
                expected.Clear();
                slowdown = tolerance;

                ent.Ent.Position() = latest.Position;
                ent.Ent.Velocity() = latest.Velocity;
                ent.Ent.IsFlying() = latest.IsFlying;
                ent.Ent.IsSprinting() = latest.IsSprinting;

                Console.WriteLine($"Corrected {c++}");
            }
        }
        else if (positionUpdateReceiver.Count > 0)
            ApplyServerPosition(positionUpdateReceiver.Latest);
    }

    public void Stream()
    {
        ref var movement = ref ent.Ent.Movement();
        if (expected.Count < tolerance)
            movement = default;

        if (positionUpdateReceiver.Count > tolerance && slowdown == 0)
            socket.Send(new MovePlayerCommand() { Step = movement });
        else movement = default;

        if (slowdown > 0)
            slowdown--;
    }

    private bool HasMatchingCommand(PositionUpdateCommand command)
    {
        foreach (var ex in expected)
        {
            if (Vector3d.Distance(command.Position, ex.Position) < 0.1)
                return true;
        }

        return false;
    }

    private void ApplyServerPosition(PositionUpdateCommand server)
    {
        ent.Ent.Position() = server.Position;
        ent.Ent.Velocity() = server.Velocity;
        ent.Ent.IsFlying() = server.IsFlying;
        ent.Ent.IsSprinting() = server.IsSprinting;
    }
}
