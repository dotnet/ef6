// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    // <summary>
    // Internal implementation for the DbConfiguration class that uses instance methods to facilitate testing
    // while allowing use static methods on the public API which require less dotting through.
    // </summary>
    internal class InternalConfiguration
    {
        private CompositeResolver<ResolverChain, ResolverChain> _resolvers;
        private RootDependencyResolver _rootResolver;
        private readonly Func<DbDispatchers> _dispatchers;

        // This does not need to be volatile since it only protects against inappropriate use not
        // thread-unsafe use.
        private bool _isLocked;

        public InternalConfiguration(
            ResolverChain appConfigChain = null,
            ResolverChain normalResolverChain = null,
            RootDependencyResolver rootResolver = null,
            AppConfigDependencyResolver appConfigResolver = null,
            Func<DbDispatchers> dispatchers = null)
        {
            _rootResolver = rootResolver ?? new RootDependencyResolver();
            _resolvers = new CompositeResolver<ResolverChain, ResolverChain>(appConfigChain ?? new ResolverChain(), normalResolverChain ?? new ResolverChain());
            _resolvers.Second.Add(_rootResolver);
            _resolvers.First.Add(appConfigResolver ?? new AppConfigDependencyResolver(AppConfig.DefaultInstance, this));
            _dispatchers = dispatchers ?? (() => DbInterception.Dispatch);
        }

        // <summary>
        // The Singleton instance of <see cref="DbConfiguration" /> for this app domain. This can be
        // set at application start before any Entity Framework features have been used and afterwards
        // should be treated as read-only.
        // </summary>
        public static InternalConfiguration Instance
        {
            // Note that GetConfiguration and SetConfiguration on DbConfigurationManager are thread-safe.
            get { return DbConfigurationManager.Instance.GetConfiguration(); }
            set
            {
                DebugCheck.NotNull(value);

                DbConfigurationManager.Instance.SetConfiguration(value);
            }
        }

        public virtual void Lock()
        {
            var beforeLoadedInterceptors = DependencyResolver.GetServices<IDbInterceptor>().ToList();
            beforeLoadedInterceptors.Each(_dispatchers().AddInterceptor);

            DbConfigurationManager.Instance.OnLoaded(this);
            _isLocked = true;

            DependencyResolver
                .GetServices<IDbInterceptor>()
                .Except(beforeLoadedInterceptors)
                .Each(_dispatchers().AddInterceptor);
        }

        public void DispatchLoadedInterceptors(DbConfigurationLoadedEventArgs loadedEventArgs)
        {
            _dispatchers().Configuration.Loaded(loadedEventArgs, new DbInterceptionContext());
        }

        public virtual void AddAppConfigResolver(IDbDependencyResolver resolver)
        {
            DebugCheck.NotNull(resolver);

            _resolvers.First.Add(resolver);
        }

        public virtual void AddDependencyResolver(IDbDependencyResolver resolver, bool overrideConfigFile = false)
        {
            DebugCheck.NotNull(resolver);
            Debug.Assert(!_isLocked);

            // New resolvers always run after the config resolvers so that config always wins over code
            // unless the override flag is used, in which case we add the new resolver right at the top.
            (overrideConfigFile ? _resolvers.First : _resolvers.Second).Add(resolver);
        }

        public virtual void AddDefaultResolver(IDbDependencyResolver resolver)
        {
            DebugCheck.NotNull(resolver);

            // Default resolvers only kick in if nothing else before the root resolves the dependency.
            _rootResolver.AddDefaultResolver(resolver);
        }

        public virtual void SetDefaultProviderServices(DbProviderServices provider, string invariantName)
        {
            DebugCheck.NotNull(provider);
            DebugCheck.NotEmpty(invariantName);

            _rootResolver.SetDefaultProviderServices(provider, invariantName);
        }

        public virtual void RegisterSingleton<TService>(TService instance)
            where TService : class
        {
            DebugCheck.NotNull(instance);
            Debug.Assert(!_isLocked);

            AddDependencyResolver(new SingletonDependencyResolver<TService>(instance, (object)null));
        }

        public virtual void RegisterSingleton<TService>(TService instance, object key)
            where TService : class
        {
            DebugCheck.NotNull(instance);
            Debug.Assert(!_isLocked);

            AddDependencyResolver(new SingletonDependencyResolver<TService>(instance, key));
        }

        public virtual void RegisterSingleton<TService>(TService instance, Func<object, bool> keyPredicate)
            where TService : class
        {
            DebugCheck.NotNull(instance);
            Debug.Assert(!_isLocked);

            AddDependencyResolver(new SingletonDependencyResolver<TService>(instance, keyPredicate));
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

        // <summary>
        // This method is not thread-safe and should only be used to switch in a different root resolver
        // before the configuration is locked and set. It is used for pushing a new configuration by
        // DbContextInfo while maintaining legacy settings (such as database initializers) that are
        // set on the root resolver.
        // </summary>
        public virtual void SwitchInRootResolver(RootDependencyResolver value)
        {
            DebugCheck.NotNull(value);

            Debug.Assert(!_isLocked);

            // The following is not thread-safe but this code is only called when pushing a configuration
            // and happens to a new DbConfiguration before it has been set and locked.
            var newChain = new ResolverChain();
            newChain.Add(value);
            _resolvers.Second.Resolvers.Skip(1).Each(newChain.Add);

            _rootResolver = value;
            _resolvers = new CompositeResolver<ResolverChain, ResolverChain>(_resolvers.First, newChain);
        }

        public virtual IDbDependencyResolver ResolverSnapshot
        {
            get
            {
                var newChain = new ResolverChain();
                _resolvers.Second.Resolvers.Each(newChain.Add);
                _resolvers.First.Resolvers.Each(newChain.Add);
                return newChain;
            }
        }

        public virtual DbConfiguration Owner { get; set; }

        public virtual void CheckNotLocked(string memberName)
        {
            if (_isLocked)
            {
                throw new InvalidOperationException(Strings.ConfigurationLocked(memberName));
            }
        }
    }
}
