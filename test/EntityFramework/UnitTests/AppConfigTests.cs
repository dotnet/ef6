// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiUnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using Xunit;

    public class AppConfigTests : TestBase
    {
        public class GetConnectionString : TestBase
        {
            [Fact]
            public void GetConnectionString_from_running_application_config()
            {
                var conn = AppConfig.DefaultInstance.GetConnectionString("AppConfigTest");

                Assert.Equal("FromTheDefaultConfigFile", conn.ConnectionString);
            }

            [Fact]
            public void GetConnectionString_from_ConnectionStringSettingsCollection()
            {
                var strings = new ConnectionStringSettingsCollection();
                strings.Add(new ConnectionStringSettings("AppConfigTest", "FromConnectionStringSettingsCollection", ""));
                var config = new AppConfig(strings);

                var conn = config.GetConnectionString("AppConfigTest");

                Assert.Equal("FromConnectionStringSettingsCollection", conn.ConnectionString);
            }

            [Fact]
            public void GetConnectionString_from_Configuration()
            {
                var config = new AppConfig(
                    CreateEmptyConfig().AddConnectionString("AppConfigTest", "FromConfiguration"));

                var conn = config.GetConnectionString("AppConfigTest");

                Assert.Equal("FromConfiguration", conn.ConnectionString);
            }
        }

        public class GetDefaultConnectionFactory : TestBase
        {
            private const string SqlExpressBaseConnectionString =
                @"Data Source=.\SQLEXPRESS; Integrated Security=True; MultipleActiveResultSets=True";

            private const string SqlConnectionFactoryName = "System.Data.Entity.Infrastructure.SqlConnectionFactory, EntityFramework";

            [Fact]
            public void TryGetDefaultConnectionFactory_returns_null_if_no_factory_is_in_app_config()
            {
                Assert.Null(AppConfig.DefaultInstance.TryGetDefaultConnectionFactory());
            }

            [Fact]
            public void TryGetDefaultConnectionFactory_returns_null_if_no_factory_is_in_Configuration_ctor()
            {
                Assert.Null(new AppConfig(CreateEmptyConfig()).TryGetDefaultConnectionFactory());
            }

            [Fact]
            public void TryGetDefaultConnectionFactory_returns_null_when_ConnectionStringSettingsCollection_ctor_is_used()
            {
                Assert.Null(new AppConfig(new ConnectionStringSettingsCollection()).TryGetDefaultConnectionFactory());
            }

            [Fact]
            public void TryGetDefaultConnectionFactory_throws_if_factory_name_in_config_is_empty()
            {
                var config = new AppConfig(CreateEmptyConfig().AddDefaultConnectionFactory(""));
                Assert.Equal(
                    Strings.SetConnectionFactoryFromConfigFailed(""),
                    Assert.Throws<InvalidOperationException>(() => { var temp = config.TryGetDefaultConnectionFactory(); }).Message);
            }

            [Fact]
            public void TryGetDefaultConnectionFactory_throws_if_factory_name_in_config_is_whitespace()
            {
                var config = new AppConfig(CreateEmptyConfig().AddDefaultConnectionFactory(" "));
                Assert.Equal(
                    Strings.SetConnectionFactoryFromConfigFailed(" "),
                    Assert.Throws<InvalidOperationException>(() => { var temp = config.TryGetDefaultConnectionFactory(); }).Message);
            }

            [Fact]
            public void TryGetDefaultConnectionFactory_can_create_instance_of_specified_connection_factory_with_no_arguments()
            {
                var factory =
                    new AppConfig(
                        CreateEmptyConfig().AddDefaultConnectionFactory(typeof(FakeConnectionFactoryNoParams).AssemblyQualifiedName)).
                        TryGetDefaultConnectionFactory();

                Assert.IsType<FakeConnectionFactoryNoParams>(factory);
            }

            [Fact]
            public void TryGetDefaultConnectionFactory_can_create_instance_of_specified_connection_factory_with_one_argument()
            {
                var factory = new AppConfig(
                    CreateEmptyConfig().AddDefaultConnectionFactory(
                        typeof(FakeConnectionFactoryOneParam).AssemblyQualifiedName,
                        "argument 0")).TryGetDefaultConnectionFactory();

                Assert.IsType<FakeConnectionFactoryOneParam>(factory);
                Assert.Equal("argument 0", ((FakeConnectionFactoryOneParam)factory).Arg);
            }

            [Fact]
            public void TryGetDefaultConnectionFactory_can_create_instance_of_specified_connection_factory_with_multiple_arguments()
            {
                var arguments = new[] { "arg0", "arg1", "arg2", "arg3", "arg4", "arg5", "arg6", "arg7", "arg8", "arg9", "arg10" };

                var factory = new AppConfig(
                    CreateEmptyConfig().AddDefaultConnectionFactory(
                        typeof(FakeConnectionFactoryManyParams).AssemblyQualifiedName,
                        arguments)).TryGetDefaultConnectionFactory();

                Assert.IsType<FakeConnectionFactoryManyParams>(factory);
                for (var i = 0; i < arguments.Length; i++)
                {
                    Assert.Equal(arguments[i], ((FakeConnectionFactoryManyParams)factory).Args[i]);
                }
            }

            [Fact]
            public void TryGetDefaultConnectionFactory_can_handle_empty_string_arguments()
            {
                var factory =
                    new AppConfig(
                        CreateEmptyConfig().AddDefaultConnectionFactory(typeof(FakeConnectionFactoryOneParam).AssemblyQualifiedName, "")).
                        TryGetDefaultConnectionFactory();

                Assert.IsType<FakeConnectionFactoryOneParam>(factory);
                Assert.Equal("", ((FakeConnectionFactoryOneParam)factory).Arg);
            }

            [Fact]
            public void TryGetDefaultConnectionFactory_throws_if_factory_type_cannot_be_found()
            {
                var config = new AppConfig(CreateEmptyConfig().AddDefaultConnectionFactory("BogusFactory"));

                Assert.Equal(
                    Strings.SetConnectionFactoryFromConfigFailed("BogusFactory"),
                    Assert.Throws<InvalidOperationException>(() => { var temp = config.TryGetDefaultConnectionFactory(); }).Message);
            }

            [Fact]
            public void TryGetDefaultConnectionFactory_throws_if_empty_constructor_is_required_but_not_available()
            {
                var config =
                    new AppConfig(
                        CreateEmptyConfig().AddDefaultConnectionFactory(typeof(FakeConnectionFactoryOneParam).AssemblyQualifiedName));

                Assert.Equal(
                    Strings.SetConnectionFactoryFromConfigFailed(typeof(FakeConnectionFactoryOneParam).AssemblyQualifiedName),
                    Assert.Throws<InvalidOperationException>(() => { var temp = config.TryGetDefaultConnectionFactory(); }).Message);
            }

            [Fact]
            public void TryGetDefaultConnectionFactory_throws_if_constructor_with_parameters_is_required_but_not_available()
            {
                var config =
                    new AppConfig(
                        CreateEmptyConfig().AddDefaultConnectionFactory(typeof(FakeConnectionFactoryNoParams).AssemblyQualifiedName, ""));

                Assert.Equal(
                    Strings.SetConnectionFactoryFromConfigFailed(typeof(FakeConnectionFactoryNoParams).AssemblyQualifiedName),
                    Assert.Throws<InvalidOperationException>(() => { var temp = config.TryGetDefaultConnectionFactory(); }).Message);
            }

            [Fact]
            public void TryGetDefaultConnectionFactory_throws_if_factory_type_does_not_implement_IDbConnectionFactory()
            {
                var config =
                    new AppConfig(CreateEmptyConfig().AddDefaultConnectionFactory(typeof(FakeNonConnectionFactory).AssemblyQualifiedName));

                Assert.Equal(
                    Strings.SetConnectionFactoryFromConfigFailed(typeof(FakeNonConnectionFactory).AssemblyQualifiedName),
                    Assert.Throws<InvalidOperationException>(() => { var temp = config.TryGetDefaultConnectionFactory(); }).Message);
            }

            [Fact]
            public void TryGetDefaultConnectionFactory_throws_if_factory_type_is_Abstract()
            {
                var config =
                    new AppConfig(CreateEmptyConfig().AddDefaultConnectionFactory(typeof(FakeBaseConnectionFactory).AssemblyQualifiedName));

                Assert.Equal(
                    Strings.SetConnectionFactoryFromConfigFailed(typeof(FakeBaseConnectionFactory).AssemblyQualifiedName),
                    Assert.Throws<InvalidOperationException>(() => { var temp = config.TryGetDefaultConnectionFactory(); }).Message);
            }

            [Fact]
            public void TryGetDefaultConnectionFactory_can_be_used_to_create_instance_of_SqlConnectionFactory()
            {
                var factory =
                    new AppConfig(CreateEmptyConfig().AddDefaultConnectionFactory(SqlConnectionFactoryName, "Some Connection String")).
                        TryGetDefaultConnectionFactory();

                Assert.IsType<SqlConnectionFactory>(factory);
                Assert.Equal("Some Connection String", ((SqlConnectionFactory)factory).BaseConnectionString);
            }

            [Fact]
            public void
                TryGetDefaultConnectionFactory_can_be_used_to_create_instance_of_SqlCEConnectionFactory_with_just_provider_invariant_name()
            {
                var factory = new AppConfig(
                    CreateEmptyConfig().AddDefaultConnectionFactory(
                        "System.Data.Entity.Infrastructure.SqlCeConnectionFactory, EntityFramework",
                        "Provider Invariant Name")).TryGetDefaultConnectionFactory();

                Assert.IsType<SqlCeConnectionFactory>(factory);
                Assert.Equal("Provider Invariant Name", ((SqlCeConnectionFactory)factory).ProviderInvariantName);
                Assert.Equal(new SqlCeConnectionFactory("PIN").DatabaseDirectory, ((SqlCeConnectionFactory)factory).DatabaseDirectory);
                Assert.Equal(new SqlCeConnectionFactory("PIN").BaseConnectionString, ((SqlCeConnectionFactory)factory).BaseConnectionString);
            }

            [Fact]
            public void
                TryGetDefaultConnectionFactory_can_be_used_to_create_instance_of_SqlCEConnectionFactory_with_provider_invariant_name_and_other_arguments
                ()
            {
                var factory = new AppConfig(
                    CreateEmptyConfig().AddDefaultConnectionFactory(
                        "System.Data.Entity.Infrastructure.SqlCeConnectionFactory, EntityFramework",
                        "Provider Invariant Name",
                        "Database Directory",
                        "Base Connection String")).TryGetDefaultConnectionFactory();

                Assert.IsType<SqlCeConnectionFactory>(factory);
                Assert.Equal("Provider Invariant Name", ((SqlCeConnectionFactory)factory).ProviderInvariantName);
                Assert.Equal("Database Directory", ((SqlCeConnectionFactory)factory).DatabaseDirectory);
                Assert.Equal("Base Connection String", ((SqlCeConnectionFactory)factory).BaseConnectionString);
            }
        }
    }

    #region Fake connection factories

    public abstract class FakeBaseConnectionFactory : IDbConnectionFactory
    {
        public DbConnection CreateConnection(string nameOrConnectionString)
        {
            throw new NotImplementedException();
        }
    }

    public class FakeConnectionFactoryNoParams : FakeBaseConnectionFactory
    {
    }

    public class FakeConnectionFactoryOneParam : FakeBaseConnectionFactory
    {
        public string Arg { get; set; }

        public FakeConnectionFactoryOneParam(string arg)
        {
            Arg = arg;
        }
    }

    public class FakeConnectionFactoryManyParams : FakeBaseConnectionFactory
    {
        public List<string> Args { get; set; }

        public FakeConnectionFactoryManyParams(
            string arg0, string arg1, string arg2, string arg3, string arg4,
            string arg5, string arg6, string arg7, string arg8, string arg9,
            string arg10)
        {
            Args = new List<string>
                       {
                           arg0,
                           arg1,
                           arg2,
                           arg3,
                           arg4,
                           arg5,
                           arg6,
                           arg7,
                           arg8,
                           arg9,
                           arg10
                       };
        }
    }

    public class FakeNonConnectionFactory
    {
    }

    #endregion

    #region Fake Initializers

    public enum FakeInitializerCtor
    {
        NoArgs,
        OneArg,
        TwoArgs
    }

    public class GenericFakeInitializerForAppConfig<T> : IDatabaseInitializer<T>
        where T : DbContext
    {
        public GenericFakeInitializerForAppConfig()
        {
            Reset();
            CtorCalled = FakeInitializerCtor.NoArgs;
        }

        public GenericFakeInitializerForAppConfig(string arg1)
        {
            Reset();
            CtorCalled = FakeInitializerCtor.OneArg;
            Arg1 = arg1;
        }

        public GenericFakeInitializerForAppConfig(string arg1, string arg2)
        {
            Reset();
            CtorCalled = FakeInitializerCtor.TwoArgs;
            Arg1 = arg1;
            Arg2 = arg2;
        }

        public static void Reset()
        {
            InitializerCalled = false;
            CtorCalled = null;
            Arg1 = null;
            Arg2 = null;
        }

        public void InitializeDatabase(T context)
        {
            InitializerCalled = true;
        }

        public static bool InitializerCalled { get; set; }

        public static FakeInitializerCtor? CtorCalled { get; set; }

        public static string Arg1 { get; set; }

        public static string Arg2 { get; set; }
    }

    public class FakeForAppConfig : DbContextUsingMockInternalContext
    {
    }

    public class FakeInitializerForAppConfig : IDatabaseInitializer<FakeForAppConfig>
    {
        public static bool InitializerCalled { get; set; }

        public static void Reset()
        {
            InitializerCalled = false;
        }

        public void InitializeDatabase(FakeForAppConfig context)
        {
            InitializerCalled = true;
        }
    }

    /// <summary>
    ///     Only use this for LegacyDatabaseInitializerConfig tests.
    /// </summary>
    public class FakeForLegacyAppConfig : DbContextUsingMockInternalContext
    {
    }

    public class FakeInitializerForLegacyAppConfig : IDatabaseInitializer<FakeForLegacyAppConfig>
    {
        public FakeInitializerForLegacyAppConfig()
        {
            Reset();
            CtorCalled = FakeInitializerCtor.NoArgs;
        }

        public FakeInitializerForLegacyAppConfig(string arg1)
        {
            Reset();
            CtorCalled = FakeInitializerCtor.OneArg;
            Arg1 = arg1;
        }

        public FakeInitializerForLegacyAppConfig(string arg1, string arg2)
        {
            Reset();
            CtorCalled = FakeInitializerCtor.TwoArgs;
            Arg1 = arg1;
            Arg2 = arg2;
        }

        public static void Reset()
        {
            InitializerCalled = false;
            CtorCalled = null;
            Arg1 = null;
            Arg2 = null;
        }

        public void InitializeDatabase(FakeForLegacyAppConfig context)
        {
            InitializerCalled = true;
        }

        public static bool InitializerCalled { get; set; }

        public static FakeInitializerCtor? CtorCalled { get; set; }

        public static string Arg1 { get; set; }

        public static string Arg2 { get; set; }
    }

    public class FakeForAppConfigNonStringParams : DbContextUsingMockInternalContext
    {
    }

    public class FakeInitializerForAppConfigNonStringParams : IDatabaseInitializer<FakeForAppConfigNonStringParams>
    {
        public FakeInitializerForAppConfigNonStringParams(int arg1)
        {
            Arg1 = arg1;
        }

        public FakeInitializerForAppConfigNonStringParams(int arg1, string arg2)
        {
            Arg1 = arg1;
            Arg2 = arg2;
        }

        public void InitializeDatabase(FakeForAppConfigNonStringParams context)
        {
            InitializerCalled = true;
        }

        public static bool InitializerCalled { get; set; }

        public static int? Arg1 { get; set; }

        public static string Arg2 { get; set; }

        public static void Reset()
        {
            InitializerCalled = false;
            Arg1 = null;
            Arg2 = null;
        }
    }

    public class FakeForAppConfigGeneric<T> : DbContextUsingMockInternalContext
    {
    }

    public class FakeInitializerForAppConfigGeneric<T> : IDatabaseInitializer<FakeForAppConfigGeneric<T>>
    {
        public void InitializeDatabase(FakeForAppConfigGeneric<T> context)
        {
            InitializerCalled = true;
        }

        public static bool InitializerCalled { get; set; }

        public static void Reset()
        {
            InitializerCalled = false;
        }
    }

    public class FakeInitializerWithMissingConstructor : IDatabaseInitializer<FakeForLegacyAppConfig>
    {
        public FakeInitializerWithMissingConstructor(int _)
        {
        }

        public void InitializeDatabase(FakeForLegacyAppConfig context)
        {
        }
    }

    #endregion
}
