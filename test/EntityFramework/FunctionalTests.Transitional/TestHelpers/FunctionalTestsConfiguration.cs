// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.SqlServerCompact;
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
            Loaded += OnLoaded;
            Loaded -= OnLoaded;

            // Now add an event that actually changes config in a verifiable way.
            // Note that OriginalConnectionFactories will be set to the DbConfiguration specified in the config file when running
            // the functional test project and set to the DbConfiguration that was set in code when running the unit tests project.
            Loaded +=
                (s, a) =>
                {
                    var currentFactory = a.DependencyResolver.GetService<IDbConnectionFactory>();
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

                    var currentProviderFactory = a.DependencyResolver.GetService<IDbProviderFactoryResolver>();
                    a.AddDependencyResolver(
                        new SingletonDependencyResolver<IDbProviderFactoryResolver>(
                            new FakeProviderFactoryResolver(currentProviderFactory))
                        , overrideConfigFile: true);

                    a.AddDependencyResolver(new FakeProviderServicesResolver(), overrideConfigFile: true);
                };
        }

        private static void OnLoaded(object sender, DbConfigurationLoadedEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public FunctionalTestsConfiguration()
        {
            SetProviderServices(SqlCeProviderServices.ProviderInvariantName, SqlCeProviderServices.Instance);
            SetProviderServices(SqlProviderServices.ProviderInvariantName, SqlProviderServices.Instance);

            SetDefaultConnectionFactory(new DefaultUnitTestsConnectionFactory());

            if (DatabaseTestHelpers.IsSqlAzure(ModelHelpers.BaseConnectionString))
            {
                SetExecutionStrategy("System.Data.SqlClient", () => new SuspendableSqlAzureExecutionStrategy());
            }
            else
            {
                SetExecutionStrategy("System.Data.SqlClient", () => new DefaultExecutionStrategy());
            }

            SetContextFactory(() => new CodeFirstScaffoldingContext("Foo"));
            SetContextFactory(() => new CodeFirstScaffoldingContextWithConnection("Bar"));

            SetMetadataAnnotationSerializer(CollationAttribute.AnnotationName, () => new CollationSerializer());

            AddInterceptor(new TestLoadedInterceptor());
            AddInterceptor(new TestLoadedInterceptor(4102, "1 yraunaJ"));
            AddInterceptor(new RegisterMutableResolver());
        }

        public static bool SuspendExecutionStrategy { get; set; }
    }
}
