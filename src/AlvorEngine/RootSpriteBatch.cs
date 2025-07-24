namespace AlvorEngine;

[Root]
public class RootSprites(SpriteBatch spriteBatch)
{
    public SpriteBatchWriter Batch => spriteBatch.Writer;

    internal void Begin(Vector2 size) => spriteBatch.Begin(size);
    internal void End() => spriteBatch.End();
}
