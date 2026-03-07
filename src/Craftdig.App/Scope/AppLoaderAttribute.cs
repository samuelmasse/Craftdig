namespace Craftdig.App;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class AppLoaderAttribute : InjectorAttribute;
