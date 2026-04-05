namespace Craftdig.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerAddServerMenu(
    AppStyle s,
    ModuleScope module,
    ModuleMultiplayerServerList serverList)
{
    public void Create(EntMut root, ServerEntry? editing)
    {
        var name = new StringBuilder(editing?.Name ?? "Craftdig Server");
        var host = new StringBuilder(editing?.Host ?? "");

        Node(root, out var topBar)
            .SizeRelativeV(s.Horizontal)
            .SizeV((0, s.BarHeight))
            .ColorV(s.BoardColor);
        {
            Node(topBar)
                .Mutate(s.Label)
                .AlignmentV(Alignment.Center)
                .TextV(editing != null ? "Edit Server Info" : "Edit Server Info");
        }

        Node(root, out var form)
            .Mutate(s.VerticalList)
            .SizeV((s.ItemWidth * 2, 0))
            .InnerSpacingV(s.ItemSpacing)
            .AlignmentV(Alignment.Horizontal)
            .OffsetV((0, s.BarHeight + s.ItemSpacing));
        {
            Node(form)
                .Mutate(s.Label)
                .TextV("Server Name");

            Node(form)
                .Mutate(s.Textbox)
                .MaxLengthV(64)
                .StringBuilderV(name)
                .IsInitialFocusV(true);

            Node(form)
                .Mutate(s.Label)
                .TextV("Server Address");

            Node(form)
                .Mutate(s.Textbox)
                .MaxLengthV(120)
                .StringBuilderV(host);
        }

        Node(root, out var bottomBar)
            .SizeRelativeV(s.Horizontal)
            .SizeV((0, s.BarHeight - s.ItemHeight))
            .AlignmentV(Alignment.Horizontal | Alignment.Bottom)
            .ColorV(s.BoardColor);
        {
            Node(bottomBar, out var buttonsList)
                .Mutate(s.HorizontalList)
                .AlignmentV(Alignment.Center)
                .OffsetMultiplierV(s.ItemSpacingXS)
                .SizeInnerMaxRelativeV(s.Vertical)
                .InnerSpacingV(s.ItemSpacingL)
                .ColorV(s.BoardColor2);
            {
                Node(buttonsList)
                    .Mutate(s.Button)
                    .SizeV((s.ItemWidthL, s.ItemHeight))
                    .TextV("Done")
                    .IsInputDisabledF(() => host.Length == 0)
                    .OnPressF(() =>
                    {
                        var hostStr = host.ToString();
                        int port = 36676;

                        var colonIdx = hostStr.LastIndexOf(':');
                        if (colonIdx >= 0 && int.TryParse(hostStr.AsSpan()[(colonIdx + 1)..], out var parsed))
                        {
                            port = parsed;
                            hostStr = hostStr[..colonIdx];
                        }

                        var entry = new ServerEntry(name.ToString(), hostStr, port);

                        if (editing != null)
                            serverList.Edit(editing, entry);
                        else serverList.Add(entry);

                        NodeStackPopR(root);
                        NodeStackPopR(root);
                        NodeSR(root).Mutate(module.Get<ModuleMultiplayerServerBrowserMenu>().Create);
                    });

                Node(buttonsList)
                    .Mutate(s.Button)
                    .SizeV((s.ItemWidthL, s.ItemHeight))
                    .TextV("Cancel")
                    .OnPressF(() => NodeStackPopR(root));
            }
        }
    }
}
