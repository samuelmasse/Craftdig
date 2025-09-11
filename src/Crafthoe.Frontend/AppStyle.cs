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

    public void Slot(EntObj ent) => ent.TagV(nameof(Slot))
        .SizeV((SlotSize, SlotSize))
        .SizeRelativeV((0, 0))
        .TextureF(() =>
        {
            var c = ent.GetSlotValueF()?.Invoke() ?? default;

            if (c.IsBlock())
                return c.Faces().Front.FaceTexture();

            return null;
        });

    public void SlotTooltip(EntObj ent) => ent
        .TooltipF(() => ent.SlotV().PlayerV().Offhand() == default ?
            ent.SlotV().GetSlotValueF()?.Invoke().Name() : null);

    public void SlotButton(EntObj ent) => ent
        .TagV(nameof(SlotButton))
        .IsSelectableV(true)
        .ColorF(() => ent.IsHoveredR() ? (1, 1, 1, 0.5f) : default)
        .OnPressF(() =>
        {
            if (ent.SlotV().PlayerV().Offhand() == default)
            {
                ent.SlotV().PlayerV().Offhand() = ent.SlotV().GetSlotValueF()?.Invoke() ?? default;
                ent.SlotV().SetSlotValueF()?.Invoke(default);
                ent.SlotAddedV() = true;
            }
        })
        .OnClickF(() =>
        {
            if (!ent.SlotAddedV())
            {
                var val = ent.SlotV().GetSlotValueF()?.Invoke() ?? default;
                ent.SlotV().SetSlotValueF()?.Invoke(ent.SlotV().PlayerV().Offhand());
                ent.SlotV().PlayerV().Offhand() = val;
            }
        
            ent.SlotAddedV() = false;
        });
}
