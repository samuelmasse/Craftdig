namespace Craftdig.Menus.Common;

[App]
public partial class AppStyle(RootText text, RootUiMouse mouse, RootKeyboard keyboard, RootInput input, RootUiScale scale, AppMenuTextures menuTextures, AppMonocraft monocraft)
{
    public readonly Texture ArrowTexture = menuTextures["MenuArrow"];
    public readonly Texture SlotTexture = menuTextures["MenuSlot"];
    public readonly Texture ButtonTexture = menuTextures["MenuButton"];
    public readonly Texture BackgroundTexture = menuTextures["MenuBackground"];

    public int FontSize => 36;
    public int FontSizeTitle => 180;

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
    public float SlotSize => 72;
    public float ScrollStep => 65;

    public Vec2 Horizontal => (1, 0);
    public Vec2 Vertical => (0, 1);

    public Vec4 BoardColor => (0.77f, 0.77f, 0.77f, 1);

    public Vec4 TextColor => (1, 1, 1, 1);
    public Vec4 TextColorDark => (0.25f, 0.25f, 0.25f, 1);
    public Vec4 TextShadowColor => (0, 0, 0, 0.5f);
    public Vec4 TextColorFaint => (0.5f, 0.5f, 0.5f, 1);
    public Vec4 SlotColor => (0.55f, 0.55f, 0.55f, 1);
    public Vec4 ButtonColor => (1, 1, 1, 1);
    public Vec4 ButtonColorDisabled => (0.4f, 0.4f, 0.4f, 1);
    public Vec4 ButtonColorHovered => (1, 0.7f, 1, 1);
    public Vec4 TooltipColor => (0.5f, 0.28f, 1, 1);

    public Vec2 TextureScale => (0.25f, 0.25f);

    public Font Font => monocraft.Font;

    public void Text(EntMut ent) => ent.Mutate()
        .Tag(nameof(Text))
        .FontV(Font)
        .FontSizeV(FontSize)
        .FontPaddingV((ItemSpacingXS, 0, ItemSpacingXS, 0))
        .TextColorV(TextColor)
        .TextShadowOffsetV((ItemSpacingXS, ItemSpacingXS))
        .TextShadowColorV(TextShadowColor)
        .TextAlignmentSnapV(ItemSpacingXS)
        .TextGlyphAlignmentSnapV(ItemSpacingXS);

    public void Label(EntMut ent) => ent.Mutate()
        .Mutate(Text)
        .Tag(nameof(Label))
        .SizeTextRelativeV((1, 1))
        .SizeRelativeV((0, 0))
        .SizeAlignmentSnapV(ItemSpacingS)
        .TextAlignmentV(Alignment.Left);

    public void LabelDark(EntMut ent) => ent.Mutate()
        .Mutate(Label)
        .TextShadowOffsetV(default)
        .TextColorV(TextColorDark);

    public void InputItem(EntMut ent, Func<Vec4> colorF) => ent.Mutate()
        .SizeV((0, ItemHeight))
        .SizeRelativeV((1, 0))
        .IsSelectableV(true)
        .IsFocusableV(true)
        .Mutate((ent) =>
        {
            Node(ent)
                .AlignmentV(Alignment.Top | Alignment.Left)
                .ColorF(colorF)
                .SizeV((ItemSpacingXS, 0))
                .SizeRelativeV((0, 1));

            Node(ent)
                .AlignmentV(Alignment.Top | Alignment.Right)
                .ColorF(colorF)
                .SizeV((ItemSpacingXS, 0))
                .SizeRelativeV((0, 1));

            Node(ent)
                .AlignmentV(Alignment.Top | Alignment.Left)
                .ColorF(colorF)
                .SizeV((0, ItemSpacingXS))
                .SizeRelativeV((1, 0));

            Node(ent)
                .AlignmentV(Alignment.Bottom | Alignment.Left)
                .ColorF(colorF)
                .SizeV((0, ItemSpacingXS))
                .SizeRelativeV((1, 0));
        });

    public Vec4 ButtonBorderColor(EntMut ent)
    {
        if (ent.IsInputDisabledFV.Resolve())
            return (0.2f, 0.2f, 0.2f, 1f);

        if ((ent.IsFocusedR || ent.IsHoveredR))
            return (1, 1, 1, 1);

        return (0, 0, 0, 1);
    }

