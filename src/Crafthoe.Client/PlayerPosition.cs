namespace Crafthoe.Client;

[Player]
public class PlayerPosition(
    AppLog log,
    PlayerSocket socket,
    PlayerEnt ent,
    PlayerPositionUpdateReceiver positionUpdateReceiver,
    PlayerSlowDownReceiver slowDownReceiver)
{
    private readonly List<PositionUpdateCommand> expected = [];
    private readonly int tolerance = 12;
    private double matching = 1;
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
                log.Info("Corrected {0}", c++);
                foreach (PositionUpdateCommand command in expected)
                    log.Info("{0} : {1}", command.Position, Vector3d.Distance(command.Position, positionUpdateReceiver.Latest.Position));
                log.Info("But:");
                log.Info(positionUpdateReceiver.Latest.Position);

                expected.Clear();
                slowdown = tolerance;
                matching = 1;
                ApplyServerPosition(latest);
            }
        }
        else if (positionUpdateReceiver.Count > 0)
            ApplyServerPosition(positionUpdateReceiver.Latest);

        matching = Math.Max(matching * 0.95, 0.001);
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
        double min = double.PositiveInfinity;

        foreach (var ex in expected)
        {
            var dist = Vector3d.Distance(command.Position, ex.Position);
            if (dist < min)
                min = dist;
        }

        log.Debug((int)(min * 1000));

        return min < matching;
    }

    private void ApplyServerPosition(PositionUpdateCommand server)
    {
        ent.Ent.Position() = server.Position;
        ent.Ent.Velocity() = server.Velocity;
        ent.Ent.IsFlying() = server.IsFlying;
        ent.Ent.IsSprinting() = server.IsSprinting;
    }
}
