// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     Internal implementation for the DbConfiguration class that uses instance methods to facilitate testing
    ///     while allowing use static methods on the public API which require less dotting through.
    /// </summary>
    internal class InternalConfiguration
    {
        private CompositeResolver<ResolverChain, ResolverChain> _resolvers;
        private RootDependencyResolver _rootResolver;

        // This does not need to be volatile since it only protects against inappropriate use not
        // thread-unsafe use.
        private bool _isLocked;

        /// <summary>
        ///     Any class derived from <see cref="DbConfiguration" /> must have a public parameterless constructor
        ///     and that constructor should call this constructor.
        /// </summary>
        public InternalConfiguration()
            : this(new ResolverChain(), new ResolverChain(), new RootDependencyResolver())
        {
            _resolvers.First.Add(new AppConfigDependencyResolver(AppConfig.DefaultInstance));
        }

        public InternalConfiguration(ResolverChain appConfigChain, ResolverChain normalResolverChain, RootDependencyResolver rootResolver)
        {
            Contract.Requires(appConfigChain != null);
            Contract.Requires(normalResolverChain != null);

            _rootResolver = rootResolver;
            _resolvers = new CompositeResolver<ResolverChain, ResolverChain>(appConfigChain, normalResolverChain);
            _resolvers.Second.Add(_rootResolver);
        }

        /// <summary>
        ///     The Singleton instance of <see cref="DbConfiguration" /> for this app domain. This can be
        ///     set at application start before any Entity Framework features have been used and afterwards
        ///     should be treated as read-only.
        /// </summary>
        public static InternalConfiguration Instance
        {
            // Note that GetConfiguration and SetConfiguration on DbConfigurationManager are thread-safe.
            get { return DbConfigurationManager.Instance.GetConfiguration(); }
            set
            {
                Contract.Requires(value != null);

                DbConfigurationManager.Instance.SetConfiguration(value);
            }
        }

        public virtual void Lock()
        {
            _isLocked = true;
        }

        public virtual void AddAppConfigResolver(IDbDependencyResolver resolver)
        {
            Contract.Requires(resolver != null);

            _resolvers.First.Add(resolver);
        }

        public virtual void AddDependencyResolver(IDbDependencyResolver resolver)
        {
            Contract.Requires(resolver != null);
            CheckNotLocked("AddDependencyResolver");

            // New resolvers always run after the config resolvers so that config always wins over code
            _resolvers.Second.Add(resolver);
        }

        public virtual void RegisterSingleton<TService>(TService instance, object key)
            where TService : class
        {
            Contract.Requires(instance != null);
            CheckNotLocked("RegisterSingleton");

            AddDependencyResolver(new SingletonDependencyResolver<TService>(instance, key));
        }

        public virtual TService GetService<TService>(object key)
        {
            return _resolvers.GetService<TService>(key);
        }

        public virtual IDbDependencyResolver DependencyResolver
        {
            get { return _resolvers; }
        }

        public virtual RootDependencyResolver RootResolver
        {
            get { return _rootResolver; }
        }

        /// <summary>
        ///     This method is not thread-safe and should only be used to switch in a different root resolver
        ///     before the configuration is locked and set. It is used for pushing a new configuration by
        ///     DbContextInfo while maintaining legacy settings (such as database initializers) that are
        ///     set on the root resolver.
        /// </summary>
        public virtual void SwitchInRootResolver(RootDependencyResolver value)
        {
            Contract.Requires(value != null);

            Contract.Assert(!_isLocked);

            // The following is not thread-safe but this code is only called when pushing a configuration
            // and happens to a new DbConfiguration before it has been set and locked.
            var newChain = new ResolverChain();
            newChain.Add(value);
            _resolvers.Second.Resolvers.Skip(1).Each(newChain.Add);

            _rootResolver = value;
            _resolvers = new CompositeResolver<ResolverChain, ResolverChain>(_resolvers.First, newChain);
        }

        public virtual DbConfiguration Owner { get; set; }

        private void CheckNotLocked(string memberName)
        {
            if (_isLocked)
            {
                throw new InvalidOperationException(Strings.ConfigurationLocked(memberName));
            }
        }
    }
}
