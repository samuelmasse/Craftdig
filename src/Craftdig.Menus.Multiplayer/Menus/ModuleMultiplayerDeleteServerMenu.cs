namespace Craftdig.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerDeleteServerMenu(
    AppStyle s,
    ModuleScope module,
    ModuleMultiplayerServerList serverList)
{
    public void Create(EntMut root, ServerEntry server)
    {
        Node(root, out var form)
            .Mutate(s.VerticalList)
            .SizeInnerMaxRelativeV((1, 0))
            .InnerSpacingV(s.ItemSpacing)
            .AlignmentV(Alignment.Center);
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
                .MarginV((0, s.ItemHeight, 0, 0))
                .SizeInnerMaxRelativeV(s.Vertical)
                .SizeInnerSumRelativeV((1, 0))
                .AlignmentV(Alignment.Horizontal)
                .InnerSpacingV(s.ItemSpacing)
                .InnerLayoutV(InnerLayout.HorizontalList);
            {
                Node(buttons)
                    .OnPressF(() =>
                    {
                        serverList.Remove(server);

                        NodeStackPopR(root);
                        NodeStackPopR(root);

                        NodeSR(root).Mutate(module.Get<ModuleMultiplayerServerBrowserMenu>().Create);
                    })
                    .TextV("Delete")
                    .Mutate(s.Button)
                    .SizeRelativeV((0, 0))
                    .SizeV((s.ItemWidthL, s.ItemHeight));

                Node(buttons)
                    .OnPressF(() => NodeStackPopR(root))
                    .TextV("Cancel")
                    .Mutate(s.Button)
                    .SizeRelativeV((0, 0))
                    .SizeV((s.ItemWidthL, s.ItemHeight));
            }
        }
    }
}
