namespace TrogloUI;

[Components]
file record UiSizeComponents(
    bool IsFloatingV,
    Func<bool>? IsFloatingF,

    bool IsPostSizedV,
    Func<bool>? IsPostSizedF,

    InnerSizing InnerSizingV,
    Func<InnerSizing>? InnerSizingF,

    SizeWeightType SizeWeightTypeV,
    Func<SizeWeightType>? SizeWeightTypeF,

    float InnerSpacingV,
    Func<float>? InnerSpacingF,

    float? SizeWeightV,
    Func<float?>? SizeWeightF,

    Vector4 PaddingV,
    Func<Vector4>? PaddingF,

    Vector2 SizeV,
    Func<Vector2>? SizeF,

    Vector2? SizeRelativeV,
    Func<Vector2?>? SizeRelativeF,

    Vector2 SizeInnerMaxRelativeV,
    Func<Vector2>? SizeInnerMaxRelativeF,

    Vector2 SizeInnerSumRelativeV,
    Func<Vector2>? SizeInnerSumRelativeF,

    Vector2 SizeTextRelativeV,
    Func<Vector2>? SizeTextRelativeF,

    Font? FontV,
    Func<Font?>? FontF,

    int FontSizeV,
    Func<int>? FontSizeF,

    Vector4 FontPaddingV,
    Func<Vector4>? FontPaddingF,

    [ComponentToString] string TextV,
    Func<ReadOnlySpan<char>>? TextF,

    Vector4 TextPaddingV,
    Func<Vector4>? TextPaddingF,

    Vector2 SizeR,
    Vector4 PaddingR,
    float? HorizontalWeightSizeR,
    float? VerticalWeightSizeR);
