namespace AlvorEngine;

[Root]
public class RootQuadIndexBuffer(RootGlw gl)
{
    private readonly int id = gl.GenBuffer(nameof(RootQuadIndexBuffer));
    private int capacity;

    public int Id => id;
    public int Capacity => capacity;

    public void EnsureCapacity(int quadVertexCount)
    {
        if (quadVertexCount < capacity)
            return;

        int newCapacity = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)quadVertexCount + 1);
        int indexCount = newCapacity / 4 * 6;
        var indexValues = new uint[indexCount];
        uint vertexIndex = 0;

        for (int i = 0; i < indexCount; i += 6)
        {
            indexValues[i + 0] = vertexIndex + 2;
            indexValues[i + 1] = vertexIndex + 3;
            indexValues[i + 2] = vertexIndex + 1;
            indexValues[i + 3] = vertexIndex + 1;
            indexValues[i + 4] = vertexIndex + 0;
            indexValues[i + 5] = vertexIndex + 2;

            vertexIndex += 4;
        }

        gl.BindBuffer(BufferTarget.ElementArrayBuffer, id);
        gl.BufferData(
            BufferTarget.ElementArrayBuffer,
            indexValues.AsSpan(),
            BufferUsageHint.StaticDraw);
        gl.UnbindBuffer(BufferTarget.ElementArrayBuffer);

        capacity = newCapacity;
    }
}
