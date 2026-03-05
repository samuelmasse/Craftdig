namespace Craftdig.Menus.Common;

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

    public void Text(EntObj ent) => ent.Mutate()
        .Tag(nameof(Text))
        .FontV(Font)
        .FontSizeV(FontSize)
        .FontPaddingV((ItemSpacingXS, 0, ItemSpacingXS, 0))
        .TextColorV(TextColor);

    public void Label(EntObj ent) => ent.Mutate()
        .Mutate(Text)
        .Tag(nameof(Label))
        .SizeTextRelativeV((1, 1))
        .SizeRelativeV((0, 0));

    public void InputItem(EntObj ent) => ent.Mutate()
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
        if (Get(ent.IsInputDisabledV, ent.IsInputDisabledFDelegate))
            return (0.2f, 0.2f, 0.2f, 1f);

        if ((ent.IsFocusedR || ent.IsHoveredR))
            return (1, 1, 1, 1);

        return (0, 0, 0, 1);
    }

    public void Textbox(EntObj ent) => ent.Mutate()
        .Mutate(InputItem)
        .Mutate(Text)
        .Tag(nameof(Textbox))
        .TextAlignmentV(Alignment.Left | Alignment.Vertical)
        .ColorV(ButtonColorDisabled)
        .TextF(() => text.Format("{0}{1}", ent.StringBuilderV, ent.CarretR))
        .TextPaddingV((ItemSpacingXS, ItemSpacingXS, ItemSpacingXS, ItemSpacingXS))
        .CursorV(MouseCursor.IBeam)
        .OnUpdateF(() =>
        {
            ent.CarretR = string.Empty;

            var sb = ent.StringBuilderV;
            if (sb == null)
                return;

            if (ent.IsFocusedR)
            {
                if (!ent.WasFocusedR)
                {
                    ent.FocusStartR = DateTime.UtcNow;
                    ent.WasFocusedR = true;
                }

                int dt = (int)(DateTime.UtcNow - ent.FocusStartR).TotalMilliseconds;
                if ((dt / 500) % 2 == 0)
                    ent.CarretR = "_";

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
            else ent.WasFocusedR = false;

            if (ent.MaxLengthV > 0)
            {
                while (sb.Length > ent.MaxLengthV)
                    sb.Remove(sb.Length - 1, 1);
            }
        });

    public void Button(EntObj ent) => ent.Mutate()
        .Mutate(InputItem)
        .Mutate(Text)
        .Tag(nameof(Button))
        .OnUpdateF(() =>
        {
            if (ent.IsFocusedR && keyboard.IsKeyPressedRepeated(Keys.Enter))
                ent.OnPressFDelegate?.Invoke();
        })
        .CursorF(() => Get(ent.IsInputDisabledV, ent.IsInputDisabledFDelegate) ? MouseCursor.Default : MouseCursor.PointingHand)
        .ColorF(() =>
        {
            if (Get(ent.IsInputDisabledV, ent.IsInputDisabledFDelegate))
                return ButtonColorDisabled;

            return ButtonColor;
        });

    public void VerticalList(EntObj ent) => ent.Mutate()
        .Tag(nameof(VerticalList))
        .InnerLayoutV(InnerLayout.VerticalList)
        .SizeInnerSumRelativeV(Vertical)
        .SizeRelativeV((0, 0));

    public void HorizontalList(EntObj ent) => ent.Mutate()
        .Tag(nameof(HorizontalList))
        .InnerLayoutV(InnerLayout.HorizontalList)
        .SizeInnerSumRelativeV(Horizontal)
        .SizeRelativeV((0, 0));

    public void Slot(EntObj ent) => ent.Mutate()
        .Tag(nameof(Slot))
        .SizeV((SlotSize, SlotSize))
        .SizeRelativeV((0, 0))
        .TextureV(SlotTexture)
        .Nodes([Node()
            .Mutate(Text)
            .OffsetV((ItemSpacingXS, ItemSpacingXS))
            .SizeV((-ItemSpacingXS * 2, -ItemSpacingXS * 2))
            .TextF(() =>
            {
                var c = ent.GetSlotValueFDelegate?.Invoke() ?? default;

                if (c == default || c.Count == 1)
                    return string.Empty;

                return text.Format("{0}", c.Count);
            })
            .TextAlignmentV(Alignment.Bottom | Alignment.Right)
            .TextureF(() =>
            {
                var c = ent.GetSlotValueFDelegate?.Invoke() ?? default;

                if (c.Item.IsBlock)
                    return c.Item.Faces.Front.FaceTexture;

                return null;
            })]);

    public void SlotTooltip(EntObj ent) => ent.Mutate()
        .TooltipF(() => ent.SlotV.PlayerV.GetOffhand() == default ?
            ent.SlotV.GetSlotValueFDelegate?.Invoke().Item.Name : null);

    public void SlotButton(EntObj ent) => ent.Mutate()
        .Tag(nameof(SlotButton))
        .IsSelectableV(true)
        .ColorF(() => ent.IsHoveredR ? (1, 1, 1, 0.5f) : default)
        .OnPressF(() =>
        {
            var val = ent.SlotV.GetSlotValueFDelegate?.Invoke() ?? default;
            var offhand = ent.SlotV.PlayerV.GetOffhand();

            if (ent.SlotV.PlayerV.GetOffhand() == default)
            {
                ent.SlotV.PlayerV.SetOffhand(val);
                ent.SlotV.SetSlotValueFDelegate?.Invoke(default);
                ent.SlotAddedV = true;
            }
        })
        .OnSecondaryPressF(() =>
        {
            var val = ent.SlotV.GetSlotValueFDelegate?.Invoke() ?? default;
            var offhand = ent.SlotV.PlayerV.GetOffhand();

            if (offhand == default && val.Count > 0)
            {
                int give = (int)Math.Ceiling(val.Count / 2f);
                ent.SlotV.PlayerV.SetOffhand(new(val.Item, give));

                if (val.Count - give > 0)
                    ent.SlotV.SetSlotValueFDelegate?.Invoke(new(val.Item, val.Count - give));
                else ent.SlotV.SetSlotValueFDelegate?.Invoke(default);

                ent.SlotAddedV = true;
            }
        })
        .OnClickF(() =>
        {
            if (!ent.SlotAddedV)
            {
                var val = ent.SlotV.GetSlotValueFDelegate?.Invoke() ?? default;
                var offhand = ent.SlotV.PlayerV.GetOffhand();

                if (val.Item == offhand.Item)
                {
                    int give = Math.Min(offhand.Count, val.Item.MaxStack - val.Count);
                    if (give > 0)
                    {
                        if (offhand.Count - give > 0)
                            offhand = new(offhand.Item, offhand.Count - give);
                        else offhand = default;

                        ent.SlotV.PlayerV.SetOffhand(offhand);
                        ent.SlotV.SetSlotValueFDelegate?.Invoke(new(val.Item, val.Count + give));
                    }
                }
                else
                {
                    ent.SlotV.SetSlotValueFDelegate?.Invoke(offhand);
                    ent.SlotV.PlayerV.SetOffhand(val);
                }
            }

            ent.SlotAddedV = false;
        })
        .OnSecondaryClickF(() =>
        {
            if (!ent.SlotAddedV)
            {
                var val = ent.SlotV.GetSlotValueFDelegate?.Invoke() ?? default;
                var offhand = ent.SlotV.PlayerV.GetOffhand();

                if (offhand.Count == 0)
                    return;

                if (val.Item == default || val.Item == offhand.Item)
                {
                    if (val.Count < offhand.Item.MaxStack)
                    {
                        ent.SlotV.SetSlotValueFDelegate?.Invoke(new(offhand.Item, val.Count + 1));

                        if (offhand.Count == 1)
                            offhand = default;
                        else offhand = new(offhand.Item, offhand.Count - 1);

                        ent.SlotV.PlayerV.SetOffhand(offhand);
                    }
                }
                else if (val.Item != offhand.Item)
                {
                    ent.SlotV.SetSlotValueFDelegate?.Invoke(offhand);
                    ent.SlotV.PlayerV.SetOffhand(val);
                }
            }

            ent.SlotAddedV = false;
        });

    public void SlotButtonInfinity(EntObj ent) => ent.Mutate()
        .Mutate(SlotButton)
        .OnPressF(() =>
        {
            var val = ent.SlotV.GetSlotValueFDelegate?.Invoke() ?? default;

            if (ent.SlotV.PlayerV.GetOffhand() == default)
            {
                ent.SlotV.PlayerV.SetOffhand(val);
                ent.SlotAddedV = true;
            }
        })
        .OnSecondaryPressF(ent.OnPressFDelegate)
        .OnClickF(() =>
        {
            if (!ent.SlotAddedV)
            {
                var val = ent.SlotV.GetSlotValueFDelegate?.Invoke() ?? default;
                var offhand = ent.SlotV.PlayerV.GetOffhand();

                if (val.Item == offhand.Item)
                {
                    if (offhand.Count < val.Item.MaxStack)
                        ent.SlotV.PlayerV.SetOffhand(new(offhand.Item, offhand.Count + 1));
                }
                else ent.SlotV.PlayerV.SetOffhand(val);
            }

            ent.SlotAddedV = false;
        })
        .OnSecondaryClickF(() =>
        {
            if (!ent.SlotAddedV)
            {
                var val = ent.SlotV.GetSlotValueFDelegate?.Invoke() ?? default;
                var offhand = ent.SlotV.PlayerV.GetOffhand();

                if (offhand.Count == 0)
                    offhand = val;
                else
                {
                    if (offhand.Count == 1)
                        offhand = default;
                    else offhand = new(offhand.Item, offhand.Count - 1);
                }

                ent.SlotV.PlayerV.SetOffhand(offhand);
            }

            ent.SlotAddedV = false;
        });
}
