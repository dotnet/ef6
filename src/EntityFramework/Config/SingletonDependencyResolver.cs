namespace System.Data.Entity.Config
{
    public class SingletonDependencyResolver<T> : IDbDependencyResolver
    {
        private readonly T _singletonInstance;
        private readonly string _name;

        public SingletonDependencyResolver(T singletonInstance)
        {
            _singletonInstance = singletonInstance;
        }

        public SingletonDependencyResolver(T singletonInstance, string name)
        {
            _singletonInstance = singletonInstance;
            _name = name;
        }

        public object Get(Type type, string name)
        {
            return type == typeof(T) && (_name == null || name == _name)
                       ? (object)_singletonInstance
                       : null;
        }

        public void Release(object service)
        {
        }
    }
}