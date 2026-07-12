namespace Craftdig.Module.Frontend;

[Module]
public class ModuleFaceAtlas(ModuleGl gl, ModuleImages imgs)
{
    private readonly Texture texture = new(gl, (0x10, 0x10), GlTextureTarget.Texture2DArray)
    {
        MagFilter = GlTextureMagFilter.Nearest,
        MinFilter = GlTextureMinFilter.NearestMipmapLinear,
        WrapS = GlTextureWrapMode.Repeat,
        WrapT = GlTextureWrapMode.Repeat
    };
    private readonly Dictionary<string, int> indices = [];
    private readonly List<ImageData> images = [new((0x10, 0x10), new Vec4u8[0x100])];
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

    public void Bind(GlTextureUnit unit)
    {
        if (lastCount != images.Count)
            BuildAtlas();

        texture.Bind(unit);
    }

    public void Unbind(GlTextureUnit unit) => texture.Unbind(unit);

    private void BuildAtlas()
    {
        int start = lastCount;
        if (textureDepth <= images.Count)
        {
            ResizeTexture((int)BitOperations.RoundUpToPowerOf2((uint)(images.Count + 1)));
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
        var pixels = new Vec4u8[image.Pixels.Length];
        image.Pixels.CopyTo(pixels);

        texture.Bind();
        gl.ActiveTexture(GlTextureUnit.Texture0);

        gl.TexSubImage3D(
            GlTextureTarget.Texture2DArray,
            0,
            0,
            0,
            index,
            (int)texture.Size.X,
            (int)texture.Size.Y,
            1,
            GlPixelFormat.Rgba,
            GlPixelType.UnsignedByte,
            pixels);

        gl.ResetActiveTexture();
        texture.Unbind();
    }

    private void ResizeTexture(int depth)
    {
        texture.Bind();
        gl.ActiveTexture(GlTextureUnit.Texture0);

        gl.TexImage3D(GlTextureTarget.Texture2DArray,
            0,
            GlInternalFormat.Rgba,
            (int)texture.Size.X,
            (int)texture.Size.Y,
            depth,
            0,
            GlPixelFormat.Rgba,
            GlPixelType.UnsignedByte,
            0);

        gl.ResetActiveTexture();
        texture.Unbind();

        textureDepth = depth;
    }
}
