namespace Crafthoe.Frontend;

[App]
public class AppStyle(AppMonocraft monocraft)
{
    public int FontSize => 32;
    public int FontSizeTitle => 160;

    public float ItemSpacingXS => 4;
    public float ItemSpacingS => 8;
    public float ItemSpacing => 16;
    public float ItemSpacingL => 24;
    public float ItemSpacingXL => 32;
    public float ItemSpacingXXL => 48;
    public float ItemHeight => 80;
    public float ItemWidth => 320;
    public float ItemWidthL => 512;
    public float BarHeight => 192;
    public float SlotSize => 64;

    public Vector2 Horizontal => (1, 0);
    public Vector2 Vertical => (0, 1);

    public Vector4 BoardColor => (1, 0, 0, 1);
    public Vector4 BoardColor2 => (1, 1, 0, 1);
    public Vector4 TextColor => (1, 1, 1, 1);
    public Vector4 ButtonColor => (1, 0, 1, 1);
    public Vector4 ButtonColorHovered => (1, 1, 1, 1);
    public Vector4 TooltipColor => (0.5f, 0.28f, 1, 1);

    public Font Font => monocraft.Font;

    public void Text(EntObj ent) => ent
        .TagV(nameof(Text))
        .FontV(Font)
        .FontSizeV(FontSize)
        .TextColorV(TextColor);

    public void Label(EntObj ent) => ent
        .Mut(Text)
        .TagV(nameof(Label))
        .SizeTextRelativeV((1, 1))
        .SizeRelativeV((0, 0));

    public void Button(EntObj ent) => ent
        .Mut(Text)
        .TagV(nameof(Button))
        .ColorF(() => ent.IsHoveredR() ? ButtonColorHovered : ButtonColor)
        .SizeV((0, ItemHeight))
        .SizeRelativeV((1, 0))
        .IsSelectableV(true);

    public void VerticalList(EntObj ent) => ent
        .TagV(nameof(VerticalList))
        .InnerLayoutV(InnerLayout.VerticalList)
        .SizeInnerSumRelativeV(Vertical)
        .SizeRelativeV((0, 0));

    public void HorizontalList(EntObj ent) => ent
        .TagV(nameof(HorizontalList))
        .InnerLayoutV(InnerLayout.HorizontalList)
        .SizeInnerSumRelativeV(Horizontal)
        .SizeRelativeV((0, 0));
}
