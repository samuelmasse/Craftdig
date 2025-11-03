namespace Crafthoe.Menus.Multiplayer;

[Module]
public class PlayerMultiplayerDisconnectedMenu(
    AppStyle s,
    AppReset reset)
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
                .TextV("Connection lost");

            Node(form)
                .OnPressF(reset.Run)
                .TextV("Ok")
                .Mut(s.Button);
        }
    }
}
