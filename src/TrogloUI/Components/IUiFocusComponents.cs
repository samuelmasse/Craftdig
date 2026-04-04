namespace TrogloUI;

[Components]
public interface IUiFocusComponents
{
    bool IsInputDisabledV { get; set; }
    Func<bool>? IsInputDisabledF { get; set; }

    bool IsFocusableV { get; set; }
    Func<bool>? IsFocusableF { get; set; }

    bool IsSilentFocusableV { get; set; }
    Func<bool>? IsSilentFocusableF { get; set; }

    bool IsInitialFocusV { get; set; }
    Func<bool>? IsInitialFocusF { get; set; }

    EntMut DeferFocusV { get; set; }
    Func<EntMut>? DeferFocusF { get; set; }

    Action? OnFocusF { get; set; }

    bool IsFocusedR { get; set; }
}
