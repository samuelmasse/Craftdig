namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionEntStreamer(
    WorldEntSyncResolver resolver,
    DimensionSyncScope syncScope,
    DimensionEntSyncCatalog catalog,
    DimensionScratchedBag scratchedBag,
    DimensionEntDisposals disposals,
    DimensionSyncChunks syncChunks,
    DimensionSockets sockets)
{
    private readonly byte[] buffer = new byte[
        ProtocolLimits.MaxMessageSize - Unsafe.SizeOf<EntUpdateCommand>()];
    private readonly List<EntMutIdx> scratched = [];
    private readonly List<DimensionEntDisposal> disposed = [];

    public void Stream()
    {
        foreach (var ent in scratchedBag.Ents)
            scratched.Add(ent);

        while (disposals.TryTake(out var entry))
        {
            if (entry.Id != Guid.Empty)
                disposed.Add(entry);
        }

        foreach (var socket in sockets.Span)
        {
            if (socket.IsWorldSyncReady)
                Stream(socket);
        }

        foreach (var ent in scratched)
            Finish(ent);

        scratched.Clear();
        disposed.Clear();
    }

    public void EnterChunk(NetSocket socket, Vec2i cloc)
    {
        if (!syncChunks.TryGet(cloc, out var ents))
            return;

        var writer = new EntUpdateWriter(buffer);
        foreach (var ent in ents)
        {
            if (IsOwner(socket, ent) || !TryMeasureFull(ent, false, out _, out _))
                continue;

            Guid id = ent.Id;
            if (!writer.TryWriteCreate(id))
            {
                Flush(socket, ref writer);
                writer.TryWriteCreate(id);
            }
        }
        Flush(socket, ref writer);

        writer = new(buffer);
        foreach (var ent in ents)
        {
            if (IsOwner(socket, ent) || !TryMeasureFull(ent, false, out ushort count, out int bytes))
                continue;

            WriteFull(socket, ref writer, ent, ent.Id, false, count, bytes);
        }
        Flush(socket, ref writer);
    }

    public void LeaveChunk(NetSocket socket, Vec2i cloc)
    {
        if (!syncChunks.TryGet(cloc, out var ents))
            return;

        var writer = new EntUpdateWriter(buffer);
        foreach (var ent in ents)
        {
            if (IsOwner(socket, ent))
                continue;

            if (!writer.TryWriteDelete(ent.Id))
            {
                Flush(socket, ref writer);
                writer.TryWriteDelete(ent.Id);
            }
        }
        Flush(socket, ref writer);
    }

    private void Stream(NetSocket socket)
    {
        WriteCreates(socket);
        WriteStates(socket);
        WriteDeletes(socket);
    }

    private void WriteCreates(NetSocket socket)
    {
        var writer = new EntUpdateWriter(buffer);
        foreach (var ent in scratched)
        {
            bool owner = IsOwner(socket, ent);
            if (WasVisible(socket, ent) || !IsVisible(socket, ent) ||
                !TryMeasureFull(ent, owner, out _, out _))
                continue;

            Guid id = ent.Id;
            var flags = owner ? EntCreateFlags.Owner : EntCreateFlags.None;
            if (!writer.TryWriteCreate(id, flags))
            {
                Flush(socket, ref writer);
                writer.TryWriteCreate(id, flags);
            }
        }
        Flush(socket, ref writer);
    }

    private void WriteStates(NetSocket socket)
    {
        var writer = new EntUpdateWriter(buffer);
        foreach (var ent in scratched)
        {
            bool owner = IsOwner(socket, ent);
            bool wasVisible = WasVisible(socket, ent);
            bool isVisible = IsVisible(socket, ent);

            if (!isVisible)
                continue;

            Guid id = ent.Id;
            if (!wasVisible)
            {
                if (TryMeasureFull(ent, owner, out ushort count, out int bytes))
                    WriteFull(socket, ref writer, ent, id, owner, count, bytes);
            }
            else if (TryMeasureDiff(ent, owner, out ushort count, out int bytes))
                WriteDiff(socket, ref writer, ent, id, owner, count, bytes);
        }
        Flush(socket, ref writer);
    }

    private void WriteDeletes(NetSocket socket)
    {
        var writer = new EntUpdateWriter(buffer);
        foreach (var ent in scratched)
        {
            if (!WasVisible(socket, ent) || IsVisible(socket, ent))
                continue;

            WriteDelete(socket, ref writer, ent.Id);
        }

        var streamed = socket.SocketStreamedChunks;
        foreach (var entry in disposed)
        {
            if (entry.Owner == socket ||
                entry.Cloc != null && streamed?.Contains(entry.Cloc.Value) == true)
                WriteDelete(socket, ref writer, entry.Id);
        }
        Flush(socket, ref writer);
    }

    private void WriteFull(
        NetSocket socket,
        ref EntUpdateWriter writer,
        EntMutIdx ent,
        Guid id,
        bool owner,
        ushort count,
        int bytes)
    {
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
        bool owner,
        ushort count,
        int bytes)
    {
        if (!writer.TryStartState(id, EntStateFlags.None, count, bytes))
        {
            Flush(socket, ref writer);
            writer.TryStartState(id, EntStateFlags.None, count, bytes);
        }

        var changed = ent.ScratchedComponents;
        if (changed == null)
            return;

        for (int page = 0; page < changed.Length; page++)
        {
            ulong bits = changed[page];
            while (bits != 0)
            {
                int bit = BitOperations.TrailingZeroCount(bits);
                int ordinal = page * EntSyncCatalog.ComponentsPerMask + bit;
                bits &= bits - 1;

                if (ordinal >= catalog.Components.Length)
                    continue;

                var component = catalog[ordinal];
                if (!CanSend(component, owner))
                    continue;

                if (component.Has(ent))
                    writer.WritePresent(component, ent, resolver);
                else writer.WriteAbsent(component.Ordinal);
            }
        }
    }

    private void WriteDelete(NetSocket socket, ref EntUpdateWriter writer, Guid id)
    {
        if (!writer.TryWriteDelete(id))
        {
            Flush(socket, ref writer);
            writer.TryWriteDelete(id);
        }
    }

    private bool TryMeasureFull(EntMutIdx ent, bool owner, out ushort count, out int bytes)
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
        return count != 0;
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
                int ordinal = page * EntSyncCatalog.ComponentsPerMask + bit;
                bits &= bits - 1;

                if (ordinal >= catalog.Components.Length)
                    continue;

                var component = catalog[ordinal];
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

    private bool WasVisible(NetSocket socket, EntMutIdx ent)
    {
        var cloc = ent.SyncCloc;
        if (cloc == null)
            return false;
        if (IsOwner(socket, ent))
            return true;

        return socket.SocketStreamedChunks?.Contains(cloc.Value) == true;
    }

    private bool IsVisible(NetSocket socket, EntMutIdx ent)
    {
        bool owner = IsOwner(socket, ent);
        if (!ent.IsLoaded ||
            !ent.Has<Vec3d, DimensionComponents.Position>() ||
            !HasSyncedComponents(ent, owner))
            return false;
        if (owner)
            return true;

        return socket.SocketStreamedChunks?.Contains(Cloc(ent)) == true;
    }

    private void Finish(EntMutIdx ent)
    {
        var previous = ent.SyncCloc;
        if (ent.IsLoaded &&
            ent.Has<Vec3d, DimensionComponents.Position>() &&
            HasSyncedComponents(ent, false))
        {
            var current = Cloc(ent);
            syncChunks.Move(ent, previous, current);
            ent.SyncCloc = current;
        }
        else
        {
            syncChunks.Remove(ent, previous);
            ent.SyncCloc = null;
        }

        ent.IsScratched = false;
        var changed = ent.ScratchedComponents;
        if (changed != null)
            Array.Clear(changed);
    }

    private bool HasSyncedComponents(EntMutIdx ent, bool owner)
    {
        foreach (var component in catalog.Components)
            if (CanSend(component, owner) && component.Has(ent))
                return true;
        return false;
    }

    private void Flush(NetSocket socket, ref EntUpdateWriter writer)
    {
        if (writer.Length == 0)
            return;

        socket.Send(writer.Command(syncScope.Id), buffer.AsSpan(0, writer.Length));
        writer = new(buffer);
    }

    private bool IsOwner(NetSocket socket, EntMutIdx ent) => (Ent)socket.SocketPlayer == ent;
    private bool CanSend(EntSyncComponent component, bool owner) =>
        component.Audience == EntSyncAudience.Everyone ||
        (owner ? component.Audience == EntSyncAudience.Owner : component.Audience == EntSyncAudience.Observers);
    private Vec2i Cloc(EntMutIdx ent) => ent.Position.ToLoc().Xy.ToCloc();
}
