namespace Craftdig;

[Dimension]
public class DimensionSelected(DimensionBlocks blocks, DimensionPlayerBag playerBag)
{
    private long time;

    public void Tick()
    {
        foreach (var player in playerBag.Ents)
            if (!player.IsRemote)
                Tick(player);

        time++;
    }

    private void Tick(EntMutIdx player)
    {
        player.BlockSelectionPosition = player.Position;
        player.BlockSelection = null;
    }

    public BlockSelection? this[EntMutIdx player]
    {
        get
        {
            if (player.BlockSelectionLastComputed != time)
            {
                player.BlockSelection = Select(player.BlockSelectionPosition, player.LookAt);
                player.BlockSelectionLastComputed = time;
            }

            return player.BlockSelection;
        }
    }

    public BlockSelection? Select(Vec3d origin, Vec3d lookAt)
    {
        Vec3i dir = (Math.Sign(lookAt.X), Math.Sign(lookAt.Y), Math.Sign(lookAt.Z));
        Vec3d dt = Vec3d.Abs((1 / lookAt.X, 1 / lookAt.Y, 1 / lookAt.Z));

        Vec3i nloc = ((int)Math.Floor(origin.X), (int)Math.Floor(origin.Y), (int)Math.Floor(origin.Z));

        Vec3d ni = new(
            dir.X > 0 ? (nloc.X + 1 - origin.X) * dt.X : (origin.X - nloc.X) * dt.X,
            dir.Y > 0 ? (nloc.Y + 1 - origin.Y) * dt.Y : (origin.Y - nloc.Y) * dt.Y,
            dir.Z > 0 ? (nloc.Z + 1 - origin.Z) * dt.Z : (origin.Z - nloc.Z) * dt.Z
        );

        Vec3i nnormal = default;

        int step = 0;
        bool found = false;

        while (step < 1024)
        {
            if (blocks.TryGet(nloc, out var block) && block.IsSolid)
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
