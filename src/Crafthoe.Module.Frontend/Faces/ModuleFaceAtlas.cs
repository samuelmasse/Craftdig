namespace Crafthoe.Module.Frontend;

[Module]
public class ModuleFaceAtlas(ModuleGlw gl, ModuleImages imgs)
{
    private readonly Texture texture = new(gl, (0xF, 0xF), TextureTarget.Texture2DArray)
    {
        MagFilter = TextureMagFilter.Nearest,
        MinFilter = TextureMinFilter.NearestMipmapLinear,
        WrapS = TextureWrapMode.Repeat,
        WrapT = TextureWrapMode.Repeat
    };
    private readonly Dictionary<string, int> indices = [];
    private readonly List<ImageData> images = [new((0xF, 0xF), new (byte, byte, byte, byte)[0xFF])];
    private int lastCount;
    private int textureDepth;

    public int this[string file]
    {
        get
        {
            if (indices.TryGetValue(file, out int val))
                return val;

            images.Add(imgs[file]);
            indices.Add(file, images.Count - 1);

            return images.Count - 1;
        }
    }

    public void Bind(TextureUnit unit)
    {
        if (lastCount != images.Count)
            BuildAtlas();

        texture.Bind(unit);
    }

    public void Unbind(TextureUnit unit) => texture.Unbind(unit);

    private void BuildAtlas()
    {
        int start = lastCount;
        if (textureDepth <= images.Count)
        {
            ResizeTexture(MathHelper.NextPowerOfTwo(images.Count + 1));
            start = 0;
        }

        for (int i = start; i < images.Count; i++)
            WriteImage(i);

        texture.GenerateMipmap();

        lastCount = images.Count;
    }

    private void WriteImage(int index)
    {
        var image = images[index];
        var pixels = new (byte, byte, byte, byte)[image.Pixels.Length];
        image.Pixels.CopyTo(pixels);

        texture.Bind();
        gl.ActiveTexture(TextureUnit.Texture0);

        GL.TexSubImage3D(
            TextureTarget.Texture2DArray,
            0,
            0,
            0,
            index,
            (int)texture.Size.X,
            (int)texture.Size.Y,
            1,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            pixels);

        gl.ResetActiveTexture();
        texture.Unbind();
    }

    private void ResizeTexture(int depth)
    {
        texture.Bind();
        gl.ActiveTexture(TextureUnit.Texture0);

        gl.TexImage3D(TextureTarget.Texture2DArray,
            0,
            PixelInternalFormat.Rgba,
            (int)texture.Size.X,
            (int)texture.Size.Y,
            depth,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            0,
            sizeof(byte),
            4);

        gl.ResetActiveTexture();
        texture.Unbind();

        textureDepth = depth;
    }
}
