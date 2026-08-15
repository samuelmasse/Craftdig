namespace Craftdig;

[World]
public class WorldEntStreamer(
    WorldEntSyncCatalog catalog,
    WorldEnts ents,
    WorldEntSyncResolver resolver,
    WorldScratchedBag scratchedBag,
    WorldEntDisposals disposals,
    WorldPlayerSockets sockets,
    WorldDimensionBag dimensions)
{
    private readonly byte[] buffer = new byte[
        ProtocolLimits.MaxMessageSize - Unsafe.SizeOf<EntUpdateCommand>()];
    private readonly List<EntMutIdx> scratched = [];
    private readonly List<Guid> disposed = [];

    public void Stream()
    {
        foreach (var ent in scratchedBag.Ents)
            scratched.Add(ent);

        while (disposals.TryTake(out var entry))
        {
            if (entry.Id != Guid.Empty)
                disposed.Add(entry.Id);
        }

        var viewers = sockets.Span;
        foreach (var socket in viewers)
        {
            if (socket.IsWorldSyncReady)
                WriteChanges(socket);
        }

        bool wroteBaseline = false;
        foreach (var socket in viewers)
        {
            if (!socket.IsWorldSyncReady)
            {
                WriteBaseline(socket);
                wroteBaseline = true;
            }
        }

        if (viewers.Length != 0)
        {
            if (wroteBaseline)
                PublishBaselineEnts();
            else PublishScratchedEnts();
        }

        foreach (var ent in scratched)
            Finish(ent);

        scratched.Clear();
        disposed.Clear();
    }

    private void WriteChanges(NetSocket socket)
    {
        WriteCreates(socket, false);
        WriteStates(socket, false);
        WriteDeletes(socket);
    }

    private void WriteBaseline(NetSocket socket)
    {
        var dimension = dimensions.Ents[0];
        var dimensionCatalog = dimension.DimensionScope.Get<DimensionEntSyncCatalog>();
        socket.Send(new EntSyncSchemaCommand(catalog.SchemaHash, dimensionCatalog.SchemaHash));

        WriteCreates(socket, true);
        WriteStates(socket, true);
        socket.IsWorldSyncReady = true;
    }

    private void WriteCreates(NetSocket socket, bool baseline)
    {
        var writer = new EntUpdateWriter(buffer);
        if (baseline)
        {
            foreach (var ent in ents)
            {
                if (!IsReplicated(ent))
                    continue;

                WriteCreate(socket, ref writer, ent);
            }
        }
        else
        {
            foreach (var ent in scratched)
            {
                if (ent.IsWorldSyncPublished || !HasSyncedComponents(ent))
                    continue;

                WriteCreate(socket, ref writer, ent);
            }
        }
        Flush(socket, ref writer);
    }

    private void WriteCreate(NetSocket socket, ref EntUpdateWriter writer, EntMutIdx ent)
    {
        var flags = IsOwner(socket, ent) ? EntCreateFlags.Owner : EntCreateFlags.None;
        if (!writer.TryWriteCreate(ent.Id, flags))
        {
            Flush(socket, ref writer);
            writer.TryWriteCreate(ent.Id, flags);
        }
    }

    private void WriteStates(NetSocket socket, bool baseline)
    {
        var writer = new EntUpdateWriter(buffer);
        if (baseline)
        {
            foreach (var ent in ents)
            {
                if (!IsReplicated(ent))
                    continue;

                WriteFull(socket, ref writer, ent, ent.Id, IsOwner(socket, ent));
            }
        }
        else
        {
            foreach (var ent in scratched)
            {
                bool hasSyncedComponents = HasSyncedComponents(ent);
                if (!hasSyncedComponents)
                    continue;

                if (ent.IsWorldSyncPublished)
                    WriteDiff(socket, ref writer, ent, ent.Id, IsOwner(socket, ent));
                else WriteFull(socket, ref writer, ent, ent.Id, IsOwner(socket, ent));
            }
        }
        Flush(socket, ref writer);
    }

    private void WriteDeletes(NetSocket socket)
    {
        var writer = new EntUpdateWriter(buffer);
        foreach (var ent in scratched)
        {
            if (ent.IsWorldSyncPublished && !HasSyncedComponents(ent))
                WriteDelete(socket, ref writer, ent.Id);
        }

        foreach (Guid id in disposed)
            WriteDelete(socket, ref writer, id);
        Flush(socket, ref writer);
    }

    private void WriteDelete(NetSocket socket, ref EntUpdateWriter writer, Guid id)
    {
        if (!writer.TryWriteDelete(id))
        {
            Flush(socket, ref writer);
            writer.TryWriteDelete(id);
        }
    }

    private void WriteFull(
        NetSocket socket,
        ref EntUpdateWriter writer,
        EntMutIdx ent,
        Guid id,
        bool owner)
    {
        MeasureFull(ent, owner, out ushort count, out int bytes);
        if (!writer.TryStartState(id, EntStateFlags.Full, count, bytes))
        {
            Flush(socket, ref writer);
            writer.TryStartState(id, EntStateFlags.Full, count, bytes);
        }

        foreach (var component in catalog.Components)
        {
            if (CanSend(component, owner) && component.Has(ent))
                writer.WritePresent(component, ent, resolver);
        }
    }

    private void WriteDiff(
        NetSocket socket,
        ref EntUpdateWriter writer,
        EntMutIdx ent,
        Guid id,
        bool owner)
    {
        if (!TryMeasureDiff(ent, owner, out ushort count, out int bytes))
            return;

        if (!writer.TryStartState(id, EntStateFlags.None, count, bytes))
        {
            Flush(socket, ref writer);
            writer.TryStartState(id, EntStateFlags.None, count, bytes);
        }

        var changed = ent.ScratchedComponents!;
        for (int page = 0; page < changed.Length; page++)
        {
            ulong bits = changed[page];
            while (bits != 0)
            {
                int bit = BitOperations.TrailingZeroCount(bits);
                var component = catalog[page * EntSyncCatalog.ComponentsPerMask + bit];
                bits &= bits - 1;

                if (!CanSend(component, owner))
                    continue;

                if (component.Has(ent))
                    writer.WritePresent(component, ent, resolver);
                else writer.WriteAbsent(component.Ordinal);
            }
        }
    }

    private void MeasureFull(EntMutIdx ent, bool owner, out ushort count, out int bytes)
    {
        count = 0;
        bytes = 0;
        foreach (var component in catalog.Components)
        {
            if (!CanSend(component, owner) || !component.Has(ent))
                continue;

            count++;
            bytes += sizeof(ushort) + component.Measure(ent);
        }
    }

    private bool TryMeasureDiff(EntMutIdx ent, bool owner, out ushort count, out int bytes)
    {
        count = 0;
        bytes = 0;
        var changed = ent.ScratchedComponents;
        if (changed == null)
            return false;

        for (int page = 0; page < changed.Length; page++)
        {
            ulong bits = changed[page];
            while (bits != 0)
            {
                int bit = BitOperations.TrailingZeroCount(bits);
                var component = catalog[page * EntSyncCatalog.ComponentsPerMask + bit];
                bits &= bits - 1;

                if (!CanSend(component, owner))
                    continue;

                count++;
                bytes += sizeof(ushort);
                if (component.Has(ent))
                    bytes += component.Measure(ent);
            }
        }
        return count != 0;
    }

    private void PublishBaselineEnts()
    {
        foreach (var ent in ents)
        {
            if (!ent.IsWorldSyncPublished && HasSyncedComponents(ent))
                Publish(ent);
        }
    }

    private void PublishScratchedEnts()
    {
        foreach (var ent in scratched)
        {
            if (!ent.IsWorldSyncPublished && HasSyncedComponents(ent))
                Publish(ent);
        }
    }

    private void Publish(EntMutIdx ent) => ent.IsWorldSyncPublished = true;

    private void Finish(EntMutIdx ent)
    {
        if (ent.IsWorldSyncPublished && !HasSyncedComponents(ent))
            ent.IsWorldSyncPublished = false;

        ent.IsScratched = false;
        var changed = ent.ScratchedComponents;
        if (changed != null)
            Array.Clear(changed);
    }

    private bool IsReplicated(EntMutIdx ent) => HasSyncedComponents(ent);

    private bool HasSyncedComponents(EntMutIdx ent)
    {
        foreach (var component in catalog.Components)
        {
            if (component.Has(ent))
                return true;
        }
        return false;
    }

    private void Flush(NetSocket socket, ref EntUpdateWriter writer)
    {
        if (writer.Length == 0)
            return;

        socket.Send(writer.Command(0), buffer.AsSpan(0, writer.Length));
        writer = new(buffer);
    }

    private bool IsOwner(NetSocket socket, EntMutIdx ent) => (Ent)socket.SocketWorldPlayer == ent;
    private bool CanSend(EntSyncComponent component, bool owner) =>
        component.Audience == EntSyncAudience.Everyone ||
        component.Audience == (owner ? EntSyncAudience.Owner : EntSyncAudience.Observers);
}
