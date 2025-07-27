namespace AlvorEngine;

[Root]
public class RootPositionColorProgram3D(RootGlw gl) :
    RenderProgram3D<PositionColorVertex>(gl, Vert, RootPositionColorProgram.Frag)
{
    internal const string Vert =
        """
        #version 330 core

        layout (location = 0) in vec3 inPosition;
        layout (location = 1) in vec3 inColor;

        out vec3 fragColor;

        uniform mat4 matView;
        uniform mat4 matProjection;

        void main()
        {
            gl_Position = vec4(inPosition, 1.0) * matView * matProjection;
            fragColor = inColor;
        }
        """;
}
