namespace Craftdig.Module;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class ModuleLoaderAttribute : InjectorAttribute;
