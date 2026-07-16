namespace Craftdig.Player.Frontend;

[Player]
public class PlayerInventoryActions(
    DimensionInventoryOperations operations,
    PlayerEnt player)
{
    private const int Capacity = 64;

    private readonly PendingInventoryAction[] pending = new PendingInventoryAction[Capacity];
    private readonly PlayerInventorySnapshot authoritative = new();
    private int start;
    private int count;
    private uint nextSequence = 1;
    private bool enabled;
    private bool initialized;

    public void Enable()
    {
        enabled = true;
        initialized = false;
        start = 0;
        count = 0;
        nextSequence = 1;
    }

    public void Submit(InventoryOperation operation)
    {
        if (!enabled || !initialized)
            return;

        if (count == Capacity)
        {
            Reconcile();
            return;
        }

        pending[(start + count) % Capacity] = new(nextSequence++, operation);
        count++;
    }

    public bool TryTakeUnsent(out uint sequence, out InventoryOperation operation)
    {
        for (int i = 0; i < count; i++)
        {
            int index = (start + i) % Capacity;
            ref var action = ref pending[index];
            if (action.Sent || action.Status == PendingInventoryActionStatus.Rejected)
                continue;

            action.Sent = true;
            sequence = action.Sequence;
            operation = action.Operation;
            return true;
        }

        sequence = 0;
        operation = default;
        return false;
    }

    public void Acknowledge(uint sequence, bool accepted, long revision)
    {
        for (int i = 0; i < count; i++)
        {
            ref var action = ref pending[(start + i) % Capacity];
            if (action.Sequence != sequence)
                continue;

            action.Status = accepted
                ? PendingInventoryActionStatus.Accepted
                : PendingInventoryActionStatus.Rejected;
            action.Revision = revision;
            Reconcile();
            return;
        }
    }

    public void BeginAuthoritativeUpdate()
    {
        if (enabled && initialized)
            authoritative.Restore(player);
    }

    public void EndAuthoritativeUpdate()
    {
        if (!enabled)
            return;

        authoritative.Capture(player);
        initialized = true;
        Reconcile();
    }

    private void Reconcile()
    {
        if (!initialized)
            return;

        authoritative.Restore(player);
        DropCompleted();
        for (int i = 0; i < count; i++)
        {
            ref var action = ref pending[(start + i) % Capacity];
            if (action.Status != PendingInventoryActionStatus.Rejected)
                operations.Apply(player, action.Operation);
        }
    }

    private void DropCompleted()
    {
        while (count > 0)
        {
            ref var action = ref pending[start];
            bool completed = action.Status == PendingInventoryActionStatus.Rejected ||
                action.Status == PendingInventoryActionStatus.Accepted &&
                authoritative.Revision >= action.Revision;
            if (!completed)
                return;

            action = default;
            start = (start + 1) % Capacity;
            count--;
        }
    }
}
