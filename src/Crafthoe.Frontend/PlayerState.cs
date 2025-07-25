namespace Crafthoe.Frontend;

[Player]
public class PlayerState(RootBackbuffer backbuffer, RootSprites sprites, AppFont font) : State
{
    public override void Render() => backbuffer.Clear();

    public override void Draw()
    {
        sprites.Batch.Write(font.Value.Size(55), "Player View", (50, 50));
    }
}
