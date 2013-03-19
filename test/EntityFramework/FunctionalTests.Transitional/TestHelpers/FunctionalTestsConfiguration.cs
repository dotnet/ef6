// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Collections.Generic;
    using System.Data.Entity.Config;
    using System.Data.Entity.Infrastructure;
    using System.Linq;

    public class FunctionalTestsConfiguration : DbConfiguration
    {
        private static volatile IList<IDbConnectionFactory> _originalConnectionFactories = new List<IDbConnectionFactory>();

        public static IList<IDbConnectionFactory> OriginalConnectionFactories
        {
            get { return _originalConnectionFactories; }
        }

        static FunctionalTestsConfiguration()
        {
            // First just a quick test that an event can be added and removed.
            OnLockingConfiguration += OnOnLockingConfiguration;
            OnLockingConfiguration -= OnOnLockingConfiguration;

            // Now add an event that actually changes config in a verifiable way.
            // Note that OriginalConnectionFactories will be set to the DbConfiguration specified in the config file when running
            // the functional test project and set to the DbConfiguration that was set in code when running the unit tests project.
            OnLockingConfiguration +=
                (s, a) =>
                    {
                        var currentFactory = a.ResolverSnapshot.GetService<IDbConnectionFactory>();
                        if (currentFactory != _originalConnectionFactories.LastOrDefault())
                        {
                            var newList = new List<IDbConnectionFactory>(_originalConnectionFactories)
                                {
                                    currentFactory
                                };
                            _originalConnectionFactories = newList;
                        }
                        a.AddDependencyResolver(
                            new SingletonDependencyResolver<IDbConnectionFactory>(
                                new SqlConnectionFactory(ModelHelpers.BaseConnectionString)), overrideConfigFile: true);

                        var currentProviderFactory = a.ResolverSnapshot.GetService<IDbProviderFactoryService>();
                        a.AddDependencyResolver(
                            new SingletonDependencyResolver<IDbProviderFactoryService>(
                                new FakeProviderFactoryService(currentProviderFactory))
                            , overrideConfigFile: true);

                        a.AddDependencyResolver(new FakeProviderServicesResolver(), overrideConfigFile: true);

                        a.AddDependencyResolver(MutableResolver.Instance, overrideConfigFile: true);
                    };
        }

        private static void OnOnLockingConfiguration(object sender, DbConfigurationEventArgs dbConfigurationEventArgs)
        {
            throw new NotImplementedException();
        }

        public FunctionalTestsConfiguration()
        {
            SetDefaultConnectionFactory(new DefaultUnitTestsConnectionFactory());
            AddDependencyResolver(new SingletonDependencyResolver<IManifestTokenService>(new FunctionalTestsManifestTokenService()));
        }
    }
}
