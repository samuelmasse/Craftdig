namespace TrogloUI;

[Components(SkipBuilder = true)]
public interface IUiDrawComponents
{
    /// <summary>Background color of the node.</summary>
    UiProp<Vector4> ColorFV { get; set; }
    /// <summary>Texture to draw on the node.</summary>
    UiProp<Texture?> TextureFV { get; set; }
    /// <summary>Sub-region position within the texture.</summary>
    UiProp<Vector2?> TextureSubPositionFV { get; set; }
    /// <summary>Sub-region size within the texture.</summary>
    UiProp<Vector2?> TextureSubSizeFV { get; set; }
    /// <summary>Sub-region size relative to the node's rendered size.</summary>
    UiProp<Vector2?> TextureSubSizeRelativeFV { get; set; }
    /// <summary>Color tint applied to the texture.</summary>
    UiProp<Vector4?> TextureColorFV { get; set; }
    /// <summary>Rotation applied to the texture.</summary>
    UiProp<SpriteBatchRotation?> TextureRotationFV { get; set; }
    /// <summary>Flip applied to the texture.</summary>
    UiProp<SpriteBatchFlip?> TextureFlipFV { get; set; }
    /// <summary>Margin around the texture (left, top, right, bottom).</summary>
    UiProp<Vector4> TextureMarginFV { get; set; }
    /// <summary>Text alignment within the node.</summary>
    UiProp<Alignment?> TextAlignmentFV { get; set; }
    /// <summary>Color of the text.</summary>
    UiProp<Vector4> TextColorFV { get; set; }
    /// <summary>Offset of the text shadow.</summary>
    UiProp<Vector2?> TextShadowOffsetFV { get; set; }
    /// <summary>Absolute color of the text shadow.</summary>
    UiProp<Vector4?> TextShadowColorFV { get; set; }
    /// <summary>Shadow color as a multiplier of the text color.</summary>
    UiProp<Vector4?> TextShadowColorRelativeFV { get; set; }
    /// <summary>Callback invoked during draw with the node's screen offset.</summary>
    UiCallback<Action?> OnDrawFV { get; set; }
    /// <summary>Callback invoked every frame.</summary>
    UiCallback<Action?> OnFrameFV { get; set; }
}
