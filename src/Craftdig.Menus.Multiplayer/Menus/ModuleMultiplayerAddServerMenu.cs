namespace Craftdig.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerAddServerMenu(
    AppStyle s,
    ModuleMultiplayerServerList serverList)
{
    public void Create(EntMut root, ServerEntry? editing)
    {
        var name = new StringBuilder(editing?.Name ?? "Craftdig Server");
        var host = new StringBuilder(editing != null ? $"{editing.Address.Host}:{editing.Address.Port}" : "");

        Node(root, out var form)
            .Mutate(s.Form)
            .OffsetV((0, s.ItemSpacingXXL));
        {
            Node(form)
                .Mutate(s.Label)
                .AlignmentV(Alignment.Horizontal)
                .TextV(editing != null ? "Edit Server Info" : "Add Server");

            Node(form)
                .Mutate(s.Label)
                .MarginV((0, s.ItemHeight, 0, 0))
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

            Node(form)
                .Mutate(s.Button)
                .MarginV((0, s.ItemHeight, 0, 0))
                .TextV("Done")
                .IsInputDisabledF(() => host.Length == 0)
                .OnPressF(() =>
                {
                    var entry = new ServerEntry(name.ToString(), ServerAddress.Parse(host.ToString()));

                    if (editing != null)
                        serverList.Edit(editing, entry);
                    else serverList.Add(entry);

                    PopMenu(root);
                    RefreshMenu(root);
                });

            Node(form)
                .Mutate(s.Button)
                .TextV("Cancel")
                .OnPressF(() => PopMenu(root));
        }
    }
}
