namespace TrogloUI;

[Components]
file record UiDrawComponents(
    Vector4 ColorV,
    Func<Vector4>? ColorF,

    Texture? TextureV,
    Func<Texture?>? TextureF,

    Vector4? TintV,
    Func<Vector4?>? TintF,

    Alignment? TextAlignmentV,
    Func<Alignment?>? TextAlignmentF,

    Vector4 TextColorV,
    Func<Vector4>? TextColorF
);
