namespace Craftdig.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerDeleteServerMenu(
    AppStyle s,
    ModuleMultiplayerServerList serverList)
{
    public void Create(EntMut root, ServerEntry server)
    {
        Node(root, out var form)
            .Mutate(s.Dialog);
        {
            Node(form)
                .Mutate(s.Label)
                .AlignmentV(Alignment.Horizontal)
                .TextV("Are you sure you want to remove this server?");

            Node(form)
                .Mutate(s.Label)
                .AlignmentV(Alignment.Horizontal)
                .TextV($"'{server.Name}' will be lost forever! (A long time!)");

            Node(form, out var buttons)
                .Mutate(s.DialogButtons)
                .MarginV((0, s.ItemHeight, 0, 0));
            {
                Node(buttons)
                    .OnPressF(() =>
                    {
                        serverList.Remove(server);
                        PopMenu(root);
                        RefreshMenu(root);
                    })
                    .TextV("Delete")
                    .Mutate(s.Button)
                    .SizeRelativeV((0, 0))
                    .SizeV((s.ItemWidthL, s.ItemHeight));

                Node(buttons)
                    .OnPressF(() => PopMenu(root))
                    .TextV("Cancel")
                    .Mutate(s.Button)
                    .SizeRelativeV((0, 0))
                    .SizeV((s.ItemWidthL, s.ItemHeight));
            }
        }
    }
}
