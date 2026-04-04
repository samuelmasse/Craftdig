namespace Craftdig.Menus.Multiplayer;

[Module]
public class PlayerMultiplayerDisconnectedMenu(
    AppStyle s,
    AppReset reset)
{
    public void Create(EntMut root)
    {
        Node(root, out var form)
            .Mutate(s.VerticalList)
            .SizeV((s.ItemWidth * 2, 0))
            .InnerSpacingV(s.ItemSpacing)
            .AlignmentV(Alignment.Center);
        {
            Node(form)
                .Mutate(s.Label)
                .AlignmentV(Alignment.Horizontal)
                .TextV("Connection lost");

            Node(form)
                .OnPressF(reset.Run)
                .TextV("Ok")
                .Mutate(s.Button);
        }
    }
}
