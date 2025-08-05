namespace Crafthoe.Player;

[Player]
public class PlayerSelected(DimensionBlocks blocks, PlayerEntity player, PlayerCamera camera)
{
    private Vector3i? loc;
    private Vector3i? normal;
    private int distance = 1024;

    public Vector3i? Loc => loc;
    public Vector3i? Normal => normal;
    public ref int Distance => ref distance;

    public void Render()
    {
        Vector3d origin = player.Entity.Position();
        Vector3d lookAt = (camera.LookAt.X, camera.LookAt.Z, camera.LookAt.Y);

        Vector3i dir = (Math.Sign(lookAt.X), Math.Sign(lookAt.Y), Math.Sign(lookAt.Z));
        Vector3d dt = Vector3d.Abs((1 / lookAt.X, 1 / lookAt.Y, 1 / lookAt.Z));

        Vector3i nloc = ((int)Math.Floor(origin.X), (int)Math.Floor(origin.Y), (int)Math.Floor(origin.Z));

        Vector3d ni = new(
            dir.X > 0 ? (nloc.X + 1 - origin.X) * dt.X : (origin.X - nloc.X) * dt.X,
            dir.Y > 0 ? (nloc.Y + 1 - origin.Y) * dt.Y : (origin.Y - nloc.Y) * dt.Y,
            dir.Z > 0 ? (nloc.Z + 1 - origin.Z) * dt.Z : (origin.Z - nloc.Z) * dt.Z
        );

        Vector3i nnormal = default;

        int step = 0;
        bool found = false;

        while (step < distance)
        {
            if (blocks.TryGet(nloc, out var block) && block.IsSolid())
            {
                found = true;
                break;
            }

            if (ni.X < ni.Y && ni.X < ni.Z)
            {
                ni.X += dt.X;
                nloc.X += dir.X;
                nnormal = (-dir.X, 0, 0);
            }
            else if (ni.Y < ni.Z)
            {
                ni.Y += dt.Y;
                nloc.Y += dir.Y;
                nnormal = (0, -dir.Y, 0);
            }
            else
            {
                ni.Z += dt.Z;
                nloc.Z += dir.Z;
                nnormal = (0, 0, -dir.Z);
            }

            step++;
        }

        if (found)
        {
            loc = nloc;
            normal = nnormal;
        }
        else
        {
            loc = null;
            normal = null;
        }
    }
}
