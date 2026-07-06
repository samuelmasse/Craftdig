namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionBlockProgram : RenderProgram3D<BlockVertex>
{
    private readonly int vecOffset;
    private readonly GlTextureUnit samplerTexture;

    public GlTextureUnit SamplerTexture => samplerTexture;

    public Vec3 Offset
    {
        set => gl.ProgramUniform3f(Id, vecOffset, value.X, value.Y, value.Z);
    }

    public DimensionBlockProgram(RootGl gl, AppFiles files) : base(
        gl, File.ReadAllText(files["Shaders/Block.vert"]), File.ReadAllText(files["Shaders/Block.frag"]))
    {
        vecOffset = gl.GetUniformLocation(Id, nameof(vecOffset));
        samplerTexture = GlTextureUnit.Texture0;
        gl.ProgramUniform1i(Id, gl.GetUniformLocation(Id, nameof(samplerTexture)), (int)samplerTexture);
    }
}
