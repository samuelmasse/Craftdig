namespace Crafthoe.Dimension;

[Dimension]
public class DimensionRigids(DimensionRigidBag bag)
{
    public void Tick()
    {
        foreach (var ent in bag.Ents)
        {
            double d = ent.Velocity().Z;
            ent.PrevPosition() = ent.Position();
            ent.Position() += ent.Velocity();
            ent.Velocity() *= (0.91f, 0.91f, 0.98f);

            // TODO: Only if flying
            ent.Velocity().Z = d * 0.6f;
        }
    }
}
