namespace Crafthoe.Frontend;

[Module]
public class ModuleMultiPlayerConnectMenu(
    AppStyle s,
    ModuleMultiPlayerConnectAction multiPlayerConnectAction)
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
                .MaxLengthV(29)
                .StringBuilderV(host)
                .IsInitialFocusV(true);

            Node(form)
                .Mut(s.Label)
                .TextV("Port");

            Node(form)
                .Mut(s.Textbox)
                .MaxLengthV(29)
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
                    Node(leftButtonsVertical)
                        .OnPressF(() =>
                        {
                            string connHost = host.ToString();
                            string connPort = port.ToString();
                            int.TryParse(connPort, out int numberPort);

                            multiPlayerConnectAction.Run(connHost, numberPort);
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
