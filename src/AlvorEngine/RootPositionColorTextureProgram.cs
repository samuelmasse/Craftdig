namespace AlvorEngine;

[Root]
public class RootPositionColorTextureProgram : RenderProgram<PositionColorTextureVertex>, ITextureProgram
{
    internal const string Vert =
        """
        #version 330 core

        layout (location = 0) in vec3 inPosition;
        layout (location = 1) in vec3 inColor;
        layout (location = 2) in vec2 inTexCoord;

        out vec3 fragColor;
        out vec2 fragTexCoord;

        void main()
        {
            gl_Position = vec4(inPosition, 1.0);
            fragColor = inColor;
            fragTexCoord = inTexCoord;
        }
        """;

    internal const string Frag =
        """
        #version 330 core

        in vec3 fragColor;
        in vec2 fragTexCoord;

        layout (location = 0) out vec4 outColor;

        uniform sampler2D samplerTexture;

        void main()
        {
            outColor = texture(samplerTexture, fragTexCoord) * vec4(fragColor, 1.0);
        }
        """;

    private readonly TextureUnit samplerTexture;

    public TextureUnit SamplerTexture => samplerTexture;

    public RootPositionColorTextureProgram(RootGlw gl) : base(gl, Vert, Frag)
    {
        samplerTexture = TextureUnit.Texture0;
        GL.Uniform1(GL.GetUniformLocation(Id, nameof(samplerTexture)), (int)samplerTexture);
    }
}
