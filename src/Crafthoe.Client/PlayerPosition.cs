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
        expected.Add(new()
        {
            Position = ent.Ent.Position(),
            Velocity = ent.Ent.Velocity(),
            IsFlying = ent.Ent.IsFlying(),
            IsSprinting = ent.Ent.IsSprinting()
        });

        if (expected.Count > tolerance)
            expected.RemoveAt(0);

        if (slowDownReceiver.ShouldSlowDown())
            slowdown = tolerance;

        if (positionUpdateReceiver.Count < tolerance)
            slowdown = 1;

        if (slowdown == 0)
        {
            var latest = positionUpdateReceiver.Latest;

            if (!HasMatchingCommand(latest))
            {
                slowdown = tolerance;
                ApplyServerPosition(latest);
                Console.WriteLine($"Corrected {c++}");
            }
        }
        else if (positionUpdateReceiver.Count > 0)
            ApplyServerPosition(positionUpdateReceiver.Latest);
    }

    public void Stream()
    {
        ref var movement = ref ent.Ent.Movement();
        ref var construction = ref ent.Ent.Construction();

        if (slowdown == 0)
            socket.Send(new MovePlayerCommand() { Movement = movement, Construction = construction });
        else
        {
            movement = default;
            construction = default;
            slowdown--;
        }
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