    public Vec4 TextBoxBorderColor(EntMut ent)
    {
        if ((ent.IsFocusedR || ent.IsHoveredR))
            return (1, 1, 1, 1);

        return ButtonColorDisabled;
    }

    public void Button(EntMut ent) => ent.Mutate()
        .Mutate((ent) => InputItem(ent, () => ButtonBorderColor(ent)))
        .Mutate(Text)
        .Mutate(PointingCursor)
        .Tag(nameof(Button))
        .OnUpdateF(() =>
        {
            if (ent.IsFocusedR && keyboard.IsKeyPressedRepeated(Keys.Enter))
                ent.OnPressFV.Resolve()?.Invoke();
        })
        .TextureV(ButtonTexture)
        .TextureSubSizeRelativeV(TextureScale)
        .TextureMarginV((ItemSpacingXS, ItemSpacingXS, ItemSpacingXS, ItemSpacingXS))
        .TextureColorF(() =>
        {
            if (ent.IsInputDisabledFV.Resolve())
                return ButtonColorDisabled;

            return ButtonColor;
        });

    public void SelectorItem(EntMut ent) => ent.Mutate()
        .SizeV((0, ItemHeight))
        .SizeRelativeV((1, 0))
        .ColorF(() => ent.IsSelectedR ? (0, 0, 0, 1) : (0, 0, 0, 0))
        .OnUpdateF(() =>
        {
            if (ent.IsFocusedR && keyboard.IsKeyPressedRepeated(Keys.Enter))
                ent.OnDoubleClickFV.Resolve()?.Invoke();
        })
        .IsSelectableV(true)
        .IsFocusableV(true)
        .Mutate((ent) =>
        {
            Node(ent)
                .Mutate(Border)
                .AlignmentV(Alignment.Top | Alignment.Left)
                .SizeV((ItemSpacingXS, 0))
                .SizeRelativeV((0, 1));

            Node(ent)
                .Mutate(Border)
                .AlignmentV(Alignment.Top | Alignment.Right)
                .SizeV((ItemSpacingXS, 0))
                .SizeRelativeV((0, 1));

            Node(ent)
                .Mutate(Border)
                .AlignmentV(Alignment.Top | Alignment.Left)
                .SizeV((0, ItemSpacingXS))
                .SizeRelativeV((1, 0));

            Node(ent)
                .Mutate(Border)
                .AlignmentV(Alignment.Bottom | Alignment.Left)
                .SizeV((0, ItemSpacingXS))
                .SizeRelativeV((1, 0));

            void Border(EntMut x) => x.Mutate()
                .ColorF(() => ent.IsFocusedR ? (1, 1, 1, 1) : (0.5f, 0.5f, 0.5f, 1f))
                .IsDisabledF(() => !ent.IsSelectedR);
        });


    public void Selector(EntMut parent, out EntMut list)
    {
        float scroll = 0;

        Node(parent, out var select)
            .Mutate(VerticalList)
            .InnerScrollOffsetF(() => (0, -Snap.Round(scroll, ItemSpacingXS)))
            .SizeInnerMaxRelativeV((1, 0))
            .AlignmentV(Alignment.Horizontal);
        list = select;

        Node(parent, out var scrollBar)
            .SizeV((20, 0))
            .SizeRelativeV((0, 1))
            .AlignmentV(Alignment.Horizontal)
            .OffsetF(() => (select.SizeR.X / 2 + ItemSpacing, 0))
            .ColorV((0, 0, 0, 0.5f))
            .RenderDelayV(1)
            .IsDisabledF(() => parent.SizeR.Y / select.SizeInnerSumR.Y >= 1);
        {
            Vec2 pressPoint = default;
            float pressScroll = default;

            Node(scrollBar, out var scrollPuck)
                .IsSelectableV(true)
                .IsSilentFocusableV(true)
                .DeferFocusV(select)
                .CursorF(() => scrollPuck.IsPressedR ? CursorShape.ResizeVertical : CursorShape.Hand)
                .ColorV((1, 1, 1, 0.5f))
                .OffsetF(() => (0, scroll / select.SizeInnerSumR.Y) * scrollBar.SizeR)
                .SizeRelativeF(() => (1, Math.Min(1, parent.SizeR.Y / select.SizeInnerSumR.Y)))
                .OnPressF(() =>
                {
                    pressScroll = scroll;
                    pressPoint = mouse.Position - scrollBar.PositionR;
                })
                .OnUpdateF(() =>
                {
                    if (!scrollPuck.IsPressedR)
                        return;

                    var newPoint = mouse.Position - scrollBar.PositionR;
                    var delta = newPoint.Y - pressPoint.Y;
                    var deltaRelative = delta / scrollBar.SizeR.Y;
                    var deltaTranslated = deltaRelative * select.SizeInnerSumR.Y;
                    scroll = pressScroll + deltaTranslated;
                });
        }

        Node(parent)
            .SizeRelativeV((1, 1))
            .IsScrollableV(true)
            .OnScrollF((wheel) => scroll += -wheel.Y * ScrollStep)
            .OnUpdateF(() =>
            {
                if (scroll < 0)
                    scroll = 0;

                var maxScroll = Math.Max(0, select.SizeInnerSumR.Y - parent.SizeR.Y);
                if (scroll > maxScroll)
                    scroll = maxScroll;
            });
    }

