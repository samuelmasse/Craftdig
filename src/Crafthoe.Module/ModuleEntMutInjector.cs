namespace Crafthoe.Module;

[Module]
public class ModuleEntMutInjector(ModuleEntsMut entsMut) : InjectorCustomHandler
{
    public override bool Handles(Type type) => type.BaseType == typeof(EntMutList);

    public override object Instantiate(Type type, InjectorScopeState state, InjectorPath path)
    {
        var constructor = Constructor(type, path);

        var parameters = constructor.GetParameters();
        var parameterValues = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];

            if (parameter.ParameterType != typeof(EntMut) || parameter.Name == null)
                throw new Exception("Cannot inject non EntMut type");

            parameterValues[i] = entsMut[parameter.Name];
        }

        return constructor.Invoke(parameterValues);
    }
}
