namespace Crafthoe.Frontend;

[Module]
public class ModuleMultiPlayerConnectMenu(
    AppStyle s,
    ModuleMultiPlayerConnectAction multiPlayerConnectAction,
    ModuleMultiPlayerConnectingMenu moduleMultiPlayerConnectingMenu)
{
    public void Create(EntObj root)
    {
        string defaultName = "localhost";
        string defaultPort = "8080";

        var host = new StringBuilder(defaultName);
        var port = new StringBuilder(defaultPort);

        Node(root, out var form)
            .Mut(s.VerticalList)
            .OffsetV((0, s.ItemHeight))
            .SizeV((s.ItemWidth * 2, 0))
            .InnerSpacingV(s.ItemSpacing)
            .AlignmentV(Alignment.Horizontal);
        {
            Node(form)
                .Mut(s.Label)
                .TextV("Host");

            Node(form)
                .Mut(s.Textbox)
                .MaxLengthV(120)
                .StringBuilderV(host)
                .IsInitialFocusV(true);

            Node(form)
                .Mut(s.Label)
                .TextV("Port");

            Node(form)
                .Mut(s.Textbox)
                .MaxLengthV(6)
                .StringBuilderV(port);
        }

        Node(root, out var bottomBar)
            .SizeRelativeV(s.Horizontal)
            .SizeV((0, s.BarHeight - s.ItemHeight))
            .AlignmentV(Alignment.Horizontal | Alignment.Bottom)
            .ColorV(s.BoardColor);
        {
            Node(bottomBar, out var buttonsList)
                .Mut(s.HorizontalList)
                .AlignmentV(Alignment.Center)
                .OffsetMultiplierV(s.ItemSpacingXS)
                .SizeInnerMaxRelativeV(s.Vertical)
                .InnerSpacingV(s.ItemSpacingL)
                .ColorV(s.BoardColor2);
            {
                Node(buttonsList, out var leftButtonsVertical)
                    .Mut(s.VerticalList)
                    .SizeV((s.ItemWidthL, 0))
                    .InnerSpacingV(s.ItemSpacing);
                {
                    var portChars = new char[byte.MaxValue];

                    Node(leftButtonsVertical)
                        .OnPressF(() =>
                        {
                            string connHost = host.ToString();
                            int connPort = int.Parse(port.ToString());

                            multiPlayerConnectAction.Start(connHost, connPort);

                            root.StackRootV()?.NodeStack().Push(
                                Node().StackRootV(root.StackRootV()).Mut(moduleMultiPlayerConnectingMenu.Create));
                        })
                        .IsInputDisabledF(() =>
                        {
                            port.CopyTo(0, portChars, port.Length);
                            return !int.TryParse(portChars.AsSpan()[..port.Length], out _);
                        })
                        .TextV("Connect")
                        .Mut(s.Button);
                }

                Node(buttonsList, out var rightButtonsVertical)
                    .Mut(s.VerticalList)
                    .SizeV((s.ItemWidthL, 0))
                    .InnerSpacingV(s.ItemSpacing);
                {
                    Node(rightButtonsVertical)
                        .OnPressF(() => root.StackRootV()?.NodeStack().Pop())
                        .TextV("Cancel")
                        .Mut(s.Button);
                }
            }
        }
    }
}