    public void Form(EntMut ent) => ent.Mutate()
        .Mutate(VerticalList)
        .SizeV((ItemWidth * 2, 0))
        .InnerSpacingV(ItemSpacing)
        .AlignmentV(Alignment.Horizontal);

    public void Dialog(EntMut ent) => ent.Mutate()
        .Mutate(VerticalList)
        .SizeRelativeV((0, 0))
        .SizeInnerMaxRelativeV((1, 0))
        .SizeInnerSumRelativeV((0, 1))
        .InnerSpacingV(ItemSpacing)
        .AlignmentV(Alignment.Center);

    public void DialogButtons(EntMut ent) => ent.Mutate()
        .SizeRelativeV((0, 0))
        .SizeInnerMaxRelativeV(Vertical)
        .SizeInnerSumRelativeV((1, 0))
        .AlignmentV(Alignment.Horizontal)
        .InnerSpacingV(ItemSpacing)
        .InnerLayoutV(InnerLayout.HorizontalList);

    public void TopBar(EntMut ent) => ent.Mutate()
        .TextureV(ButtonTexture)
        .TextureSubSizeRelativeV(TextureScale)
        .TextureOriginRelativeV((0.5f, 0))
        .TextureAlignmentSnapV(1)
        .TextureColorV((0.6f, 0.6f, 0.6f, 1))
        .SizeRelativeV(Horizontal)
        .SizeV((0, BarHeight))
        .ColorV(BoardColor);

    public void BottomBar(EntMut ent) => ent.Mutate()
        .TextureV(ButtonTexture)
        .TextureSubSizeRelativeV(TextureScale)
        .TextureOriginRelativeV((0.5f, 0f))
        .TextureAlignmentSnapV(1)
        .TextureColorV((0.6f, 0.6f, 0.6f, 1))
        .SizeRelativeV(Horizontal)
        .SizeV((0, BarHeight))
        .AlignmentV(Alignment.Bottom)
        .AlignmentSnapV(ItemSpacingXS)
        .ColorV(BoardColor);

    public void MiddleBar(EntMut ent) => ent.Mutate()
        .SizeRelativeV((1, 1))
        .SizeV((0, -BarHeight * 2))
        .OffsetV((0, BarHeight));

    public void Darken(EntMut ent) => Node(ent)
        .SizeRelativeV((1, 1))
        .ColorV((0, 0, 0, 0.2f));

    public void ButtonBar(EntMut ent) => ent.Mutate()
        .Mutate(HorizontalList)
        .AlignmentV(Alignment.Center)
        .SizeInnerMaxRelativeV(Vertical)
        .InnerSpacingV(ItemSpacingL);

    public void ButtonRow(EntMut ent) => ent.Mutate()
        .SizeRelativeV(Horizontal)
        .SizeInnerMaxRelativeV(Vertical)
        .InnerSpacingV(ItemSpacing)
        .InnerLayoutV(InnerLayout.HorizontalList)
        .InnerSizingV(InnerSizing.HorizontalWeight);

