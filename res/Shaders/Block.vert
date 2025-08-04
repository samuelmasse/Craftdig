#version 330 core

layout (location = 0) in vec3 inPosition;
layout (location = 1) in vec3 inColor;
layout (location = 2) in vec3 inTexCoord;

out vec3 fragColor;
out vec3 fragTexCoord;

uniform mat4 matView;
uniform mat4 matProjection;

uniform vec3 vecOffset;

void main()
{
    gl_Position = vec4(inPosition + vecOffset, 1.0) * matView * matProjection;
    fragColor = inColor;
    fragTexCoord = inTexCoord;
}
