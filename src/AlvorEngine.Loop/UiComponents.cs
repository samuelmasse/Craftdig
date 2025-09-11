namespace AlvorEngine.Loop;

[Components]
file record UiComponents(
    LazyList<EntObj> Nodes,
    LazyStack<EntObj> NodeStack,

    bool IsDisabledV,
    Func<bool>? IsDisabledF,

    bool IsOrderedV,
    Func<bool>? IsOrderedF,

    float OrderValueV,
    Func<float>? OrderValueF,

    bool IsSelectableV,
    Func<bool>? IsSelectableF,

    bool IsDeletedV,
    Func<bool>? IsDeletedF,

    InnerLayout InnerLayoutV,
    Func<InnerLayout>? InnerLayoutF,

    float InnerSpacingV,
    Func<float>? InnerSpacingF,

    InnerSizing InnerSizingV,
    Func<InnerSizing>? InnerSizingF,

    float? SizeWeightV,
    Func<float?>? SizeWeightF,

    Vector2 OffsetV,
    Func<Vector2>? OffsetF,

    Alignment AlignmentV,
    Func<Alignment>? AlignmentF,

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

    Vector4 ColorV,
    Func<Vector4>? ColorF,

    Texture? TextureV,
    Func<Texture?>? TextureF,

    Vector4? TintV,
    Func<Vector4?>? TintF,

    Font? FontV,
    Func<Font?>? FontF,

    int FontSizeV,
    Func<int>? FontSizeF,

    [ComponentToString] string TextV,
    Func<ReadOnlySpan<char>>? TextF,

    Alignment? TextAlignmentV,
    Func<Alignment?>? TextAlignmentF,

    Vector4 TextColorV,
    Func<Vector4>? TextColorF,

    Action? OnUpdateF,
    Action? OnClickF,
    Action? OnPressF,
    Action? OnSecondaryClickF,
    Action? OnSecondaryPressF,

    [ComponentToString] string? TagV,

    [ComponentTryGet] EntObj? StackedNodeR,
    Vector2 OffsetR,
    Vector2 SizeR,
    Vector4 PaddingR,
    bool IsHoveredR,
    [ComponentReturnSpan] Memory<EntObj> NodesR
);
