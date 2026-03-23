namespace Craftdig.Server;

[Server]
public class ServerEntScratched
{
    public void Mark(EntMutIdx ent, int index)
    {
        if (ent.IsLoading)
            return;

        int page = index / 64;
        int sub = index % 64;

        var scratched = ent.ScratchedComponents ??= new ulong[1];
        if (page >= scratched.Length)
        {
            var next = new ulong[page + 1];
            scratched.CopyTo(next);
            scratched = next;
            ent.ScratchedComponents = scratched;
        }

        scratched[page] |= 1UL << sub;
        ent.IsScratched = true;
    }
}