    public void PointingCursor(EntMut ent) => ent.Mutate()
        .CursorF(() => ent.IsInputDisabledFV.Resolve() ? CursorShape.Default : CursorShape.Hand);

    public void VerticalList(EntMut ent) => ent.Mutate()
        .Tag(nameof(VerticalList))
        .InnerLayoutV(InnerLayout.VerticalList)
        .SizeInnerSumRelativeV(Vertical)
        .SizeRelativeV((0, 0));

    public void HorizontalList(EntMut ent) => ent.Mutate()
        .Tag(nameof(HorizontalList))
        .InnerLayoutV(InnerLayout.HorizontalList)
        .SizeInnerSumRelativeV(Horizontal)
        .SizeRelativeV((0, 0));

    public void Slot(EntMut ent) => ent.Mutate()
        .Tag(nameof(Slot))
        .SizeV((SlotSize, SlotSize))
        .SizeRelativeV((0, 0))
        .ColorV(SlotColor)
        .TextureV(SlotTexture)
        .TextureColorV(null)
        .TextureSubSizeRelativeV(null)
        .TextureMarginV(default)
        .Mutate((ent) =>
        {
            NodesClear(ent);

            Node(ent)
                .OffsetV((ItemSpacingXS, ItemSpacingXS))
                .SizeV((-ItemSpacingXS * 2, -ItemSpacingXS * 2))
                .TextureF(() =>
                {
                    var c = ent.GetSlotValueFV.Resolve()?.Invoke() ?? default;

                    if (c.Item.IsBlock)
                        return c.Item.Faces.Front.FaceTexture;

                    return null;
                });

            Node(ent)
                .Mutate(Text)
                .SizeRelativeV((1, 1))
                .OffsetV((0, 0))
                .TextF(() =>
                {
                    var c = ent.GetSlotValueFV.Resolve()?.Invoke() ?? default;

                    if (c == default || c.Count == 1)
                        return string.Empty;

                    return text.Format("{0}", c.Count);
                })
                .TextOffsetV((-ItemSpacingXS, ItemSpacingXS))
                .TextAlignmentV(Alignment.Bottom | Alignment.Right);
        });

    public void SlotTooltip(EntMut ent) => ent.Mutate()
        .TooltipF(() => ent.SlotFV.Resolve().PlayerFV.Resolve().Offhand == default ?
            ent.SlotFV.Resolve().GetSlotValueFV.Resolve()?.Invoke().Item.Name : null);

