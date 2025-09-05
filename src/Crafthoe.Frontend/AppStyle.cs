namespace Crafthoe.Frontend;

[App]
public class AppStyle(AppMonocraft monocraft)
{
    public void Text(EntObj ent) => ent
        .FontV(monocraft.Font)
        .FontSizeV(50)
        .TextColorV((1, 1, 1, 1));

    public void Label(EntObj ent) => ent
        .Mut(Text)
        .SizeTextRelativeV((1, 1));

    public void Button(EntObj ent) => ent
        .Mut(Text)
        .ColorF(() => ent.IsHoveredR() ? (1, 1, 1, 1) : (1, 0, 1, 1))
        .SizeV((0, 128))
        .SizeRelativeV((1, 0))
        .IsSelectableV(true);
}
