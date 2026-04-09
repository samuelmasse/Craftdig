namespace Craftdig.Menus.Singleplayer;

[Module]
public class ModuleSingleplayerEditWorldMenu(
    AppStyle s,
    ModuleWriteWorldMetaAction writeWorldMetaAction)
{
    public void Create(EntMut root, WorldEntry world)
    {
        var name = new StringBuilder(world.Meta.Name);

        Node(root, out var form)
            .Mutate(s.Form)
            .OffsetV((0, s.ItemHeight));
        {
            Node(form)
                .Mutate(s.Label)
                .TextV("World Name");

            Node(form)
                .Mutate(s.Textbox)
                .MaxLengthV(29)
                .StringBuilderV(name)
                .IsInitialFocusV(true);
        }

        Node(root, out var bottomBar)
            .Mutate(s.BottomBar)
            .SizeV((0, s.BarHeight - s.ItemHeight));
        {
            Node(bottomBar, out var buttonsList)
                .Mutate(s.ButtonBar);
            {
                Node(buttonsList)
                    .OnPressF(() =>
                    {
                        var worldName = name.ToString();
                        if (string.IsNullOrEmpty(worldName))
                            worldName = world.Meta.Name;

                        writeWorldMetaAction.Write(world.Meta with { Name = worldName }, world.Paths);

                        PopMenu(root);
                        RefreshMenu(root);
                    })
                    .TextV("Save")
                    .Mutate(s.Button)
                    .SizeV((s.ItemWidthL, s.ItemHeight));

                Node(buttonsList)
                    .OnPressF(() => PopMenu(root))
                    .TextV("Cancel")
                    .Mutate(s.Button)
                    .SizeV((s.ItemWidthL, s.ItemHeight));
            }
        }
    }
}
