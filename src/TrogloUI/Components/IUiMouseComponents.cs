namespace TrogloUI;

[Components]
public interface IUiMouseComponents
{
    bool IsSelectableV { get; set; }
    Func<bool>? IsSelectableF { get; set; }

    bool IsScrollableV { get; set; }
    Func<bool>? IsScrollableF { get; set; }

    MouseCursor? CursorV { get; set; }
    Func<MouseCursor?>? CursorF { get; set; }

    Action? OnClickF { get; set; }
    Action? OnDoubleClickF { get; set; }
    Action? OnPressF { get; set; }
    Action? OnSecondaryClickF { get; set; }
    Action? OnSecondaryPressF { get; set; }
    Action<Vector2>? OnScrollF { get; set; }

    bool IsHoveredR { get; set; }
    bool IsPressedR { get; set; }
    bool IsSecondaryPressedR { get; set; }
}
