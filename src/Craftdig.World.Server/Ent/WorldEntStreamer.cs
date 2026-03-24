namespace Craftdig.World.Server;

[World]
public class WorldEntStreamer(AppLog log, WorldScratchedBag scratchedBag)
{
    private readonly List<EntMutIdx> scratched = [];

    public void Stream()
    {
        foreach (var ent in scratchedBag.Ents)
            scratched.Add(ent);

        foreach (var ent in scratched)
            Stream(ent);

        scratched.Clear();
    }

    private void Stream(EntMutIdx ent)
    {
        log.Info("Scratched {0}", ent);

        ent.IsScratched = false;
        if (ent.ScratchedComponents != null)
            Array.Clear(ent.ScratchedComponents);
    }
}
