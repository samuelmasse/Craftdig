namespace Craftdig.Menus.Singleplayer;

[Module]
public class ModuleSingleplayerDeleteWorldMenu(
    AppStyle s,
    ModuleScope scope)
{
    public void Create(EntMut root, WorldEntry world)
    {
        Node(root, out var form)
            .Mutate(s.VerticalList)
            .SizeInnerMaxRelativeV((1, 0))
            .InnerSpacingV(s.ItemSpacing)
            .AlignmentV(Alignment.Center);
        {
            Node(form)
                .Mutate(s.Label)
                .AlignmentV(Alignment.Horizontal)
                .TextV("Are you sure you want to delete this world?");

            Node(form)
                .Mutate(s.Label)
                .AlignmentV(Alignment.Horizontal)
                .TextV($"'{world.Meta.Name}' will be lost forever! (A long time!)");

            Node(form)
                .Mutate(s.Label);

            Node(form, out var buttons)
                .SizeInnerMaxRelativeV(s.Vertical)
                .SizeInnerSumRelativeV((1, 0))
                .AlignmentV(Alignment.Horizontal)
                .InnerSpacingV(s.ItemSpacing)
                .InnerLayoutV(InnerLayout.HorizontalList);
            {
                Node(buttons)
                    .OnPressF(() =>
                    {
                        Directory.Delete(world.Dir, true);

                        NodeStackPopR(root);
                        NodeStackPopR(root);

                        NodeSR(root).Mutate(scope.Get<ModuleSingleplayerWorldSelectMenu>().Create);
                    })
                    .TextV("Delete")
                    .Mutate(s.Button)
                    .SizeRelativeV((0, 0))
                    .SizeV((s.ItemWidthL, s.ItemHeight));

                Node(buttons)
                    .OnPressF(() => NodeStackPopR(root))
                    .TextV("Cancel")
                    .Mutate(s.Button)
                    .SizeRelativeV((0, 0))
                    .SizeV((s.ItemWidthL, s.ItemHeight));
            }
        }
    }
}
