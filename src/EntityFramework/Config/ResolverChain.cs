namespace System.Data.Entity.Config
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Extensions;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// Chain-of-Responsibility implementation for <see cref="IDbDependencyResolver"/> instances.
    /// </summary>
    internal class ResolverChain : IDbDependencyResolver
    {
        private readonly IList<IDbDependencyResolver> _resolvers = new List<IDbDependencyResolver>();

        public virtual void Add(IDbDependencyResolver resolver)
        {
            Contract.Requires(resolver != null);

            _resolvers.Add(resolver);
        }

        public virtual object Get(Type type, string name)
        {
            return _resolvers
                .Reverse()
                .Select(r => r.Get(type, name))
                .FirstOrDefault(s => s != null);
        }

        public virtual void Release(object service)
        {
            _resolvers.Each(r => r.Release(service));
        }
    }
}