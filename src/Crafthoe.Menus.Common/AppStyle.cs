namespace Crafthoe.Menus.Common;

[App]
public class AppStyle(RootText text, RootKeyboard keyboard, AppMenuTextures menuTextures, AppMonocraft monocraft)
{
    public readonly Texture ArrowTexture = menuTextures["MenuArrow"];
    public readonly Texture SlotTexture = menuTextures["MenuSlot"];

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
    public float SlotSize => 72;

    public Vector2 Horizontal => (1, 0);
    public Vector2 Vertical => (0, 1);

    public Vector4 BoardColor => (1, 0, 0, 1);
    public Vector4 BoardColor2 => (1, 1, 0, 1);

    public Vector4 TextColor => (1, 1, 1, 1);
    public Vector4 ButtonColor => (1, 0, 1, 1);
    public Vector4 ButtonColorDisabled => (0.4f, 0, 0.4f, 1);
    public Vector4 ButtonColorHovered => (1, 0.7f, 1, 1);
    public Vector4 TooltipColor => (0.5f, 0.28f, 1, 1);

    public Font Font => monocraft.Font;

    public void Text(EntObj ent) => ent
        .Tag(nameof(Text))
        .FontV(Font)
        .FontSizeV(FontSize)
        .FontPaddingV((ItemSpacingXS, 0, ItemSpacingXS, 0))
        .TextColorV(TextColor);

    public void Label(EntObj ent) => ent
        .Mut(Text)
        .Tag(nameof(Label))
        .SizeTextRelativeV((1, 1))
        .SizeRelativeV((0, 0));

    public void InputItem(EntObj ent) => ent
        .SizeV((0, ItemHeight))
        .SizeRelativeV((1, 0))
        .IsSelectableV(true)
        .IsFocusableV(true)
        .Nodes([
            Node()
                .AlignmentV(Alignment.Top | Alignment.Left)
                .ColorF(() => InputItemBorderColor(ent))
                .SizeV((ItemSpacingXS, 0))
                .SizeRelativeV((0, 1)),
            Node()
                .AlignmentV(Alignment.Top | Alignment.Right)
                .ColorF(() => InputItemBorderColor(ent))
                .SizeV((ItemSpacingXS, 0))
                .SizeRelativeV((0, 1)),
            Node()
                .AlignmentV(Alignment.Top | Alignment.Left)
                .ColorF(() => InputItemBorderColor(ent))
                .SizeV((0, ItemSpacingXS))
                .SizeRelativeV((1, 0)),
            Node()
                .AlignmentV(Alignment.Bottom | Alignment.Left)
                .ColorF(() => InputItemBorderColor(ent))
                .SizeV((0, ItemSpacingXS))
                .SizeRelativeV((1, 0))]);

    public Vector4 InputItemBorderColor(EntObj ent)
    {
        if (Get(ent.IsInputDisabledV(), ent.IsInputDisabledF()))
            return (0.2f, 0.2f, 0.2f, 1f);

        if ((ent.IsFocusedR() || ent.IsHoveredR()))
            return (1, 1, 1, 1);

        return (0, 0, 0, 1);
    }

    public void Textbox(EntObj ent) => ent
        .Mut(InputItem)
        .Mut(Text)
        .Tag(nameof(Textbox))
        .TextAlignmentV(Alignment.Left | Alignment.Vertical)
        .ColorV(ButtonColorDisabled)
        .TextF(() => text.Format("{0}{1}", ent.StringBuilderV(), ent.CarretR()))
        .TextPaddingV((ItemSpacingXS, ItemSpacingXS, ItemSpacingXS, ItemSpacingXS))
        .CursorV(MouseCursor.IBeam)
        .OnUpdateF(() =>
        {
            ent.CarretR() = string.Empty;

            var sb = ent.StringBuilderV();
            if (sb == null)
                return;

            if (ent.IsFocusedR())
            {
                if (!ent.WasFocusedR())
                {
                    ent.FocusStartR() = DateTime.UtcNow;
                    ent.WasFocusedR() = true;
                }

                int dt = (int)(DateTime.UtcNow - ent.FocusStartR()).TotalMilliseconds;
                if ((dt / 500) % 2 == 0)
                    ent.CarretR() = "_";

                if (keyboard.IsKeyPressedRepeated(Keys.Backspace) && sb.Length > 0)
                    sb.Remove(sb.Length - 1, 1);

                if (keyboard.Text.Count > 0)
                {
                    foreach (var rune in keyboard.Text)
                        sb.Append(rune);
                }

                if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyPressed(Keys.V))
                    sb.Append(keyboard.Clipboard);
            }
            else ent.WasFocusedR() = false;

            if (ent.MaxLengthV() > 0)
            {
                while (sb.Length > ent.MaxLengthV())
                    sb.Remove(sb.Length - 1, 1);
            }
        });

    public void Button(EntObj ent) => ent
        .Mut(InputItem)
        .Mut(Text)
        .Tag(nameof(Button))
        .OnUpdateF(() =>
        {
            if (ent.IsFocusedR() && keyboard.IsKeyPressedRepeated(Keys.Enter))
                ent.OnPressF()?.Invoke();
        })
        .CursorF(() => Get(ent.IsInputDisabledV(), ent.IsInputDisabledF()) ? MouseCursor.Default : MouseCursor.PointingHand)
        .ColorF(() =>
        {
            if (Get(ent.IsInputDisabledV(), ent.IsInputDisabledF()))
                return ButtonColorDisabled;

            return ButtonColor;
        });

    public void VerticalList(EntObj ent) => ent
        .Tag(nameof(VerticalList))
        .InnerLayoutV(InnerLayout.VerticalList)
        .SizeInnerSumRelativeV(Vertical)
        .SizeRelativeV((0, 0));

    public void HorizontalList(EntObj ent) => ent
        .Tag(nameof(HorizontalList))
        .InnerLayoutV(InnerLayout.HorizontalList)
        .SizeInnerSumRelativeV(Horizontal)
        .SizeRelativeV((0, 0));

    public void Slot(EntObj ent) => ent
        .Tag(nameof(Slot))
        .SizeV((SlotSize, SlotSize))
        .SizeRelativeV((0, 0))
        .TextureV(SlotTexture)
        .Nodes([Node()
            .Mut(Text)
            .OffsetV((ItemSpacingXS, ItemSpacingXS))
            .SizeV((-ItemSpacingXS * 2, -ItemSpacingXS * 2))
            .TextF(() =>
            {
                var c = ent.GetSlotValueF()?.Invoke() ?? default;

                if (c == default || c.Count == 1)
                    return string.Empty;

                return text.Format("{0}", c.Count);
            })
            .TextAlignmentV(Alignment.Bottom | Alignment.Right)
            .TextureF(() =>
            {
                var c = ent.GetSlotValueF()?.Invoke() ?? default;

                if (c.Item.IsBlock())
                    return c.Item.Faces().Front.FaceTexture();

                return null;
            })]);

    public void SlotTooltip(EntObj ent) => ent
        .TooltipF(() => ent.SlotV().PlayerV().Offhand() == default ?
            ent.SlotV().GetSlotValueF()?.Invoke().Item.Name() : null);

    public void SlotButton(EntObj ent) => ent
        .Tag(nameof(SlotButton))
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