    public void SlotButton(EntMut ent) => ent.Mutate()
        .Tag(nameof(SlotButton))
        .IsSelectableV(true)
        .ColorF(() => ent.IsHoveredR ? (1, 1, 1, 0.5f) : default)
        .OnPressF(() =>
        {
            var val = ent.SlotFV.Resolve().GetSlotValueFV.Resolve()?.Invoke() ?? default;
            var offhand = ent.SlotFV.Resolve().PlayerFV.Resolve().Offhand;

            if (ent.SlotFV.Resolve().PlayerFV.Resolve().Offhand == default)
            {
                ent.SlotFV.Resolve().PlayerFV.Resolve().Mutate().Offhand(val);
                ent.SlotFV.Resolve().SetSlotValueFV.Resolve()?.Invoke(default);
                ent.SlotAddedR = true;
            }
        })
        .OnSecondaryPressF(() =>
        {
            var val = ent.SlotFV.Resolve().GetSlotValueFV.Resolve()?.Invoke() ?? default;
            var offhand = ent.SlotFV.Resolve().PlayerFV.Resolve().Offhand;

            if (offhand == default && val.Count > 0)
            {
                int give = (int)Math.Ceiling(val.Count / 2f);
                ent.SlotFV.Resolve().PlayerFV.Resolve().Mutate().Offhand(new(val.Item, give));

                if (val.Count - give > 0)
                    ent.SlotFV.Resolve().SetSlotValueFV.Resolve()?.Invoke(new(val.Item, val.Count - give));
                else ent.SlotFV.Resolve().SetSlotValueFV.Resolve()?.Invoke(default);

                ent.SlotAddedR = true;
            }
        })
        .OnClickF(() =>
        {
            if (!ent.SlotAddedR)
            {
                var val = ent.SlotFV.Resolve().GetSlotValueFV.Resolve()?.Invoke() ?? default;
                var offhand = ent.SlotFV.Resolve().PlayerFV.Resolve().Offhand;

                if (val.Item == offhand.Item)
                {
                    int give = Math.Min(offhand.Count, val.Item.MaxStack - val.Count);
                    if (give > 0)
                    {
                        if (offhand.Count - give > 0)
                            offhand = new(offhand.Item, offhand.Count - give);
                        else offhand = default;

                        ent.SlotFV.Resolve().PlayerFV.Resolve().Mutate().Offhand(offhand);
                        ent.SlotFV.Resolve().SetSlotValueFV.Resolve()?.Invoke(new(val.Item, val.Count + give));
                    }
                }
                else
                {
                    ent.SlotFV.Resolve().SetSlotValueFV.Resolve()?.Invoke(offhand);
                    ent.SlotFV.Resolve().PlayerFV.Resolve().Mutate().Offhand(val);
                }
            }

            ent.SlotAddedR = false;
        })
        .OnSecondaryClickF(() =>
        {
            if (!ent.SlotAddedR)
            {
                var val = ent.SlotFV.Resolve().GetSlotValueFV.Resolve()?.Invoke() ?? default;
                var offhand = ent.SlotFV.Resolve().PlayerFV.Resolve().Offhand;

                if (offhand.Count == 0)
                    return;

                if (val.Item == default || val.Item == offhand.Item)
                {
                    if (val.Count < offhand.Item.MaxStack)
                    {
                        ent.SlotFV.Resolve().SetSlotValueFV.Resolve()?.Invoke(new(offhand.Item, val.Count + 1));

                        if (offhand.Count == 1)
                            offhand = default;
                        else offhand = new(offhand.Item, offhand.Count - 1);

                        ent.SlotFV.Resolve().PlayerFV.Resolve().Mutate().Offhand(offhand);
                    }
                }
                else if (val.Item != offhand.Item)
                {
                    ent.SlotFV.Resolve().SetSlotValueFV.Resolve()?.Invoke(offhand);
                    ent.SlotFV.Resolve().PlayerFV.Resolve().Mutate().Offhand(val);
                }
            }

            ent.SlotAddedR = false;
        });

    public void SlotButtonInfinity(EntMut ent) => ent.Mutate()
        .Mutate(SlotButton)
        .OnPressF(() =>
        {
            var val = ent.SlotFV.Resolve().GetSlotValueFV.Resolve()?.Invoke() ?? default;

            if (ent.SlotFV.Resolve().PlayerFV.Resolve().Offhand == default)
            {
                ent.SlotFV.Resolve().PlayerFV.Resolve().Mutate().Offhand(val);
                ent.SlotAddedR = true;
            }
        })
        .OnSecondaryPressF(ent.OnPressFV.Resolve())
        .OnClickF(() =>
        {
            if (!ent.SlotAddedR)
            {
                var val = ent.SlotFV.Resolve().GetSlotValueFV.Resolve()?.Invoke() ?? default;
                var offhand = ent.SlotFV.Resolve().PlayerFV.Resolve().Offhand;

                if (val.Item == offhand.Item)
                {
                    if (offhand.Count < val.Item.MaxStack)
                        ent.SlotFV.Resolve().PlayerFV.Resolve().Mutate().Offhand(new(offhand.Item, offhand.Count + 1));
                }
                else ent.SlotFV.Resolve().PlayerFV.Resolve().Mutate().Offhand(val);
            }

            ent.SlotAddedR = false;
        })
        .OnSecondaryClickF(() =>
        {
            if (!ent.SlotAddedR)
            {
                var val = ent.SlotFV.Resolve().GetSlotValueFV.Resolve()?.Invoke() ?? default;
                var offhand = ent.SlotFV.Resolve().PlayerFV.Resolve().Offhand;

                if (offhand.Count == 0)
                    offhand = val;
                else
                {
                    if (offhand.Count == 1)
                        offhand = default;
                    else offhand = new(offhand.Item, offhand.Count - 1);
                }

                ent.SlotFV.Resolve().PlayerFV.Resolve().Mutate().Offhand(offhand);
            }

            ent.SlotAddedR = false;
        });

}
