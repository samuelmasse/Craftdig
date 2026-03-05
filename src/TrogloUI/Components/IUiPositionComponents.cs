namespace TrogloUI;

[Components]
public interface IUiPositionComponents
{
    InnerLayout InnerLayoutV { get; set; }
    Func<InnerLayout>? InnerLayoutF { get; set; }

    Alignment AlignmentV { get; set; }
    Func<Alignment>? AlignmentF { get; set; }

    Vector2 OffsetV { get; set; }
    Func<Vector2>? OffsetF { get; set; }

    Vector2 OffsetTextRelativeV { get; set; }
    Func<Vector2>? OffsetTextRelativeF { get; set; }

    float OffsetMultiplierV { get; set; }
    Func<float>? OffsetMultiplierF { get; set; }

    Vector2 OffsetR { get; set; }
}
