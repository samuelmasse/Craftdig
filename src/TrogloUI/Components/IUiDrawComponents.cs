namespace TrogloUI;

[Components]
public interface IUiDrawComponents
{
    Vector4 ColorV { get; set; }
    Func<Vector4>? ColorF { get; set; }

    Texture? TextureV { get; set; }
    Func<Texture?>? TextureF { get; set; }

    Vector4? TintV { get; set; }
    Func<Vector4?>? TintF { get; set; }

    Alignment? TextAlignmentV { get; set; }
    Func<Alignment?>? TextAlignmentF { get; set; }

    Vector4 TextColorV { get; set; }
    Func<Vector4>? TextColorF { get; set; }

    Action<Vector2>? OnDrawF { get; set; }
    Action? OnFrameF { get; set; }

    Vector2 DrawOffsetR { get; set; }
}
