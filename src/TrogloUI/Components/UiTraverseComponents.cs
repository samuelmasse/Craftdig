namespace TrogloUI;

[Components]
file record UiTraverseComponents(
    LazyList<EntObj> Nodes,
    LazyStack<EntObj> NodeStack,

    bool IsDeletedV,
    Func<bool>? IsDeletedF,

    bool IsDisabledV,
    Func<bool>? IsDisabledF,

    bool IsOrderedV,
    Func<bool>? IsOrderedF,

    float OrderValueV,
    Func<float>? OrderValueF,

    EntObj? StackedNodeR,
    Memory<EntObj> NodesR);
