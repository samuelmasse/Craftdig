namespace Craftdig.Menus.Common;

[Module]
public class ModuleMainBackgroundMenu(AppStyle s)
{
    public void Create(EntMut root) => Node(root)
        .TextureV(s.BackgroundTexture)
        .TextureSubSizeRelativeV(s.TextureScale)
        .TextureColorV((0.7f, 0.7f, 0.7f, 1));
}
