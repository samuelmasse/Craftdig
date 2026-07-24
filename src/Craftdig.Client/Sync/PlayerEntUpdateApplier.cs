namespace Craftdig.Client;

[Player]
public class PlayerEntUpdateApplier(
    WorldEntSyncCatalog worldCatalog,
    WorldModuleIndices moduleIndices,
    WorldUniverse universe,
    DimensionEntSyncCatalog dimensionCatalog,
    DimensionRemoteInterpolation remoteInterpolation,
    PlayerIdentityCache identityCache,
    PlayerEntReplicas replicas,
    PlayerInventoryActions inventory) : IEntSyncReadResolver
{
    public void Apply(PlayerEntUpdate update)
    {
        var command = update.Command;
        var catalog = Catalog(command.ScopeId);
        var reader = new EntUpdateReader(command, update.Buffer.AsSpan(0, update.Length));

        ApplyCreateRecords(command.ScopeId, ref reader);
        ApplyStateRecords(command.ScopeId, catalog, ref reader);
        ApplyDeleteRecords(ref reader);
    }

    Ent IEntSyncReadResolver.ModuleEnt(int index) => moduleIndices[index];

    EntMutIdx IEntSyncReadResolver.Ent(Guid id) => replicas.Ent(id);

    private void ApplyCreateRecords(uint scopeId, ref EntUpdateReader reader)
    {
        while (reader.TryReadCreate(out var record))
        {
            bool isOwner = (record.Flags & EntCreateFlags.Owner) != 0;
            replicas.Create(scopeId, record.Id, isOwner);
        }
    }

    private void ApplyStateRecords(
        uint scopeId,
        EntSyncCatalog catalog,
        ref EntUpdateReader reader)
    {
        while (reader.TryReadState(out var record))
            ApplyStateRecord(scopeId, catalog, record, ref reader);
    }

    private void ApplyDeleteRecords(ref EntUpdateReader reader)
    {
        while (reader.TryReadDelete(out var record))
            replicas.Delete(record.Id);
    }

    private void ApplyStateRecord(
        uint scopeId,
        EntSyncCatalog catalog,
        EntStateRecord record,
        ref EntUpdateReader reader)
    {
        var replica = replicas[record.Id];
        bool isFullState = (record.Flags & EntStateFlags.Full) != 0;
        bool reconcilesInventory = scopeId != 0 && replica.IsOwner;

        // Remove replayed predictions before applying owner components. EndAuthoritativeUpdate
        // captures the new server state and replays actions the server has not incorporated yet.
        if (reconcilesInventory)
            inventory.BeginAuthoritativeUpdate();

        int pageCount = (catalog.Components.Length + EntSyncCatalog.ComponentsPerMask - 1) /
            EntSyncCatalog.ComponentsPerMask;
        Span<ulong> receivedComponents = stackalloc ulong[pageCount];
        receivedComponents.Clear();

        ApplyStateComponents(
            scopeId,
            catalog,
            replica,
            record.ComponentCount,
            isFullState,
            receivedComponents,
            ref reader);

        if (isFullState)
            RemoveComponentsMissingFromFullState(catalog, replica, receivedComponents);

        FinalizeAppliedState(scopeId, record.Id, ref replica, isFullState);

        if (reconcilesInventory)
            inventory.EndAuthoritativeUpdate();
    }

    private void ApplyStateComponents(
        uint scopeId,
        EntSyncCatalog catalog,
        PlayerEntReplica replica,
        ushort count,
        bool isFullState,
        scoped Span<ulong> receivedComponents,
        ref EntUpdateReader reader)
    {
        for (int i = 0; i < count; i++)
        {
            var token = reader.ReadComponent();
            var component = catalog[token.Ordinal];
            receivedComponents[token.Ordinal / EntSyncCatalog.ComponentsPerMask] |=
                1UL << (token.Ordinal % EntSyncCatalog.ComponentsPerMask);

            if (token.IsPresent)
                ApplyPresentComponent(scopeId, component, replica, isFullState, ref reader);
            else component.Unset(replica.Ent);
        }
    }

    private void RemoveComponentsMissingFromFullState(
        EntSyncCatalog catalog,
        PlayerEntReplica replica,
        ReadOnlySpan<ulong> receivedComponents)
    {
        // A full state is replacement state. Omitted receivable components must not survive
        // from a previous incarnation or an earlier full snapshot of the replica.
        for (int ordinal = 0; ordinal < catalog.Components.Length; ordinal++)
        {
            var component = catalog[ordinal];
            int page = ordinal / EntSyncCatalog.ComponentsPerMask;
            ulong bit = 1UL << (ordinal % EntSyncCatalog.ComponentsPerMask);
            if (CanReceive(component, replica.IsOwner) && (receivedComponents[page] & bit) == 0)
                component.Unset(replica.Ent);
        }
    }

    private void ApplyPresentComponent(
        uint scopeId,
        EntSyncComponent component,
        PlayerEntReplica replica,
        bool isFullState,
        ref EntUpdateReader reader)
    {
        if (scopeId == 0 || replica.IsOwner)
        {
            reader.ReadPresent(component, replica.Ent, this);
            return;
        }

        if (component.Component == DimensionComponents.Position.Component)
        {
            ApplyRemotePosition(replica.Ent, isFullState, component, ref reader);
            return;
        }

        if (component.Component == DimensionComponents.LookAt.Component)
        {
            ApplyRemoteLookAt(replica.Ent, isFullState, component, ref reader);
            return;
        }

        reader.ReadPresent(component, replica.Ent, this);
    }

    private void ApplyRemotePosition(
        EntMutIdx ent,
        bool isFullState,
        EntSyncComponent component,
        ref EntUpdateReader reader)
    {
        // The ECS component is the newest authoritative target. Presentation starts from the
        // currently displayed value so a mid-interpolation update cannot snap backward.
        bool hadPosition = ent.Has<Vec3d, DimensionComponents.Position>();
        var displayedPosition = hadPosition ? remoteInterpolation.Position(ent) : default;
        reader.ReadPresent(component, ent, this);

        if (hadPosition && !isFullState)
            remoteInterpolation.StartPosition(ent, displayedPosition);
        else remoteInterpolation.SnapPosition(ent);
    }

    private void ApplyRemoteLookAt(
        EntMutIdx ent,
        bool isFullState,
        EntSyncComponent component,
        ref EntUpdateReader reader)
    {
        bool hadLookAt = ent.Has<Vec3, DimensionComponents.LookAt>();
        var displayedLookAt = hadLookAt ? remoteInterpolation.LookAt(ent) : default;
        reader.ReadPresent(component, ent, this);

        if (hadLookAt && !isFullState)
            remoteInterpolation.StartLookAt(ent, displayedLookAt);
        else remoteInterpolation.SnapLookAt(ent);
    }

    private void FinalizeAppliedState(
        uint scopeId,
        Guid id,
        ref PlayerEntReplica replica,
        bool isFullState)
    {
        if (scopeId == 0)
        {
            if (replica.Ent.IsUniverse)
                universe.Bind(replica.Ent);
            return;
        }

        if (replica.Ent.IsPlayer)
            identityCache.ObservePlayerEnt(id);
        else identityCache.ForgetPlayerEnt(id);

        if (replica.IsOwner)
        {
            if (isFullState)
                replicas.MarkOwnerReady(replica.Ent);
            return;
        }

        replicas.UpdateRemoteChunkMembership(id, ref replica);
    }

    private EntSyncCatalog Catalog(uint scopeId) => scopeId == 0 ? worldCatalog : dimensionCatalog;

    private bool CanReceive(EntSyncComponent component, bool isOwner) =>
        component.Audience == EntSyncAudience.Everyone ||
        component.Audience == (isOwner ? EntSyncAudience.Owner : EntSyncAudience.Observers);
}
