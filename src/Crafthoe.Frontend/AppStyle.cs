namespace Crafthoe.Frontend;

[App]
public class AppStyle(RootText text, AppMonocraft monocraft)
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
        .Mut(Text)
        .TextV("64")
        .TextF(() =>
        {
            var c = ent.GetSlotValueF()?.Invoke() ?? default;

            if (c == default || c.Count == 1)
                return string.Empty;

            return text.Format("{0}", c.Count);
        })
        .TextAlignmentV(Alignment.Bottom | Alignment.Right)
        .SizeV((SlotSize, SlotSize))
        .SizeRelativeV((0, 0))
        .TextureF(() =>
        {
            var c = ent.GetSlotValueF()?.Invoke() ?? default;

            if (c.Item.IsBlock())
                return c.Item.Faces().Front.FaceTexture();

            return null;
        });

    public void SlotTooltip(EntObj ent) => ent
        .TooltipF(() => ent.SlotV().PlayerV().Offhand() == default ?
            ent.SlotV().GetSlotValueF()?.Invoke().Item.Name() : null);

    public void SlotButton(EntObj ent) => ent
        .TagV(nameof(SlotButton))
        .IsSelectableV(true)
        .ColorF(() => ent.IsHoveredR() ? (1, 1, 1, 0.5f) : default)
        .OnPressF(() =>
        {
            var val = ent.SlotV().GetSlotValueF()?.Invoke() ?? default;
            ref var offhand = ref ent.SlotV().PlayerV().Offhand();

            if (ent.SlotV().PlayerV().Offhand() == default)
            {
                offhand = val;
                ent.SlotV().SetSlotValueF()?.Invoke(default);
                ent.SlotAddedV() = true;
            }
        })
        .OnSecondaryPressF(() =>
        {
            var val = ent.SlotV().GetSlotValueF()?.Invoke() ?? default;
            ref var offhand = ref ent.SlotV().PlayerV().Offhand();

            if (offhand == default && val.Count > 0)
            {
                int give = (int)Math.Ceiling(val.Count / 2f);
                offhand = new(val.Item, give);

                if (val.Count - give > 0)
                    ent.SlotV().SetSlotValueF()?.Invoke(new(val.Item, val.Count - give));
                else ent.SlotV().SetSlotValueF()?.Invoke(default);

                ent.SlotAddedV() = true;
            }
        })
        .OnClickF(() =>
        {
            if (!ent.SlotAddedV())
            {
                var val = ent.SlotV().GetSlotValueF()?.Invoke() ?? default;
                ref var offhand = ref ent.SlotV().PlayerV().Offhand();

                if (val.Item == offhand.Item)
                {
                    int give = Math.Min(offhand.Count, val.Item.MaxStack() - val.Count);
                    if (give > 0)
                    {
                        if (offhand.Count - give > 0)
                            offhand = new(offhand.Item, offhand.Count - give);
                        else offhand = default;

                        ent.SlotV().SetSlotValueF()?.Invoke(new(val.Item, val.Count + give));
                    }
                }
                else
                {
                    ent.SlotV().SetSlotValueF()?.Invoke(offhand);
                    ent.SlotV().PlayerV().Offhand() = val;
                }
            }

            ent.SlotAddedV() = false;
        })
        .OnSecondaryClickF(() =>
        {
            if (!ent.SlotAddedV())
            {
                var val = ent.SlotV().GetSlotValueF()?.Invoke() ?? default;
                ref var offhand = ref ent.SlotV().PlayerV().Offhand();

                if (offhand.Count == 0)
                    return;

                if (val.Item == default || val.Item == offhand.Item)
                {
                    if (val.Count < offhand.Item.MaxStack())
                    {
                        ent.SlotV().SetSlotValueF()?.Invoke(new(offhand.Item, val.Count + 1));

                        if (offhand.Count == 1)
                            offhand = default;
                        else offhand = new(offhand.Item, offhand.Count - 1);
                    }
                }
                else if (val.Item != offhand.Item)
                {
                    ent.SlotV().SetSlotValueF()?.Invoke(offhand);
                    ent.SlotV().PlayerV().Offhand() = val;
                }
            }

            ent.SlotAddedV() = false;
        });

    public void SlotButtonInfinity(EntObj ent) => ent
        .Mut(SlotButton)
        .OnPressF(() =>
        {
            var val = ent.SlotV().GetSlotValueF()?.Invoke() ?? default;

            if (ent.SlotV().PlayerV().Offhand() == default)
            {
                ent.SlotV().PlayerV().Offhand() = val;
                ent.SlotAddedV() = true;
            }
        })
        .OnSecondaryPressF(ent.OnPressF())
        .OnClickF(() =>
        {
            if (!ent.SlotAddedV())
            {
                var val = ent.SlotV().GetSlotValueF()?.Invoke() ?? default;
                ref var offhand = ref ent.SlotV().PlayerV().Offhand();

                if (val.Item == offhand.Item)
                {
                    if (offhand.Count < val.Item.MaxStack())
                        offhand = new(offhand.Item, offhand.Count + 1);
                }
                else ent.SlotV().PlayerV().Offhand() = val;
            }

            ent.SlotAddedV() = false;
        })
        .OnSecondaryClickF(() =>
        {
            if (!ent.SlotAddedV())
            {
                var val = ent.SlotV().GetSlotValueF()?.Invoke() ?? default;
                ref var offhand = ref ent.SlotV().PlayerV().Offhand();

                if (offhand.Count == 0)
                    offhand = val;
                else
                {
                    if (offhand.Count == 1)
                        offhand = default;
                    else offhand = new(offhand.Item, offhand.Count - 1);
                }
            }

            ent.SlotAddedV() = false;
        });
}
