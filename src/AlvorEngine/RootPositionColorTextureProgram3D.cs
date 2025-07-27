namespace AlvorEngine;

[Root]
public class RootPositionColorTextureProgram3D : RenderProgram3D<PositionColorTextureVertex>, ITextureProgram
{
    internal const string Vert =
        """
        #version 330 core

        layout (location = 0) in vec3 inPosition;
        layout (location = 1) in vec3 inColor;
        layout (location = 2) in vec2 inTexCoord;

        out vec3 fragColor;
        out vec2 fragTexCoord;

        uniform mat4 matView;
        uniform mat4 matProjection;

        void main()
        {
            gl_Position = vec4(inPosition, 1.0) * matView * matProjection;
            fragColor = inColor;
            fragTexCoord = inTexCoord;
        }
        """;

    private readonly TextureUnit samplerTexture;

    public TextureUnit SamplerTexture => samplerTexture;

    public RootPositionColorTextureProgram3D(RootGlw gl) : base(gl, Vert, RootPositionColorTextureProgram.Frag)
    {
        samplerTexture = TextureUnit.Texture0;
        GL.Uniform1(GL.GetUniformLocation(Id, nameof(samplerTexture)), (int)samplerTexture);
    }
}
