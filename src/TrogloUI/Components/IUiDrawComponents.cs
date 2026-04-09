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
    /// <summary>Color tint applied to the texture.</summary>
    UiProp<Vector4?> TextureColorFV { get; set; }
    /// <summary>Rotation applied to the texture.</summary>
    UiProp<SpriteBatchRotation?> TextureRotationFV { get; set; }
    /// <summary>Flip applied to the texture.</summary>
    UiProp<SpriteBatchFlip?> TextureFlipFV { get; set; }
    /// <summary>Text alignment within the node.</summary>
    UiProp<Alignment?> TextAlignmentFV { get; set; }
    /// <summary>Color of the text.</summary>
    UiProp<Vector4> TextColorFV { get; set; }
    /// <summary>Callback invoked during draw with the node's screen offset.</summary>
    UiCallback<Action?> OnDrawFV { get; set; }
    /// <summary>Callback invoked every frame.</summary>
    UiCallback<Action?> OnFrameFV { get; set; }
}
