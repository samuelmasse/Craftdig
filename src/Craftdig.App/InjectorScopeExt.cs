namespace Craftdig.App;

public static class InjectorScopeExt
{
    extension<T>(T injector) where T : InjectorScope
    {
        public T Run(Action<T> action)
        {
            action.Invoke(injector);
            return injector;
        }

        public T With(object instance)
        {
            injector.Add(instance);
            return injector;
        }

        public T With(Func<T, object> func)
        {
            injector.Add(func.Invoke(injector));
            return injector;
        }
    }
}
