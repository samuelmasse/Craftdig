namespace TrogloUI;

[Components]
public interface IUiSizeComponents
{
    bool IsFloatingV { get; set; }
    Func<bool>? IsFloatingF { get; set; }

    bool IsPostSizedV { get; set; }
    Func<bool>? IsPostSizedF { get; set; }

    InnerSizing InnerSizingV { get; set; }
    Func<InnerSizing>? InnerSizingF { get; set; }

    SizeWeightType SizeWeightTypeV { get; set; }
    Func<SizeWeightType>? SizeWeightTypeF { get; set; }

    float InnerSpacingV { get; set; }
    Func<float>? InnerSpacingF { get; set; }

    float? SizeWeightV { get; set; }
    Func<float?>? SizeWeightF { get; set; }

    Vector4 PaddingV { get; set; }
    Func<Vector4>? PaddingF { get; set; }

    Vector2 SizeV { get; set; }
    Func<Vector2>? SizeF { get; set; }

    Vector2? SizeRelativeV { get; set; }
    Func<Vector2?>? SizeRelativeF { get; set; }

    Vector2 SizeInnerMaxRelativeV { get; set; }
    Func<Vector2>? SizeInnerMaxRelativeF { get; set; }

    Vector2 SizeInnerSumRelativeV { get; set; }
    Func<Vector2>? SizeInnerSumRelativeF { get; set; }

    Vector2 SizeTextRelativeV { get; set; }
    Func<Vector2>? SizeTextRelativeF { get; set; }

    Font? FontV { get; set; }
    Func<Font?>? FontF { get; set; }

    int FontSizeV { get; set; }
    Func<int>? FontSizeF { get; set; }

    Vector4 FontPaddingV { get; set; }
    Func<Vector4>? FontPaddingF { get; set; }

    /// <summary>Text content of the node.</summary>
    [ComponentToString] UiText TextFV { get; set; }

    Vector4 TextPaddingV { get; set; }
    Func<Vector4>? TextPaddingF { get; set; }

    Vector2 SizeR { get; set; }
    Vector2 SizeInnerSumR { get; set; }
    Vector4 PaddingR { get; set; }
    float? HorizontalWeightSizeR { get; set; }
    float? VerticalWeightSizeR { get; set; }
}
