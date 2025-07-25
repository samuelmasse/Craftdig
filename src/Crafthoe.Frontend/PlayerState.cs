namespace Crafthoe.Frontend;

[Player]
public class PlayerState(RootBackbuffer backbuffer, RootSprites sprites, AppFont font, PlayerContext context) : State
{
    public override void Load()
    {
        context.Load();
    }

    public override void Update(double time)
    {
        context.Update(time);
    }

    public override void Render()
    {
        backbuffer.Clear();
        context.Render();
    }

    public override void Draw()
    {
        sprites.Batch.Write(font.Value.Size(55), "Player View", (50, 50));
    }
}
