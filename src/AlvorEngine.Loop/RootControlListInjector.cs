
namespace AlvorEngine.Loop;

[Root]
public class RootControlListInjector(RootControls controls) : InjectorCustomHandler
{
    public override bool Handles(Type type) => type.BaseType == typeof(ControlList);

    public override object Instantiate(Type type, InjectorScopeState state, InjectorPath path)
    {
        var constructor = Constructor(type, path);

        var parameters = constructor.GetParameters();
        var parameterValues = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];

            if (parameter.ParameterType != typeof(Control) || parameter.Name == null)
                throw new Exception("Cannot inject non Control type");

            parameterValues[i] = controls[parameter.Name];
        }

        return constructor.Invoke(parameterValues);
    }
}
