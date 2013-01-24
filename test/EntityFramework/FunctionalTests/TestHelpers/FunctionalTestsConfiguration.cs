// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Config;
    using System.Data.Entity.Infrastructure;
    using System.Linq;

    public class FunctionalTestsConfiguration : DbConfiguration
    {
        private static readonly IList<IDbConnectionFactory> _originalConnectionFactorieses = new List<IDbConnectionFactory>();

        public static IList<IDbConnectionFactory> OriginalConnectionFactories
        {
            get { return _originalConnectionFactorieses; }
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
                        if (currentFactory != OriginalConnectionFactories.LastOrDefault())
                        {
                            OriginalConnectionFactories.Add(currentFactory);
                        }

                        a.AddDependencyResolver(DefaultConnectionFactoryResolver.Instance, overrideConfigFile: true);
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
