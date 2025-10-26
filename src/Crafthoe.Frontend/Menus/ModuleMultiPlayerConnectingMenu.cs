namespace Crafthoe.Frontend;

[Module]
public class ModuleMultiPlayerConnectingMenu(
    AppStyle s,
    ModuleMultiPlayerConnectAction multiPlayerConnectAction,
    ModuleMultiPlayerJoinAction multiPlayerJoinAction)
{
    public void Create(EntObj root)
    {
        Node(root, out var form)
            .Mut(s.VerticalList)
            .SizeV((s.ItemWidth * 2, 0))
            .InnerSpacingV(s.ItemSpacing)
            .AlignmentV(Alignment.Center);
        {
            Node(form)
                .Mut(s.Label)
                .AlignmentV(Alignment.Horizontal)
                .TextF(() =>
                {
                    if (multiPlayerConnectAction.Exception != null)
                        return multiPlayerConnectAction.Exception.Message;

                    return "Connecting...";
                })
                .OnUpdateF(() =>
                {
                    if (multiPlayerConnectAction.Connecting)
                        return;

                    if (multiPlayerConnectAction.Socket != null)
                    {
                        multiPlayerJoinAction.Run(
                            multiPlayerConnectAction.Host!,
                            multiPlayerConnectAction.Port,
                            multiPlayerConnectAction.Socket);
                    }
                });

            Node(form)
                .OnPressF(() =>
                {
                    multiPlayerConnectAction.Cancel();
                    root.StackRootV()?.NodeStack().Pop();
                })
                .TextV("Cancel")
                .Mut(s.Button);
        }
    }
}
