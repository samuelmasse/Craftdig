namespace Crafthoe.Dimension;

[Dimension]
public class DimensionBlockProgram : RenderProgram3D<BlockVertex>
{
    private readonly int vecOffset;
    private readonly TextureUnit samplerTexture;

    public TextureUnit SamplerTexture => samplerTexture;

    public Vector3 Offset
    {
        set => GL.Uniform3(vecOffset, value);
    }

    public DimensionBlockProgram(RootGlw gl, AppFiles files) : base(
        gl, File.ReadAllText(files["Shaders/Block.vert"]), File.ReadAllText(files["Shaders/Block.frag"]))
    {
        vecOffset = GL.GetUniformLocation(Id, nameof(vecOffset));
        samplerTexture = TextureUnit.Texture0;
        GL.Uniform1(GL.GetUniformLocation(Id, nameof(samplerTexture)), (int)samplerTexture);
    }
}
