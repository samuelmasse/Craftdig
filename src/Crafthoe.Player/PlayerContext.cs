namespace Crafthoe.Player;

[Player]
public class PlayerContext(RootCanvas canvas, AppFiles files, PlayerGlw gl)
{
    private int shaderProgram;
    private int vao;

    public void Load()
    {
        using var vert = new ShaderStage(gl, File.ReadAllText(files["Shaders/PositionColor.vert"]), ShaderType.VertexShader);
        using var frag = new ShaderStage(gl, File.ReadAllText(files["Shaders/PositionColor.frag"]), ShaderType.FragmentShader);
        var program = new ShaderProgram(gl, [vert, frag]);

        shaderProgram = program.Id;

        float[] vertices = [
            0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f,
            -0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 0.0f,
            0.0f, 0.5f, 0.0f, 0.0f, 0.0f, 1.0f
        ];

        vao = gl.GenVertexArray();
        int vbo = gl.GenBuffer();

        gl.BindVertexArray(vao);
        gl.BindBuffer(BufferTarget.ArrayBuffer, vbo);

        gl.BufferData<float>(BufferTarget.ArrayBuffer, vertices, BufferUsageHint.StaticDraw);

        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        gl.EnableVertexAttribArray(0);

        gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        gl.EnableVertexAttribArray(1);

        gl.UnbindVertexArray();
        gl.UnbindBuffer(BufferTarget.ArrayBuffer);
    }

    public void Render()
    {
        gl.Viewport(canvas.Size);

        gl.UseProgram(shaderProgram);
        gl.BindVertexArray(vao);

        gl.DrawArrays(PrimitiveType.Triangles, 0, 3);

        gl.UnbindVertexArray();
        gl.UnuseProgram();

        gl.ResetViewport();
    }
}
