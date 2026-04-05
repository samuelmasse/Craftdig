namespace TrogloUI;

[Components(SkipBuilder = true)]
public interface IUiTraverseComponents
{
    /// <summary>Whether this node is marked for deletion.</summary>
    UiProp<bool> IsDeletedFV { get; set; }
    /// <summary>Whether this node is disabled and excluded from traversal.</summary>
    UiProp<bool> IsDisabledFV { get; set; }
    /// <summary>Whether child nodes should be sorted by their order value.</summary>
    UiProp<bool> IsOrderedFV { get; set; }
    /// <summary>Sort priority when the parent has ordering enabled.</summary>
    UiProp<float> OrderValueFV { get; set; }

    /// <summary>The stacked node currently displayed from the node stack.</summary>
    EntMut StackedNodeR { get; internal set; }
    /// <summary>Compiled list of active child nodes after traversal.</summary>
    Memory<EntMut> NodesR { get; internal set; }
}
