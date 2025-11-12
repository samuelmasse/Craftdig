namespace Crafthoe.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerLoginMenu(
    AppStyle s,
    ModuleMultiplayerCredentials multiplayerCredentials,
    ModuleMultiplayerConnectMenu connectMenu)
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
                .TextV("Google Login")
                .OnFrameF(() =>
                {
                    if (multiplayerCredentials.Email == null)
                        return;

                    root.StackRootV()?.NodeStack().Pop();
                    root.StackRootV()?.NodeStack().Push(
                        Node().StackRootV(root.StackRootV()).Mut(connectMenu.Create));
                });

            bool loginStarted = false;

            Node(form)
                .OnPressF(() =>
                {
                    multiplayerCredentials.StartLogin();
                    loginStarted = true;
                })
                .IsInputDisabledF(() => loginStarted)
                .TextV("Login")
                .Mut(s.Button);

            Node(form)
                .OnPressF(() =>
                {
                    multiplayerCredentials.StopLogin();
                    root.StackRootV()?.NodeStack().Pop();
                })
                .TextV("Cancel")
                .Mut(s.Button);
        }
    }
}
