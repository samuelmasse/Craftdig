namespace Craftdig.Dimension;

[Dimension]
public class DimensionSelected(DimensionBlocks blocks, DimensionPlayerBag playerBag)
{
    private long time;

    public void Tick()
    {
        foreach (var player in playerBag.Ents)
        {
            player.BlockSelectionPosition() = player.Position();
            player.BlockSelectionLookAt() = player.Movement().LookAt;
            player.BlockSelection() = null;
        }

        time++;
    }

    public BlockSelection? this[EntMut player]
    {
        get
        {
            if (player.BlockSelectionLastComputed() != time)
            {
                player.BlockSelection() = Select(player.BlockSelectionPosition(), player.BlockSelectionLookAt());
                player.BlockSelectionLastComputed() = time;
            }

            return player.BlockSelection();
        }
    }

    public BlockSelection? Select(Vector3d origin, Vector3d lookAt)
    {
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

        while (step < 1024)
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
            return new(nloc, nnormal);
        else return null;
    }
}
