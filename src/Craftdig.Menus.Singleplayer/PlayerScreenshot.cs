namespace Craftdig.Menus.Singleplayer;

[Player]
public class PlayerScreenshot(WorldPaths paths, PlayerGl gl, PlayerRenderer renderer)
{
    public void Run()
    {
        var size = new Vec2u(512, 512);
        var (width, height) = ((int)size.X, (int)size.Y);
        var pixels = new Vec4u8[width * height];

        var framebuffer = gl.GenFramebuffer();
        gl.BindFramebuffer(GlFramebufferTarget.Framebuffer, framebuffer);

        var texture = new Texture2D(gl, size)
        {
            Pixels = pixels,
            MinFilter = GlTextureMinFilter.Linear,
            MagFilter = GlTextureMagFilter.Linear
        };
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, texture.Id);
        gl.FramebufferTexture2D(GlFramebufferTarget.Framebuffer, GlFramebufferAttachment.ColorAttachment0, GlTextureTarget.Texture2D, texture.Id, 0);
        gl.UnbindTexture(GlTextureTarget.Texture2D);
        gl.ResetActiveTexture();

        var depth = gl.GenRenderbuffer();
        gl.BindRenderbuffer(GlRenderbufferTarget.Renderbuffer, depth);
        gl.RenderbufferStorage(GlRenderbufferTarget.Renderbuffer, GlInternalFormat.DepthComponent24, size);
        gl.FramebufferRenderbuffer(GlFramebufferTarget.Framebuffer, GlFramebufferAttachment.DepthAttachment, GlRenderbufferTarget.Renderbuffer, depth);
        gl.UnbindRenderbuffer(GlRenderbufferTarget.Renderbuffer);

        gl.DrawBuffers([GlDrawBufferMode.ColorAttachment0]);
        renderer.Render(size);
        gl.ResetDrawBuffers();

        gl.PixelStorei(GlPixelStoreParameter.PackAlignment, 1);
        gl.ReadBuffer(GlReadBufferMode.ColorAttachment0);
        gl.ReadPixels(size, GlPixelFormat.Rgba, GlPixelType.UnsignedByte, pixels.AsSpan());
        gl.ResetReadBuffer();
        gl.ResetPixelStore(GlPixelStoreParameter.PackAlignment);

        gl.UnbindFramebuffer(GlFramebufferTarget.Framebuffer);

        gl.DeleteFramebuffer(framebuffer);
        gl.DeleteRenderbuffer(depth);
        texture.Dispose();

        var png = PngBuilder.Create(width, height, true);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var (r, g, b, a) = pixels[y * width + x];
                png.SetPixel(new(r, g, b, a, false), x, height - 1 - y);
            }
        }

        File.WriteAllBytes(Path.Join(paths.Root, "Screenshot.png"), png.Save());
    }
}
