namespace AlvorEngine;

public abstract class RenderProgram<T> : IRenderProgram where T : IVertex
{
    private readonly Glw gl;
    private readonly ShaderProgram program;

    public int Id => program.Id;

    public RenderProgram(Glw gl, string vertCode, string fragCode)
    {
        this.gl = gl;
        using var vert = new ShaderStage(gl, vertCode, ShaderType.VertexShader);
        using var frag = new ShaderStage(gl, fragCode, ShaderType.FragmentShader);
        program = new ShaderProgram(gl, [vert, frag]);
    }

    public void SetAttributes() => T.SetAttributes(gl);
}
