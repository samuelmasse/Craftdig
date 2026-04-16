namespace Craftdig.Menus.Common;

[Module]
public class ModuleMainBackgroundMenu(AppStyle s)
{
    public void Create(EntMut root) => Node(root)
        .TextureV(s.BackgroundTexture)
        .TextureSubSizeRelativeV(s.TextureScale)
        .TextureOriginRelativeV((0.5f, 0.5f))
        .TextureAlignmentSnapV(1)
        .TextureColorV((0.7f, 0.7f, 0.7f, 1));
}
