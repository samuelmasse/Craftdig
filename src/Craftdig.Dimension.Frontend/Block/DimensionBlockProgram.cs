namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionBlockProgram : RenderProgram3D<BlockVertex>
{
    private readonly int vecOffset;
    private readonly TextureUnit samplerTexture;

    public TextureUnit SamplerTexture => samplerTexture;

    public Vector3 Offset
    {
        set => gl.Uniform3(vecOffset, value);
    }

    public DimensionBlockProgram(RootGlw gl, AppFiles files) : base(
        gl, File.ReadAllText(files["Shaders/Block.vert"]), File.ReadAllText(files["Shaders/Block.frag"]))
    {
        vecOffset = gl.GetUniformLocation(Id, nameof(vecOffset));
        samplerTexture = TextureUnit.Texture0;
        gl.Uniform1(gl.GetUniformLocation(Id, nameof(samplerTexture)), (int)samplerTexture);
    }
}
