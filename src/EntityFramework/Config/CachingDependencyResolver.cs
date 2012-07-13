namespace System.Data.Entity.Config
{
    using System.Collections.Concurrent;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// This class wraps another <see cref="IDbDependencyResolver"/> such that the resolutions
    /// made by that resolver are cached in a thread-safe manner.
    /// Note that Release is a no-op since this class does not handle releasing and re-caching
    /// new instances. It should not be used for anything that EF will call Release on.
    /// </summary>
    internal class CachingDependencyResolver : IDbDependencyResolver
    {
        private readonly IDbDependencyResolver _underlyingResolver;

        private readonly ConcurrentDictionary<Tuple<Type, string>, object> _resolvedDependencies 
            = new ConcurrentDictionary<Tuple<Type, string>, object>();

        public CachingDependencyResolver(IDbDependencyResolver underlyingResolver)
        {
            Contract.Requires(underlyingResolver != null);

            _underlyingResolver = underlyingResolver;
        }

        public virtual object GetService(Type type, string name)
        {
            return _resolvedDependencies.GetOrAdd(
                Tuple.Create(type, name),
                k => _underlyingResolver.GetService(type, name));
        }

        public virtual void Release(object service)
        {
        }
    }
}