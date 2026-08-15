namespace Craftdig;

public partial class AppStyle
{
    private int TextboxDoubleClickWindowMs => 500;
    private int TextboxBlinkHalfPeriodMs => 500;
    private float TextboxContentVerticalInset => ItemSpacingXS * 4;
    private float TextboxCaretHorizontalShift => ItemSpacingXS;
    private float TextboxTextLeftInset => ItemSpacingXS * 2;

    public void Textbox(EntMut ent) => ent.Mutate()
        .Mutate((ent) => InputItem(ent, () => TextBoxBorderColor(ent)))
        .Mutate(Text)
        .Tag(nameof(Textbox))
        .ColorV((0, 0, 0, 1))
        .CursorV(CursorShape.Text)
        .TextAlignmentV(Alignment.Left | Alignment.Vertical)
        .TextPaddingV((ItemSpacingXS, ItemSpacingXS, ItemSpacingXS, ItemSpacingXS))
        .TextF(() => TextboxResolveDisplayText(ent))
        .Mutate(TextboxBuildSelectionHighlight)
        .Mutate(TextboxBuildCaretBar)
        .OnUpdateF(() => TextboxUpdate(ent));

    private ReadOnlySpan<char> TextboxResolveDisplayText(EntMut ent)
    {
        var sb = ent.TextboxStringBuilderFV.Resolve();
        if (sb == null)
            return default;

        if (TextboxShowEndCaret(ent, sb))
            return text.Format("{0}_", sb);

        return text.Format("{0}", sb);
    }

    private void TextboxBuildSelectionHighlight(EntMut ent)
    {
        Node(ent)
            .SizeRelativeV((0, 1))
            .ColorV((0.3f, 0.5f, 1, 0.5f))
            .IsDisabledF(() => TextboxIsSelectionHighlightHidden(ent))
            .OffsetF(() => TextboxSelectionOffset(ent))
            .SizeF(() => TextboxSelectionSize(ent));
    }

    private bool TextboxIsSelectionHighlightHidden(EntMut ent)
    {
        var sb = ent.TextboxStringBuilderFV.Resolve();
        if (sb == null)
            return true;

        if (!ent.IsFocusedR)
            return true;

        return TextboxSelectionStart(ent, sb) == TextboxSelectionEnd(ent, sb);
    }

    private Vec2 TextboxSelectionOffset(EntMut ent)
    {
        var sb = ent.TextboxStringBuilderFV.Resolve();
        if (sb == null)
            return default;

        float startX = TextboxCaretX(ent, sb, TextboxSelectionStart(ent, sb));
        return (startX, TextboxContentVerticalInset);
    }

    private Vec2 TextboxSelectionSize(EntMut ent)
    {
        var sb = ent.TextboxStringBuilderFV.Resolve();
        if (sb == null)
            return (0, -TextboxContentVerticalInset * 2);

        float startX = TextboxCaretX(ent, sb, TextboxSelectionStart(ent, sb));
        float endX = TextboxCaretX(ent, sb, TextboxSelectionEnd(ent, sb));
        return (endX - startX, -TextboxContentVerticalInset * 2);
    }

    private void TextboxBuildCaretBar(EntMut ent)
    {
        Node(ent)
            .SizeRelativeV((0, 1))
            .SizeV((ItemSpacingXS, -TextboxContentVerticalInset * 2))
            .ColorV((1, 1, 1, 1))
            .IsDisabledF(() => TextboxIsCaretBarHidden(ent))
            .OffsetF(() => TextboxCaretBarOffset(ent));
    }

    private bool TextboxIsCaretBarHidden(EntMut ent)
    {
        var sb = ent.TextboxStringBuilderFV.Resolve();
        if (sb == null)
            return true;

        if (!ent.IsFocusedR)
            return true;

        if (TextboxHasSelection(ent))
            return true;

        if (!TextboxCaretBlinkOn(ent))
            return true;

        return ent.TextboxCaretIndexR >= sb.Length;
    }

