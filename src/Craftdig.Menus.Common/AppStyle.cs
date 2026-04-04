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
    public float ScrollStep => 65;

    public Vector2 Horizontal => (1, 0);
    public Vector2 Vertical => (0, 1);

    public Vector4 BoardColor => (1, 0, 0, 1);
    public Vector4 BoardColor2 => (1, 1, 0, 1);

    public Vector4 TextColor => (1, 1, 1, 1);
    public Vector4 TextColorFaint => (0.5f, 0.5f, 0.5f, 1);
    public Vector4 ButtonColor => (1, 0, 1, 1);
    public Vector4 ButtonColorDisabled => (0.4f, 0, 0.4f, 1);
    public Vector4 ButtonColorHovered => (1, 0.7f, 1, 1);
    public Vector4 TooltipColor => (0.5f, 0.28f, 1, 1);

    public Font Font => monocraft.Font;

    public void Text(EntMut ent) => ent.Mutate()
        .Tag(nameof(Text))
        .FontV(Font)
        .FontSizeV(FontSize)
        .FontPaddingV((ItemSpacingXS, 0, ItemSpacingXS, 0))
        .TextColorV(TextColor);

    public void Label(EntMut ent) => ent.Mutate()
        .Mutate(Text)
        .Tag(nameof(Label))
        .SizeTextRelativeV((1, 1))
        .SizeRelativeV((0, 0));

    public void InputItem(EntMut ent) => ent.Mutate()
        .SizeV((0, ItemHeight))
        .SizeRelativeV((1, 0))
        .IsSelectableV(true)
        .IsFocusableV(true)
        .Mutate((ent) =>
        {
            Node(ent)
                .AlignmentV(Alignment.Top | Alignment.Left)
                .ColorF(() => InputItemBorderColor(ent))
                .SizeV((ItemSpacingXS, 0))
                .SizeRelativeV((0, 1));

            Node(ent)
                .AlignmentV(Alignment.Top | Alignment.Right)
                .ColorF(() => InputItemBorderColor(ent))
                .SizeV((ItemSpacingXS, 0))
                .SizeRelativeV((0, 1));

            Node(ent)
                .AlignmentV(Alignment.Top | Alignment.Left)
                .ColorF(() => InputItemBorderColor(ent))
                .SizeV((0, ItemSpacingXS))
                .SizeRelativeV((1, 0));

            Node(ent)
                .AlignmentV(Alignment.Bottom | Alignment.Left)
                .ColorF(() => InputItemBorderColor(ent))
                .SizeV((0, ItemSpacingXS))
                .SizeRelativeV((1, 0));
        });

    public Vector4 InputItemBorderColor(EntMut ent)
    {
        if (Get(ent.IsInputDisabledV, ent.IsInputDisabledFDelegate))
            return (0.2f, 0.2f, 0.2f, 1f);

        if ((ent.IsFocusedR || ent.IsHoveredR))
            return (1, 1, 1, 1);

        return (0, 0, 0, 1);
    }

    public void Textbox(EntMut ent) => ent.Mutate()
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

                bool modified = false;

                if (keyboard.IsKeyPressedRepeated(Keys.Backspace) && sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                    modified = true;
                }

                if (keyboard.Text.Count > 0)
                {
                    foreach (var rune in keyboard.Text)
                    {
                        sb.Append(rune);
                        modified = true;
                    }
                }

                if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyPressed(Keys.V))
                {
                    sb.Append(keyboard.Clipboard);
                    modified = true;
                }

                if (modified)
                    ent.OnTextUpdatedDelegate?.Invoke();
            }
            else ent.WasFocusedR = false;

            if (ent.MaxLengthV > 0)
            {
                while (sb.Length > ent.MaxLengthV)
                    sb.Remove(sb.Length - 1, 1);
            }
        });

    public void Button(EntMut ent) => ent.Mutate()
        .Mutate(InputItem)
        .Mutate(Text)
        .Mutate(PointingCursor)
        .Tag(nameof(Button))
        .OnUpdateF(() =>
        {
            if (ent.IsFocusedR && keyboard.IsKeyPressedRepeated(Keys.Enter))
                ent.OnPressFDelegate?.Invoke();
        })
        .ColorF(() =>
        {
            if (Get(ent.IsInputDisabledV, ent.IsInputDisabledFDelegate))
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
                ent.OnDoubleClickFDelegate?.Invoke();
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


    public void PointingCursor(EntMut ent) => ent.Mutate()
        .CursorF(() => Get(ent.IsInputDisabledV, ent.IsInputDisabledFDelegate) ? MouseCursor.Default : MouseCursor.PointingHand);

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
        .TextureV(SlotTexture)
        .Mutate((ent) =>
        {
            ent.Nodes.Clear();

            Node(ent)
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
                });
        });

    public void SlotTooltip(EntMut ent) => ent.Mutate()
        .TooltipF(() => ent.SlotV.PlayerV.Offhand == default ?
            ent.SlotV.GetSlotValueFDelegate?.Invoke().Item.Name : null);

    public void SlotButton(EntMut ent) => ent.Mutate()
        .Tag(nameof(SlotButton))
        .IsSelectableV(true)
        .ColorF(() => ent.IsHoveredR ? (1, 1, 1, 0.5f) : default)
        .OnPressF(() =>
        {
            var val = ent.SlotV.GetSlotValueFDelegate?.Invoke() ?? default;
            var offhand = ent.SlotV.PlayerV.Offhand;

            if (ent.SlotV.PlayerV.Offhand == default)
            {
                ent.SlotV.PlayerV.Mutate().Offhand(val);
                ent.SlotV.SetSlotValueFDelegate?.Invoke(default);
                ent.SlotAddedV = true;
            }
        })
        .OnSecondaryPressF(() =>
        {
            var val = ent.SlotV.GetSlotValueFDelegate?.Invoke() ?? default;
            var offhand = ent.SlotV.PlayerV.Offhand;

            if (offhand == default && val.Count > 0)
            {
                int give = (int)Math.Ceiling(val.Count / 2f);
                ent.SlotV.PlayerV.Mutate().Offhand(new(val.Item, give));

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
                var offhand = ent.SlotV.PlayerV.Offhand;

                if (val.Item == offhand.Item)
                {
                    int give = Math.Min(offhand.Count, val.Item.MaxStack - val.Count);
                    if (give > 0)
                    {
                        if (offhand.Count - give > 0)
                            offhand = new(offhand.Item, offhand.Count - give);
                        else offhand = default;

                        ent.SlotV.PlayerV.Mutate().Offhand(offhand);
                        ent.SlotV.SetSlotValueFDelegate?.Invoke(new(val.Item, val.Count + give));
                    }
                }
                else
                {
                    ent.SlotV.SetSlotValueFDelegate?.Invoke(offhand);
                    ent.SlotV.PlayerV.Mutate().Offhand(val);
                }
            }

            ent.SlotAddedV = false;
        })
        .OnSecondaryClickF(() =>
        {
            if (!ent.SlotAddedV)
            {
                var val = ent.SlotV.GetSlotValueFDelegate?.Invoke() ?? default;
                var offhand = ent.SlotV.PlayerV.Offhand;

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

                        ent.SlotV.PlayerV.Mutate().Offhand(offhand);
                    }
                }
                else if (val.Item != offhand.Item)
                {
                    ent.SlotV.SetSlotValueFDelegate?.Invoke(offhand);
                    ent.SlotV.PlayerV.Mutate().Offhand(val);
                }
            }

            ent.SlotAddedV = false;
        });

    public void SlotButtonInfinity(EntMut ent) => ent.Mutate()
        .Mutate(SlotButton)
        .OnPressF(() =>
        {
            var val = ent.SlotV.GetSlotValueFDelegate?.Invoke() ?? default;

            if (ent.SlotV.PlayerV.Offhand == default)
            {
                ent.SlotV.PlayerV.Mutate().Offhand(val);
                ent.SlotAddedV = true;
            }
        })
        .OnSecondaryPressF(ent.OnPressFDelegate)
        .OnClickF(() =>
        {
            if (!ent.SlotAddedV)
            {
                var val = ent.SlotV.GetSlotValueFDelegate?.Invoke() ?? default;
                var offhand = ent.SlotV.PlayerV.Offhand;

                if (val.Item == offhand.Item)
                {
                    if (offhand.Count < val.Item.MaxStack)
                        ent.SlotV.PlayerV.Mutate().Offhand(new(offhand.Item, offhand.Count + 1));
                }
                else ent.SlotV.PlayerV.Mutate().Offhand(val);
            }

            ent.SlotAddedV = false;
        })
        .OnSecondaryClickF(() =>
        {
            if (!ent.SlotAddedV)
            {
                var val = ent.SlotV.GetSlotValueFDelegate?.Invoke() ?? default;
                var offhand = ent.SlotV.PlayerV.Offhand;

                if (offhand.Count == 0)
                    offhand = val;
                else
                {
                    if (offhand.Count == 1)
                        offhand = default;
                    else offhand = new(offhand.Item, offhand.Count - 1);
                }

                ent.SlotV.PlayerV.Mutate().Offhand(offhand);
            }

            ent.SlotAddedV = false;
        });
}
