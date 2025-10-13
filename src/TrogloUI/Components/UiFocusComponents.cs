namespace TrogloUI;

[Components]
file record UiFocusComponents(
    bool IsInputDisabledV,
    Func<bool>? IsInputDisabledF,

    bool IsFocusableV,
    Func<bool>? IsFocusableF,

    bool IsFocusedR);
