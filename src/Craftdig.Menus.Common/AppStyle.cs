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
        if (ent.IsInputDisabledFV.Resolve())
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
        .TextF(() => text.Format("{0}{1}", ent.StringBuilderFV.Resolve(), ent.CarretR))
        .TextPaddingV((ItemSpacingXS, ItemSpacingXS, ItemSpacingXS, ItemSpacingXS))
        .CursorV(MouseCursor.IBeam)
        .OnUpdateF(() =>
        {
            ent.CarretR = string.Empty;

            var sb = ent.StringBuilderFV.Resolve();
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
                    ent.OnTextUpdatedFV.Resolve()?.Invoke();
            }
            else ent.WasFocusedR = false;

            if (ent.MaxLengthFV.Resolve() > 0)
            {
                while (sb.Length > ent.MaxLengthFV.Resolve())
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
                ent.OnPressFV.Resolve()?.Invoke();
        })
        .ColorF(() =>
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


    public void Selector(EntMut parent, RootUiMouse mouse, out EntMut list)
    {
        float scroll = 0;

        Node(parent, out var select)
            .Mutate(VerticalList)
            .InnerScrollOffsetF(() => (0, -scroll))
            .SizeInnerMaxRelativeV((1, 0))
            .AlignmentV(Alignment.Horizontal);
        list = select;

        Node(parent, out var scrollBar)
            .SizeV((20, 0))
            .SizeRelativeV((0, 1))
            .OffsetV((ItemWidthL - ItemSpacingXXL - ItemSpacingS, 0))
            .AlignmentV(Alignment.Center)
            .ColorV((0, 1, 1, 1))
            .IsDisabledF(() => parent.SizeR.Y / select.SizeInnerSumR.Y >= 1);
        {
            Vector2 pressPoint = default;
            float pressScroll = default;

            Node(scrollBar, out var scrollPuck)
                .IsSelectableV(true)
                .IsSilentFocusableV(true)
                .DeferFocusV(select)
                .CursorF(() => scrollPuck.IsPressedR ? MouseCursor.ResizeNS : MouseCursor.PointingHand)
                .ColorF(() => scrollPuck.IsPressedR ? (1, 1, 0, 1) : (0.5f, 0.5f, 1, 1))
                .OffsetF(() => (0, scroll / select.SizeInnerSumR.Y) * scrollBar.SizeR)
                .SizeRelativeF(() => (1, Math.Min(1, parent.SizeR.Y / select.SizeInnerSumR.Y)))
                .OnPressF(() =>
                {
                    pressScroll = scroll;
                    pressPoint = mouse.Position - scrollBar.DrawOffsetR;
                })
                .OnUpdateF(() =>
                {
                    if (!scrollPuck.IsPressedR)
                        return;

                    var newPoint = mouse.Position - scrollBar.DrawOffsetR;
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

    public void PointingCursor(EntMut ent) => ent.Mutate()
        .CursorF(() => ent.IsInputDisabledFV.Resolve() ? MouseCursor.Default : MouseCursor.PointingHand);

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
            NodesClear(ent);

            Node(ent)
                .Mutate(Text)
                .OffsetV((ItemSpacingXS, ItemSpacingXS))
                .SizeV((-ItemSpacingXS * 2, -ItemSpacingXS * 2))
                .TextF(() =>
                {
                    var c = ent.GetSlotValueFV.Resolve()?.Invoke() ?? default;

                    if (c == default || c.Count == 1)
                        return string.Empty;

                    return text.Format("{0}", c.Count);
                })
                .TextAlignmentV(Alignment.Bottom | Alignment.Right)
                .TextureF(() =>
                {
                    var c = ent.GetSlotValueFV.Resolve()?.Invoke() ?? default;

                    if (c.Item.IsBlock)
                        return c.Item.Faces.Front.FaceTexture;

                    return null;
                });
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
