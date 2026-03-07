namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionEntSorter(DimensionRigidBag rigidBag, DimensionEntPersister entPersister)
{
    public void Tick()
    {
        foreach (var ent in rigidBag.Ents)
            Tick(ent);
    }

    private void Tick(EntMutIdx ent)
    {
        var rloc = ent.Position.ToLoc().Xy.ToCloc().ToRloc();
        if (ent.RigidRloc == rloc)
            return;

        ent.RigidPersistId++;
        entPersister.Schedule(ent, rloc, DateTime.MinValue);
        ent.RigidRloc = rloc;
    }
}
