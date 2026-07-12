#version 330 core

layout (location = 0) in vec3 inPosition;
layout (location = 1) in vec3 inLighting;
layout (location = 2) in vec3 inTexCoord;

out vec3 fragLighting;
out vec3 fragTexCoord;

uniform mat4 matView;
uniform mat4 matProjection;

uniform vec3 vecOffset;

void main()
{
    gl_Position = matProjection * matView * vec4(inPosition + vecOffset, 1.0);
    fragLighting = inLighting;
    fragTexCoord = inTexCoord;
}
