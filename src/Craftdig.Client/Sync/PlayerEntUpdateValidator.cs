namespace Craftdig;

[Player]
public class PlayerEntUpdateValidator(
    WorldEntSyncCatalog worldCatalog,
    WorldModuleIndices moduleIndices,
    DimensionEntSyncCatalog dimensionCatalog,
    PlayerEntReplicas replicas) : IEntSyncReadValidator
{
    private readonly HashSet<Guid> createdIdsInPacket = [];
    private readonly HashSet<Guid> ownerIdsInPacket = [];
    private readonly HashSet<Guid> deletedIdsInPacket = [];

    public bool Validate(PlayerEntUpdate update)
    {
        ResetPacketState();

        var command = update.Command;
        var catalog = Catalog(command.ScopeId);
        var validator = new EntUpdateValidator(update.Buffer.AsSpan(0, update.Length));

        if (!ValidateCreateRecords(command.CreateCount, ref validator))
            return false;
        if (!ValidateStateRecords(command.StateCount, catalog, ref validator))
            return false;
        if (!ValidateDeleteRecords(command.DeleteCount, ref validator))
            return false;

        // Counts describe the complete payload. Trailing bytes indicate a schema or framing error.
        return validator.Complete;
    }

    bool IEntSyncReadValidator.IsModuleIndexValid(int index) => moduleIndices.Contains(index);

    bool IEntSyncReadValidator.IsEntIdValid(Guid id) => IsKnown(id);

    private void ResetPacketState()
    {
        createdIdsInPacket.Clear();
        ownerIdsInPacket.Clear();
        deletedIdsInPacket.Clear();
    }

    private bool ValidateCreateRecords(ushort count, ref EntUpdateValidator validator)
    {
        for (int i = 0; i < count; i++)
        {
            if (!validator.TryReadCreate(out var record) ||
                record.Id == Guid.Empty ||
                (record.Flags & ~EntCreateFlags.Owner) != 0 ||
                replicas.Contains(record.Id) ||
                !createdIdsInPacket.Add(record.Id))
                return false;

            if ((record.Flags & EntCreateFlags.Owner) != 0)
                ownerIdsInPacket.Add(record.Id);
        }

        return true;
    }

    private bool ValidateStateRecords(
        ushort count,
        EntSyncCatalog catalog,
        ref EntUpdateValidator validator)
    {
        int pageCount = (catalog.Components.Length + EntSyncCatalog.ComponentsPerMask - 1) /
            EntSyncCatalog.ComponentsPerMask;
        Span<ulong> componentsSeen = stackalloc ulong[pageCount];

        for (int i = 0; i < count; i++)
        {
            if (!validator.TryReadState(out var record) ||
                record.Id == Guid.Empty ||
                (record.Flags & ~EntStateFlags.Full) != 0 ||
                !IsKnown(record.Id))
                return false;

            bool isOwner = IsOwner(record.Id);
            componentsSeen.Clear();
            if (!ValidateStateComponents(
                    record.ComponentCount,
                    catalog,
                    isOwner,
                    componentsSeen,
                    ref validator))
                return false;
        }

        return true;
    }

    private bool ValidateStateComponents(
        ushort count,
        EntSyncCatalog catalog,
        bool isOwner,
        scoped Span<ulong> componentsSeen,
        ref EntUpdateValidator validator)
    {
        for (int i = 0; i < count; i++)
        {
            if (!validator.TryReadComponent(out var token) || token.Ordinal >= catalog.Components.Length)
                return false;

            int page = token.Ordinal / EntSyncCatalog.ComponentsPerMask;
            ulong bit = 1UL << (token.Ordinal % EntSyncCatalog.ComponentsPerMask);
            if ((componentsSeen[page] & bit) != 0)
                return false;
            componentsSeen[page] |= bit;

            var component = catalog[token.Ordinal];
            if (!CanReceive(component, isOwner) ||
                token.IsPresent && !validator.TryReadPresent(component, this))
                return false;
        }

        return true;
    }

    private bool ValidateDeleteRecords(ushort count, ref EntUpdateValidator validator)
    {
        for (int i = 0; i < count; i++)
        {
            if (!validator.TryReadDelete(out var record) ||
                record.Id == Guid.Empty ||
                !IsKnown(record.Id) ||
                !deletedIdsInPacket.Add(record.Id))
                return false;
        }

        return true;
    }

    private bool IsKnown(Guid id) => replicas.Contains(id) || createdIdsInPacket.Contains(id);

    private bool IsOwner(Guid id) => createdIdsInPacket.Contains(id)
        ? ownerIdsInPacket.Contains(id)
        : replicas.IsOwner(id);

    private EntSyncCatalog Catalog(uint scopeId) => scopeId == 0 ? worldCatalog : dimensionCatalog;

    private bool CanReceive(EntSyncComponent component, bool isOwner) =>
        component.Audience == EntSyncAudience.Everyone ||
        component.Audience == (isOwner ? EntSyncAudience.Owner : EntSyncAudience.Observers);
}
