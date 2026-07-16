namespace Craftdig.Client;

[Player]
public class PlayerPosition(
    PlayerSocket socket,
    PlayerEnt ent,
    PlayerPositionUpdateReceiver positionUpdateReceiver)
{
    private readonly List<PositionUpdateCommand> expected = [];
    private readonly List<PositionUpdateCommand> received = [];
    private readonly int tolerance = 12;
    private bool correcting = true;
    private double matching = 1;
    private int listen;
    private int debounce;

    public ReadOnlySpan<PositionUpdateCommand> Expected => CollectionsMarshal.AsSpan(expected);
    public ReadOnlySpan<PositionUpdateCommand> Received => CollectionsMarshal.AsSpan(received);
    public int Tolerance => tolerance;
    public ref bool Correcting => ref correcting;
    public double Matching => matching;
    public int Listen => listen;
    public int Debounce => debounce;

    public bool Prepare()
    {
        if (positionUpdateReceiver.Count == 0)
            return false;

        var server = positionUpdateReceiver.Latest;
        ApplyServerPosition(server);
        ent.LookAt = server.LookAt;
        return true;
    }

    public void Tick()
    {
        expected.Add(new()
        {
            Position = ent.Position,
            Velocity = ent.Velocity,
            IsFlying = ent.IsFlying,
            IsSprinting = ent.IsSprinting
        });

        if (expected.Count > tolerance)
            expected.RemoveAt(0);

        if (positionUpdateReceiver.Count > 0)
        {
            received.Add(positionUpdateReceiver.Latest);
            if (received.Count > tolerance)
                received.RemoveAt(0);
        }

        if (positionUpdateReceiver.Count < tolerance)
            listen = 1;

        if (listen == 0 && correcting && debounce == 0 && !HasMatchingCommand())
        {
            listen = tolerance;
            debounce = tolerance * 10;
            matching = 1;

            // Intentionally add some latency
            var command = new MovePlayerCommand
            {
                Movement = new() { LookAt = ent.LookAt }
            };
            socket.Send(command);
            socket.Send(command);
        }

        if (listen > 0 && received.Count > 0)
            ApplyServerPosition(received[^1]);

        if (debounce > 0)
            debounce--;

        matching = Math.Max(matching * 0.97, 0.001);
    }

    public void Stream()
    {
        if (listen > 0)
        {
            ent.Movement = new() { LookAt = ent.LookAt };
            ent.Construction = default;
            listen--;
        }

        socket.Send(new MovePlayerCommand() { Movement = ent.Movement, Construction = ent.Construction, Drop = ent.Drop });
        ent.Drop = default;
    }

    private bool HasMatchingCommand()
    {
        double min = double.PositiveInfinity;
        double max = 0;

        foreach (var command in received)
        {
            foreach (var ex in expected)
            {
                var dist = Vec3d.Distance(command.Position, ex.Position);
                if (dist < min)
                    min = dist;
                if (dist > max)
                    max = dist;
            }
        }

        return min < matching;
    }

    private void ApplyServerPosition(PositionUpdateCommand server)
    {
        ent.Position = server.Position;
        ent.Velocity = server.Velocity;
        ent.IsFlying = server.IsFlying;
        ent.IsSprinting = server.IsSprinting;
    }
}
