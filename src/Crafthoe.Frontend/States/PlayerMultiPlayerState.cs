namespace Crafthoe.Frontend;

[Player]
public class PlayerMultiPlayerState(
    WorldTick tick,
    DimensionChunks chunks,
    DimensionClientChunkReceiverHandler chunkReceiverHandler,
    PlayerEnt ent,
    PlayerContext player,
    PlayerCommonState commonState,
    PlayerMultiPlayerDisconnectAction multiPlayerDisconnectAction,
    PlayerChunkUpdateQueue chunkUpdateQueue,
    PlayerPositionUpdateReceiver positionUpdateReceiver) : State
{
    public override void Load()
    {
        commonState.Load();
    }

    public override void Unload()
    {
        commonState.Unload();
        multiPlayerDisconnectAction.Run();
    }

    public override void Update(double time)
    {
        commonState.Update(time);

        int ticks = tick.Update(time);
        while (ticks > 0)
        {
            if (!commonState.Inv)
                player.Tick();

            ent.Ent.PrevPosition() = ent.Ent.Position();
            ent.Ent.Position() = positionUpdateReceiver.Latest;

            ticks--;
        }

        if (!commonState.Inv)
            player.Update(time);
    }

    public override void Render()
    {
        int count = chunkUpdateQueue.Count;
        while (count > 0 && chunkUpdateQueue.TryDequeue(out var item))
        {
            var (cloc, blocks) = item;

            chunks.Alloc(cloc);
            var chunk = chunks[cloc];
            chunk.Blocks() = blocks;
            chunkReceiverHandler.Handle(chunk);
        }

        commonState.Render();
    }

    public override void Draw()
    {
        commonState.Draw();
    }
}
