namespace Craftdig.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerLoginMenu(
    AppStyle s,
    ModuleMultiplayerCredentials multiplayerCredentials,
    ModuleMultiplayerConnectMenu connectMenu)
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
                .TextV("Google Login")
                .OnFrameF(() =>
                {
                    if (multiplayerCredentials.Email == null)
                        return;

                    root.StackRootFV.Resolve().NodeStack.Pop();
                    NodeStack(root.StackRootFV.Resolve()).StackRootV(root.StackRootFV.Resolve()).Mutate(connectMenu.Create);
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
                .Mutate(s.Button);

            Node(form)
                .OnPressF(() =>
                {
                    multiplayerCredentials.StopLogin();
                    root.StackRootFV.Resolve().NodeStack.Pop();
                })
                .TextV("Cancel")
                .Mutate(s.Button);
        }
    }
}