    private Vec2 TextboxCaretBarOffset(EntMut ent)
    {
        var sb = ent.TextboxStringBuilderFV.Resolve();
        if (sb == null)
            return default;

        float x = TextboxCaretX(ent, sb, Math.Clamp(ent.TextboxCaretIndexR, 0, sb.Length));
        return (x - TextboxCaretHorizontalShift, TextboxContentVerticalInset);
    }

    private void TextboxUpdate(EntMut ent)
    {
        var sb = ent.TextboxStringBuilderFV.Resolve();
        if (sb == null)
            return;

        ent.TextboxCaretIndexR = Math.Clamp(ent.TextboxCaretIndexR, 0, sb.Length);
        ent.TextboxSelectionAnchorR = Math.Clamp(ent.TextboxSelectionAnchorR, 0, sb.Length);
        TextboxEnforceMaxLength(ent, sb);

        int caretBefore = ent.TextboxCaretIndexR;
        TextboxHandleMouse(ent, sb);

        if (!ent.IsFocusedR)
        {
            ent.TextboxWasFocusedR = false;
            return;
        }

        TextboxHandleFocusGained(ent, sb);

        bool shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
        bool ctrl = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);
        TextboxHandleCaretNavigation(ent, sb, shift, ctrl);

        bool modified = false;
        modified |= TextboxHandleDeletion(ent, sb, ctrl);
        modified |= TextboxHandlePaste(ent, sb, ctrl);
        modified |= TextboxHandleCharacterInput(ent, sb, ctrl);

        if (modified)
            TextboxFinalizeModification(ent, sb);

