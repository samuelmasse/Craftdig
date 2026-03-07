namespace Craftdig.Server;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class ServerAttribute : InjectorAttribute;
