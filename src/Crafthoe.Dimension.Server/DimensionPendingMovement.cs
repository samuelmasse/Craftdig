namespace Crafthoe.Dimension.Server;

[Dimension]
public class DimensionPendingMovement(DimensionPlayerBag bag)
{
    public void Tick()
    {
        foreach (var ent in bag.Ents)
            ProcessMovement((EntMut)ent);
    }

    private void ProcessMovement(EntMut ent)
    {
        var pending = ent.PendingMovement();
        if (pending == null)
            return;

        ref var mov = ref ent.Movement();

        int count = pending.Count;

        Console.WriteLine(count);

        while (count > 4)
        {
            pending.TryDequeue(out _);
            count--;
        }

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
}
