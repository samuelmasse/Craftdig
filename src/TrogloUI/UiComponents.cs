namespace TrogloUI;

[Components]
file record UiComponents(
    // Structure
    LazyList<EntObj> Nodes,
    LazyStack<EntObj> NodeStack,

    // Flags
    bool IsDisabledV,
    Func<bool>? IsDisabledF,

    bool IsOrderedV,
    Func<bool>? IsOrderedF,

    bool IsSelectableV,
    Func<bool>? IsSelectableF,

    bool IsDeletedV,
    Func<bool>? IsDeletedF,

    bool IsPostSizedV,
    Func<bool>? IsPostSizedF,

    bool IsFloatingV,
    Func<bool>? IsFloatingF,

    bool IsInputDisabledV,
    Func<bool>? IsInputDisabledF,

    bool IsFocusableV,
    Func<bool>? IsFocusableF,

    // Layout
    InnerLayout InnerLayoutV,
    Func<InnerLayout>? InnerLayoutF,

    InnerSizing InnerSizingV,
    Func<InnerSizing>? InnerSizingF,

    float InnerSpacingV,
    Func<float>? InnerSpacingF,

    float OrderValueV,
    Func<float>? OrderValueF,

    float? SizeWeightV,
    Func<float?>? SizeWeightF,

    SizeWeightType SizeWeightTypeV,
    Func<SizeWeightType>? SizeWeightTypeF,

    Alignment AlignmentV,
    Func<Alignment>? AlignmentF,

    // Position
    Vector2 OffsetV,
    Func<Vector2>? OffsetF,

    Vector2 OffsetTextRelativeV,
    Func<Vector2>? OffsetTextRelativeF,

    float OffsetMultiplierV,
    Func<float>? OffsetMultiplierF,

    Vector4 PaddingV,
    Func<Vector4>? PaddingF,

    // Size
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

    // Visual
    Vector4 ColorV,
    Func<Vector4>? ColorF,

    Texture? TextureV,
    Func<Texture?>? TextureF,

    Vector4? TintV,
    Func<Vector4?>? TintF,

    // Text
    Font? FontV,
    Func<Font?>? FontF,

    int FontSizeV,
    Func<int>? FontSizeF,

    Vector4 FontPaddingV,
    Func<Vector4>? FontPaddingF,

    [ComponentToString] string TextV,
    Func<ReadOnlySpan<char>>? TextF,

    Alignment? TextAlignmentV,
    Func<Alignment?>? TextAlignmentF,

    Vector4 TextColorV,
    Func<Vector4>? TextColorF,

    Vector4 TextPaddingV,
    Func<Vector4>? TextPaddingF,

    // Cursor
    MouseCursor? CursorV,
    Func<MouseCursor?>? CursorF,

    // Events
    Action? OnUpdateF,
    Action? OnClickF,
    Action? OnPressF,
    Action? OnSecondaryClickF,
    Action? OnSecondaryPressF,

    // Tag
    [ComponentToString] string? TagV,

    // Runtime
    [ComponentTryGet] EntObj? StackedNodeR,
    Vector2 OffsetR,
    Vector2 SizeR,
    Vector4 PaddingR,
    bool IsHoveredR,
    bool IsFocusedR,
    float? HorizontalWeightSizeR,
    float? VerticalWeightSizeR,
    [ComponentReturnSpan] Memory<EntObj> NodesR
);
