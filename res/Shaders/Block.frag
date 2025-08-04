#version 330 core

in vec3 fragColor;
in vec2 fragTexCoord;

layout (location = 0) out vec4 outColor;

uniform sampler2DArray samplerTexture;

void main()
{
    outColor = texture(samplerTexture, vec3(fragTexCoord, 0.0)) * vec4(fragColor, 1.0);
}
