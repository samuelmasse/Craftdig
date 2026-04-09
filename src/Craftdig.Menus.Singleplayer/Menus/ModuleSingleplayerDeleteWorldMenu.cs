namespace Craftdig.Menus.Singleplayer;

[Module]
public class ModuleSingleplayerDeleteWorldMenu(
    AppStyle s)
{
    public void Create(EntMut root, WorldEntry world)
    {
        Node(root, out var form)
            .Mutate(s.Dialog);
        {
            Node(form)
                .Mutate(s.Label)
                .AlignmentV(Alignment.Horizontal)
                .TextV("Are you sure you want to delete this world?");

            Node(form)
                .Mutate(s.Label)
                .AlignmentV(Alignment.Horizontal)
                .TextV($"'{world.Meta.Name}' will be lost forever! (A long time!)");

            Node(form, out var buttons)
                .Mutate(s.DialogButtons)
                .MarginV((0, s.ItemHeight, 0, 0));
            {
                Node(buttons)
                    .OnPressF(() =>
                    {
                        Directory.Delete(world.Dir, true);

                        PopMenu(root);
                        RefreshMenu(root);
                    })
                    .TextV("Delete")
                    .Mutate(s.Button)
                    .SizeRelativeV((0, 0))
                    .SizeV((s.ItemWidthL, s.ItemHeight));

                Node(buttons)
                    .OnPressF(() => PopMenu(root))
                    .TextV("Cancel")
                    .Mutate(s.Button)
                    .SizeRelativeV((0, 0))
                    .SizeV((s.ItemWidthL, s.ItemHeight));
            }
        }
    }
}
