#version 330 core

in vec3 fragLighting;
in vec3 fragTexCoord;

layout (location = 0) out vec4 outColor;

uniform sampler2DArray samplerTexture;
uniform float skyStrength;

float getBrightness(float level)
{
    return level / (4.0 - 3.0 * level);
}

vec3 notGamma(vec3 color)
{
    float maxComponent = max(max(color.x, color.y), color.z);
    if (maxComponent <= 0.0)
        return color;

    float maxInverted = 1.0 - maxComponent;
    float maxScaled = 1.0 - maxInverted * maxInverted * maxInverted * maxInverted;
    return color * (maxScaled / maxComponent);
}

float parabolicMixFactor(float level)
{
    float value = 2.0 * level - 1.0;
    return value * value;
}

void main()
{
    float skyLevel = clamp(fragLighting.y, 0.0, 1.0);
    float blockLevel = clamp(fragLighting.z, 0.0, 1.0);
    float sky = getBrightness(skyLevel) * skyStrength;
    float block = getBrightness(blockLevel) * 1.4;

    const vec3 ambientColor = vec3(10.0 / 255.0);
    const vec3 blockLightTint = vec3(1.0, 216.0 / 255.0, 140.0 / 255.0);
    vec3 blockColor = mix(blockLightTint, vec3(1.0), 0.9 * parabolicMixFactor(blockLevel));
    vec3 lightColor = ambientColor + vec3(sky) + blockColor * block;
    lightColor = clamp(lightColor, 0.0, 1.0);

    lightColor = mix(lightColor, notGamma(lightColor), 0.5);
    outColor = texture(samplerTexture, fragTexCoord) * vec4(lightColor * fragLighting.x, 1.0);
}
