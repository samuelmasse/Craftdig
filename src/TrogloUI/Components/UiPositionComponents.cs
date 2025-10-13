namespace TrogloUI;

[Components]
file record UiPositionComponents(
    InnerLayout InnerLayoutV,
    Func<InnerLayout>? InnerLayoutF,

    Alignment AlignmentV,
    Func<Alignment>? AlignmentF,

    Vector2 OffsetV,
    Func<Vector2>? OffsetF,

    Vector2 OffsetTextRelativeV,
    Func<Vector2>? OffsetTextRelativeF,

    float OffsetMultiplierV,
    Func<float>? OffsetMultiplierF,

    Vector2 OffsetR
);
