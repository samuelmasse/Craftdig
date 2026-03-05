namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionEntitySorter(DimensionRigidBag rigidBag, DimensionEntityPersister entityPersister)
{
    public void Tick()
    {
        foreach (var ent in rigidBag.Ents)
            Tick(ent);
    }

    private void Tick(EntRefMut ent)
    {
        var rloc = ent.Position.ToLoc().Xy.ToCloc().ToRloc();
        if (ent.RigidRloc == rloc)
            return;

        ent.RigidPersistId++;
        entityPersister.Schedule(ent, rloc, DateTime.MinValue);
        ent.RigidRloc = rloc;
    }
}
