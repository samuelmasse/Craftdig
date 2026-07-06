namespace Craftdig.Menus.Common;

[Components(SkipBuilder = true)]
public interface IAppUiTextboxComponents
{
    /// <summary>String builder backing the textbox content.</summary>
    UiProp<StringBuilder?> TextboxStringBuilderFV { get; set; }
    /// <summary>Maximum character length for the textbox.</summary>
    UiProp<int> TextboxMaxLengthFV { get; set; }
    /// <summary>Callback invoked when the textbox content changes.</summary>
    UiCallback<Action?> TextboxOnTextUpdatedFV { get; set; }

    /// <summary>Whether the textbox was focused in the previous frame.</summary>
    bool TextboxWasFocusedR { get; set; }
    /// <summary>Timestamp when the textbox received focus.</summary>
    DateTime TextboxFocusStartR { get; set; }
    /// <summary>Caret insertion index within the textbox's string.</summary>
    int TextboxCaretIndexR { get; set; }
    /// <summary>Selection anchor index within the textbox's string.</summary>
    int TextboxSelectionAnchorR { get; set; }
    /// <summary>Whether the textbox was pressed in the previous frame.</summary>
    bool TextboxWasPressedR { get; set; }
    /// <summary>Timestamp of the last mouse-down on the textbox.</summary>
    DateTime TextboxLastClickAtR { get; set; }
    /// <summary>Mouse position at the last mouse-down.</summary>
    Vec2 TextboxLastClickPositionR { get; set; }
    /// <summary>Consecutive click count on the textbox.</summary>
    int TextboxClickCountR { get; set; }
    /// <summary>Caret index where the current textbox press started.</summary>
    int TextboxPressStartIndexR { get; set; }
}
