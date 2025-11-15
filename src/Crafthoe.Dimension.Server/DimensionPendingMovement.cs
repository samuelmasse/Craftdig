namespace Crafthoe.Dimension.Server;

[Dimension]
public class DimensionPendingMovement(DimensionSockets sockets)
{
    public void Tick()
    {
        foreach (var ns in sockets.Span)
            ProcessMovement(ns);
    }

    private void ProcessMovement(NetSocket ns)
    {
        var ent = ns.Ent.SocketPlayer();
        var pending = ent.PendingMovement();
        if (pending == null)
            return;

        int ahead = pending.Count;
        while (ahead > 12)
        {
            pending.TryDequeue(out _);
            ahead--;
        }

        Console.WriteLine(ahead);

        if (ahead > 1)
        {
            if (ent.PendingMovementWait() > Wait(ahead))
            {
                ns.Send<SlowTickCommand>();
                ent.PendingMovementWait() = 0;
            }

            if (ent.PendingMovementLongWait() > LongWait(ahead))
                ns.Send<SlowDownCommand>();

            ent.PendingMovementWait()++;
            ent.PendingMovementLongWait()++;
        }
        else
        {
            ent.PendingMovementWait() = 0;
            ent.PendingMovementLongWait() = 0;
        }

        ref var mov = ref ent.Movement();

        if (pending.TryDequeue(out var step))
        {
            if (step.Sprint != MovementAction.None)
                mov.Sprint = step.Sprint;
            if (step.Fly != MovementAction.None)
                mov.Fly = step.Fly;
            if (step.Jump)
                mov.Jump = true;

            mov.FlyUp = step.FlyUp;
            mov.FlyDown = step.FlyDown;
            mov.Vector = step.Vector;
        }
    }

    private int Wait(int ahead) => ahead switch
    {
        2 => 24,
        3 => 12,
        4 => 6,
        5 => 3,
        6 => 1,
        _ => 0
    };

    private int LongWait(int ahead) => ahead switch
    {
        2 => 120,
        3 => 60,
        4 => 30,
        5 => 15,
        6 => 7,
        _ => 0
    };
}
