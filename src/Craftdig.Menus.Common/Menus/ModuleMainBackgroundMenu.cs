namespace Craftdig.Menus.Common;

[Module]
public class ModuleMainBackgroundMenu
{
    public void Create(EntMut root) => Node(root).ColorV((0.4f, 0.2f, 0.2f, 1));
}
