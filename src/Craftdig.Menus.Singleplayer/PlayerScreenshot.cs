namespace Craftdig.Menus.Singleplayer;

[Player]
public class PlayerScreenshot(WorldPaths paths, PlayerGlw gl, PlayerRenderer renderer)
{
    public void Run()
    {
        var size = new Vector2i(512, 512);
        var pixels = new (byte, byte, byte, byte)[size.X * size.Y];

        int framebuffer = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);

        var texture = new Texture2D(gl, size)
        {
            Pixels = pixels,
            MinFilter = TextureMinFilter.Linear,
            MagFilter = TextureMagFilter.Linear
        };
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(TextureTarget.Texture2D, texture.Id);
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texture.Id, 0);
        gl.UnbindTexture(TextureTarget.Texture2D);
        gl.ResetActiveTexture();

        int depth = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depth);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, size.X, size.Y);
        gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depth);
        gl.UnbindRenderbuffer(RenderbufferTarget.Renderbuffer);

        gl.DrawBuffers(DrawBuffersEnum.ColorAttachment0);
        renderer.Render(size);
        gl.ResetDrawBuffers();

        gl.PixelStore(PixelStoreParameter.PackAlignment, 1);
        gl.ReadBuffer(ReadBufferMode.ColorAttachment0);
        gl.ReadPixels(0, 0, size.X, size.Y, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
        gl.ResetReadBuffer();
        gl.ResetPixelStore(PixelStoreParameter.PackAlignment);

        gl.UnbindFramebuffer(FramebufferTarget.Framebuffer);

        gl.DeleteFramebuffer(framebuffer);
        gl.DeleteRenderbuffer(depth);
        texture.Dispose();

        var png = PngBuilder.Create(size.X, size.Y, true);
        for (int y = 0; y < size.Y; y++)
        {
            for (int x = 0; x < size.X; x++)
            {
                var (r, g, b, a) = pixels[y * size.X + x];
                png.SetPixel(new(r, g, b, a, false), x, size.Y - 1 - y);
            }
        }

        File.WriteAllBytes(Path.Join(paths.Root, "Screenshot.png"), png.Save());
    }
}
