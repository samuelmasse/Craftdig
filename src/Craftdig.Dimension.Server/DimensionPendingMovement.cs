namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionPendingMovement(AppLog log, DimensionSockets sockets)
{
    public void Tick()
    {
        foreach (var ns in sockets.Span)
        {
            if (ns.PlayerSpawnPhase == PlayerSpawnPhase.Active)
                ProcessMovement(ns);
        }
    }

    private void ProcessMovement(NetSocket ns)
    {
        var ent = ns.SocketPlayer;
        var pending = ent.PendingMovement;
        if (pending == null)
            return;

        int ahead = pending.Count;
        while (ahead > 12)
        {
            pending.TryDequeue(out _);
            ahead--;
        }

        log.Debug("{0} ahead by {1}", ent.Tag, ahead);

        if (ahead > 2)
        {
            if (ent.PendingMovementWait > Wait(ahead))
            {
                ns.Send<SlowTickCommand>();
                ent.PendingMovementWait = 0;
            }

            ent.PendingMovementWait++;
        }
        else ent.PendingMovementWait = 0;

        var mov = ent.Movement;
        var constr = ent.Construction;
        var drop = ent.Drop;

        if (pending.TryDequeue(out var cmd))
        {
            var cmov = cmd.Movement;

            if (cmov.Sprint != MovementAction.None)
                mov.Sprint = cmov.Sprint;
            if (cmov.Fly != MovementAction.None)
                mov.Fly = cmov.Fly;
            if (cmov.Jump)
                mov.Jump = true;

            mov.FlyUp = cmov.FlyUp;
            mov.FlyDown = cmov.FlyDown;
            mov.Vector = cmov.Vector;
            if (cmov.LookAt != default)
            {
                mov.LookAt = cmov.LookAt;
                if (ent.LookAt != cmov.LookAt)
                    ent.LookAt = cmov.LookAt;
            }

            constr = cmd.Construction;
            drop = cmd.Drop;
        }

        ent.Movement = mov;
        ent.Drop = drop;
        ent.Construction = constr;
    }

    private int Wait(int ahead) => ahead switch
    {
        3 => 24,
        4 => 12,
        5 => 6,
        6 => 3,
        7 => 1,
        _ => 0
    };
}
