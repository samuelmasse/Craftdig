namespace AlvorEngine;

public interface ITextureProgram : IRenderProgram
{
    public TextureUnit SamplerTexture { get; }
}