        if (ent.TextboxCaretIndexR != caretBefore)
            TextboxResetBlink(ent);
    }

    private void TextboxHandleMouse(EntMut ent, StringBuilder sb)
    {
        if (!ent.IsPressedR)
        {
            ent.TextboxWasPressedR = false;
            return;
        }

        int hit = TextboxMouseToCaretIndex(ent, sb, mouse.Position.X);
        if (!ent.TextboxWasPressedR)
        {
            TextboxBeginMousePress(ent, sb, hit);
            ent.TextboxWasPressedR = true;
            return;
        }

        if (ent.TextboxClickCountR == 2)
        {
            // For double click, drag using entire words
            TextboxExtendWordDrag(ent, sb, hit);
            return;
        }

        if (ent.TextboxClickCountR >= 3)
        {
            // For triple click don't drag
            return;
        }

        // For single click drag by simply setting the caret
        ent.TextboxCaretIndexR = hit;
    }

    private void TextboxBeginMousePress(EntMut ent, StringBuilder sb, int hit)
    {
        var now = DateTime.UtcNow;
        bool withinTimeWindow = (now - ent.TextboxLastClickAtR).TotalMilliseconds < TextboxDoubleClickWindowMs;
        bool samePosition = mouse.Position == ent.TextboxLastClickPositionR;
        bool chainContinues = withinTimeWindow && samePosition;

        ent.TextboxClickCountR = chainContinues ? ent.TextboxClickCountR + 1 : 1;
        ent.TextboxLastClickAtR = now;
        ent.TextboxLastClickPositionR = mouse.Position;
        ent.TextboxPressStartIndexR = hit;

        if (ent.TextboxClickCountR == 2)
        {
            // Double click selects the word under the cursor
            ent.TextboxSelectionAnchorR = TextboxWordStart(sb, hit);
            ent.TextboxCaretIndexR = TextboxWordEnd(sb, hit);
            return;
        }

        if (ent.TextboxClickCountR >= 3)
        {
            // Triple click selects everything
            ent.TextboxSelectionAnchorR = 0;
            ent.TextboxCaretIndexR = sb.Length;
            return;
        }

        // Single click set caret
        ent.TextboxCaretIndexR = hit;
        ent.TextboxSelectionAnchorR = hit;
    }

    private void TextboxExtendWordDrag(EntMut ent, StringBuilder sb, int hit)
    {
        // When double clicking a world, dragging the mouse will select whole words left or right of
        // the original word, but always including the original highlighted word

        int originalWordStart = TextboxWordStart(sb, ent.TextboxPressStartIndexR);
        int originalWordEnd = TextboxWordEnd(sb, ent.TextboxPressStartIndexR);

        // Mouse is to the left of the original word
        if (hit < originalWordStart)
        {
            ent.TextboxSelectionAnchorR = originalWordEnd;
            ent.TextboxCaretIndexR = TextboxWordStart(sb, hit);
            return;
        }

        // Mouse is to the right of the original word
        if (hit > originalWordEnd)
        {
            ent.TextboxSelectionAnchorR = originalWordStart;
            ent.TextboxCaretIndexR = TextboxWordEnd(sb, hit);
            return;
        }

        // Mouse is still within the original word
        ent.TextboxSelectionAnchorR = originalWordStart;
        ent.TextboxCaretIndexR = originalWordEnd;
    }

    private void TextboxHandleFocusGained(EntMut ent, StringBuilder sb)
    {
        if (ent.TextboxWasFocusedR)
            return;

        ent.TextboxFocusStartR = DateTime.UtcNow;
        ent.TextboxWasFocusedR = true;

        // If the textbox is pressed, the mouse set the caret, don't set it again
        if (ent.IsPressedR)
            return;

        // Otherwise place caret at the end on focus
        ent.TextboxCaretIndexR = sb.Length;
        ent.TextboxSelectionAnchorR = sb.Length;
    }

    private void TextboxHandleCaretNavigation(EntMut ent, StringBuilder sb, bool shift, bool ctrl)
    {
        void MoveCaret(int newCaret)
        {
            ent.TextboxCaretIndexR = newCaret;

            if (!shift)
                ent.TextboxSelectionAnchorR = newCaret;
        }

        if (keyboard.IsKeyPressedRepeated(Keys.Left))
            MoveCaret(ctrl ? TextboxPrevWord(sb, ent.TextboxCaretIndexR) : Math.Max(0, ent.TextboxCaretIndexR - 1));

        if (keyboard.IsKeyPressedRepeated(Keys.Right))
            MoveCaret(ctrl ? TextboxNextWord(sb, ent.TextboxCaretIndexR) : Math.Min(sb.Length, ent.TextboxCaretIndexR + 1));

        if (keyboard.IsKeyPressed(Keys.Home))
            MoveCaret(0);

        if (keyboard.IsKeyPressed(Keys.End))
            MoveCaret(sb.Length);

        if (ctrl && keyboard.IsKeyPressed(Keys.A))
        {
            ent.TextboxSelectionAnchorR = 0;
            ent.TextboxCaretIndexR = sb.Length;
        }
    }

    private bool TextboxHandleDeletion(EntMut ent, StringBuilder sb, bool ctrl)
    {
        bool modified = false;

        if (keyboard.IsKeyPressedRepeated(Keys.Backspace))
            modified |= TextboxBackspaceStep(ent, sb, ctrl);

        if (keyboard.IsKeyPressedRepeated(Keys.Delete))
            modified |= TextboxDeleteStep(ent, sb, ctrl);

        return modified;
    }

    private bool TextboxBackspaceStep(EntMut ent, StringBuilder sb, bool ctrl)
    {
        if (TextboxHasSelection(ent))
        {
            TextboxDeleteSelection(ent, sb);
            return true;
        }

        if (ent.TextboxCaretIndexR == 0)
            return false;

        int removeFrom = ctrl ? TextboxPrevWord(sb, ent.TextboxCaretIndexR) : ent.TextboxCaretIndexR - 1;
        sb.Remove(removeFrom, ent.TextboxCaretIndexR - removeFrom);

        ent.TextboxCaretIndexR = removeFrom;
        ent.TextboxSelectionAnchorR = removeFrom;

        return true;
    }

    private bool TextboxDeleteStep(EntMut ent, StringBuilder sb, bool ctrl)
    {
        if (TextboxHasSelection(ent))
        {
            TextboxDeleteSelection(ent, sb);
            return true;
        }

        if (ent.TextboxCaretIndexR >= sb.Length)
            return false;

        int removeTo = ctrl ? TextboxNextWord(sb, ent.TextboxCaretIndexR) : ent.TextboxCaretIndexR + 1;
        sb.Remove(ent.TextboxCaretIndexR, removeTo - ent.TextboxCaretIndexR);

        return true;
    }

    private bool TextboxHandlePaste(EntMut ent, StringBuilder sb, bool ctrl)
    {
        if (!ctrl || !keyboard.IsKeyPressed(Keys.V))
            return false;

        if (TextboxHasSelection(ent))
            TextboxDeleteSelection(ent, sb);

        sb.Insert(ent.TextboxCaretIndexR, input.Clipboard);

        ent.TextboxCaretIndexR += input.Clipboard.Length;
        ent.TextboxSelectionAnchorR = ent.TextboxCaretIndexR;

        return true;
    }

    private bool TextboxHandleCharacterInput(EntMut ent, StringBuilder sb, bool ctrl)
    {
        if (ctrl || keyboard.Text.Count == 0)
            return false;

        if (TextboxHasSelection(ent))
            TextboxDeleteSelection(ent, sb);

        Span<char> buf = stackalloc char[2];
        foreach (var rune in keyboard.Text)
        {
            int length = rune.EncodeToUtf16(buf);
            sb.Insert(ent.TextboxCaretIndexR, buf[..length]);
            ent.TextboxCaretIndexR += length;
        }

        ent.TextboxSelectionAnchorR = ent.TextboxCaretIndexR;

        return true;
    }

    private void TextboxFinalizeModification(EntMut ent, StringBuilder sb)
    {
        TextboxEnforceMaxLength(ent, sb);
        ent.TextboxCaretIndexR = Math.Clamp(ent.TextboxCaretIndexR, 0, sb.Length);
        ent.TextboxSelectionAnchorR = Math.Clamp(ent.TextboxSelectionAnchorR, 0, sb.Length);
        ent.TextboxOnTextUpdatedFV.Resolve()?.Invoke();
    }

    private bool TextboxHasSelection(EntMut ent) => ent.TextboxCaretIndexR != ent.TextboxSelectionAnchorR;

    private bool TextboxShowEndCaret(EntMut ent, StringBuilder sb) =>
        ent.IsFocusedR
        && !TextboxHasSelection(ent)
        && TextboxCaretBlinkOn(ent)
        && ent.TextboxCaretIndexR >= sb.Length;

    private int TextboxSelectionStart(EntMut ent, StringBuilder sb) =>
        Math.Clamp(Math.Min(ent.TextboxCaretIndexR, ent.TextboxSelectionAnchorR), 0, sb.Length);

    private int TextboxSelectionEnd(EntMut ent, StringBuilder sb) =>
        Math.Clamp(Math.Max(ent.TextboxCaretIndexR, ent.TextboxSelectionAnchorR), 0, sb.Length);

    private bool TextboxCaretBlinkOn(EntMut ent)
    {
        int dt = (int)(DateTime.UtcNow - ent.TextboxFocusStartR).TotalMilliseconds;
        return (dt / TextboxBlinkHalfPeriodMs) % 2 == 0;
    }

    private void TextboxDeleteSelection(EntMut ent, StringBuilder sb)
    {
        int start = TextboxSelectionStart(ent, sb);
        int end = TextboxSelectionEnd(ent, sb);

        sb.Remove(start, end - start);

        ent.TextboxCaretIndexR = start;
        ent.TextboxSelectionAnchorR = start;
    }

    private void TextboxResetBlink(EntMut ent) => ent.TextboxFocusStartR = DateTime.UtcNow;

    private void TextboxEnforceMaxLength(EntMut ent, StringBuilder sb)
    {
        int max = ent.TextboxMaxLengthFV.Resolve();
        if (max <= 0)
            return;

        if (sb.Length <= max)
            return;

        sb.Length = max;
    }

    private int TextboxPrevWord(StringBuilder sb, int from)
    {
        int i = from;

        while (i > 0 && char.IsWhiteSpace(sb[i - 1]))
            i--;

        while (i > 0 && !char.IsWhiteSpace(sb[i - 1]))
            i--;

        return i;
    }

    private int TextboxNextWord(StringBuilder sb, int from)
    {
        int i = from;

        while (i < sb.Length && !char.IsWhiteSpace(sb[i]))
            i++;

        while (i < sb.Length && char.IsWhiteSpace(sb[i]))
            i++;

        return i;
    }

    private int TextboxWordStart(StringBuilder sb, int from)
    {
        int i = Math.Clamp(from, 0, sb.Length);

        while (i > 0 && !char.IsWhiteSpace(sb[i - 1]))
            i--;

        return i;
    }

    private int TextboxWordEnd(StringBuilder sb, int from)
    {
        int i = Math.Clamp(from, 0, sb.Length);

        while (i < sb.Length && !char.IsWhiteSpace(sb[i]))
            i++;

        return i;
    }

    private float TextboxCaretX(EntMut ent, StringBuilder sb, int index) =>
        TextboxTextLeftInset + TextboxAdvanceUntilIndex(ent, sb, index);

    private int TextboxMouseToCaretIndex(EntMut ent, StringBuilder sb, float mouseX)
    {
        float localX = mouseX - ent.PositionR.X - TextboxTextLeftInset;
        if (localX <= 0)
            return 0;

        var font = ent.FontFV.Resolve();
        if (font == null)
            return 0;

        int fontSizePx = (int)(ent.FontSizeFV.Resolve() * scale.Scale);
        if (fontSizePx <= 0)
            return 0;

        var fontSize = font.Size(fontSizePx);
        float glyphSnap = ent.TextGlyphAlignmentSnapFV.Resolve() * scale.Scale;

        float pen = 0;
        int caret = 0;

        int bestCaret = 0;
        float bestDistance = Math.Abs(localX);

        foreach (var chunk in sb.GetChunks())
        {
            foreach (var rune in chunk.Span.EnumerateRunes())
            {
                var slot = fontSize.GlyphSlot(rune);

                pen += slot.Glyph.Advance;
                caret += rune.Utf16SequenceLength;

                float caretX = Snap.Round(pen, glyphSnap) / scale.Scale;
                float distance = Math.Abs(localX - caretX);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestCaret = caret;
                }
            }
        }

        return bestCaret;
    }

    private float TextboxAdvanceUntilIndex(EntMut ent, StringBuilder sb, int index)
    {
        var font = ent.FontFV.Resolve();
        if (font == null)
            return 0;

        int fontSizePx = (int)(ent.FontSizeFV.Resolve() * scale.Scale);
        if (fontSizePx <= 0)
            return 0;

        var fontSize = font.Size(fontSizePx);
        float pen = 0;
        int pos = 0;

        foreach (var chunk in sb.GetChunks())
        {
            if (pos >= index)
                break;

            foreach (var rune in chunk.Span.EnumerateRunes())
            {
                if (pos >= index)
                    break;

                var slot = fontSize.GlyphSlot(rune);

                pen += slot.Glyph.Advance;
                pos += rune.Utf16SequenceLength;
            }
        }

        float glyphSnap = ent.TextGlyphAlignmentSnapFV.Resolve() * scale.Scale;
        return Snap.Round(pen, glyphSnap) / scale.Scale;
    }
}
