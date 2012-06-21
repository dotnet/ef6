namespace System.Data.Entity.Config
{
    internal class CompositeResolver<TFirst, TSecond> : IDbDependencyResolver
        where TFirst : IDbDependencyResolver
        where TSecond :IDbDependencyResolver

    {
        private readonly TFirst _firstResolver;
        private readonly TSecond _secondResolver;

        public CompositeResolver(TFirst firstResolver, TSecond secondResolver)
        {
            _firstResolver = firstResolver;
            _secondResolver = secondResolver;
        }

        public TFirst First
        {
            get { return _firstResolver; }
        }

        public TSecond Second
        {
            get { return _secondResolver; }
        }

        public virtual object Get(Type type, string name)
        {
            return _firstResolver.Get(type, name) ?? _secondResolver.Get(type, name);
        }

        public virtual void Release(object service)
        {
            _firstResolver.Release(service);
            _secondResolver.Release(service);
        }
    }
}