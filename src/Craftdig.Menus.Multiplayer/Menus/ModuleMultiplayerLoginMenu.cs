namespace Craftdig.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerLoginMenu(
    AppStyle s,
    ModuleMultiplayerCredentials multiplayerCredentials,
    ModuleScope module)
{
    public void Create(EntMut root)
    {
        Node(root, out var form)
            .Mutate(s.Form)
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

                    PopMenu(root);
                    PushMenu(root, module.Get<ModuleMultiplayerServerBrowserMenu>().Create, s.Darken);
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
                    PopMenu(root);
                })
                .TextV("Cancel")
                .Mutate(s.Button);
        }
    }
}
