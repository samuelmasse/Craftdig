namespace TrogloUI;

[Components]
file record UiMouseComponents(
    bool IsSelectableV,
    Func<bool>? IsSelectableF,

    MouseCursor? CursorV,
    Func<MouseCursor?>? CursorF,

    Action? OnClickF,
    Action? OnPressF,
    Action? OnSecondaryClickF,
    Action? OnSecondaryPressF,

    bool IsHoveredR);
