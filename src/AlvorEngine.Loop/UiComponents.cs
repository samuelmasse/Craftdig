namespace AlvorEngine.Loop;

[Components]
file record UiComponents(
    LazyList<EntObj> Nodes,
    LazyStack<EntObj> NodeStack,

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

    Vector2 SizeV,
    Func<Vector2>? SizeF,

    Vector2 SizeRelativeV,
    Func<Vector2>? SizeRelativeF,

    Vector2 SizeInnerMaxRelativeV,
    Func<Vector2>? SizeInnerMaxRelativeF,

    Vector2 SizeInnerSumRelativeV,
    Func<Vector2>? SizeInnerSumRelativeF,

    Vector2 SizeTextRelativeV,
    Func<Vector2>? SizeTextRelativeF,

    Vector4 ColorV,
    Func<Vector4>? ColorF,

    Font? FontV,
    Func<Font?>? FontF,

    int FontSizeV,
    Func<int>? FontSizeF,

    string TextV,
    Func<ReadOnlySpan<char>>? TextF,

    Alignment? TextAlignmentV,
    Func<Alignment?>? TextAlignmentF,

    Action? OnClickF,
    Action? OnDrawF,
    Action? OnPressF,

    [ComponentTryGet] EntObj? StackedNodeR,
    Vector2 OffsetR,
    Vector2 SizeR,
    bool IsHoveredR
);
