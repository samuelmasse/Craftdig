namespace TrogloUI;

[Components(SkipBuilder = true)]
public interface IUiDrawComponents
{
    /// <summary>Background color of the node.</summary>
    UiProp<Vector4> ColorFV { get; set; }
    /// <summary>Texture to draw on the node.</summary>
    UiProp<Texture?> TextureFV { get; set; }
    /// <summary>Tint applied to the texture.</summary>
    UiProp<Vector4?> TintFV { get; set; }
    /// <summary>Text alignment within the node.</summary>
    UiProp<Alignment?> TextAlignmentFV { get; set; }
    /// <summary>Color of the text.</summary>
    UiProp<Vector4> TextColorFV { get; set; }
    /// <summary>Callback invoked during draw with the node's screen offset.</summary>
    UiCallback<Action<Vector2>?> OnDrawFV { get; set; }
    /// <summary>Callback invoked every frame.</summary>
    UiCallback<Action?> OnFrameFV { get; set; }

    /// <summary>Resolved draw position of this node relative to the UI root.</summary>
    Vector2 DrawOffsetR { get; internal set; }
}
