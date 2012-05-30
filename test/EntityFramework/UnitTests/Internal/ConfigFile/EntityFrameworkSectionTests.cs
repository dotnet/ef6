namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Configuration;
    using System.Data.Entity.Resources;
    using System.IO;
    using System.Linq;
    using Xunit;

    public class EntityFrameworkSectionTests : TestBase
    {
        public class DefaultConnectionFactory
        {
            [Fact]
            public void Can_load_empty_section()
            {
                var config = CreateConfig(@"<entityFramework />");

                var ef = (EntityFrameworkSection)config.GetSection("entityFramework");

                Assert.NotNull(ef);
                Assert.False(ef.DefaultConnectionFactory.ElementInformation.IsPresent);
                Assert.Equal(0, ef.Contexts.Count);
            }

            [Fact]
            public void Can_load_default_connection_factory_section()
            {
                var config = CreateConfig(
                    @"<entityFramework>
                         <defaultConnectionFactory type='FactoryType' />
                      </entityFramework>");

                var ef = (EntityFrameworkSection)config.GetSection("entityFramework");

                Assert.True(ef.DefaultConnectionFactory.ElementInformation.IsPresent);
                Assert.Equal("FactoryType", ef.DefaultConnectionFactory.FactoryTypeName);
            }

            [Fact]
            public void Can_load_default_connection_factory_with_parameters_section()
            {
                var config = CreateConfig(
                    @"<entityFramework>
                          <defaultConnectionFactory type='FactoryType'>
                              <parameters>
                                  <parameter value='MyString' />
                                  <parameter value='2' type='System.Int32' />
                              </parameters>
                          </defaultConnectionFactory>
                      </entityFramework>");

                var ef = (EntityFrameworkSection)config.GetSection("entityFramework");

                Assert.Equal(2, ef.DefaultConnectionFactory.Parameters.Count);
                var p1 = ef.DefaultConnectionFactory.Parameters.Cast<ParameterElement>().ElementAt(0);
                var p2 = ef.DefaultConnectionFactory.Parameters.Cast<ParameterElement>().ElementAt(1);
                Assert.Equal("MyString", p1.ValueString);
                Assert.Equal("System.String", p1.TypeName);
                Assert.Equal("2", p2.ValueString);
                Assert.Equal("System.Int32", p2.TypeName);
            }

            [Fact]
            public void Parameter_type_defaults_to_string()
            {
                var config = CreateConfig(
                    @"<entityFramework>
                          <defaultConnectionFactory type='FactoryType'>
                              <parameters>
                                  <parameter value='ABC' />
                              </parameters>
                          </defaultConnectionFactory>
                      </entityFramework>");

                var ef = (EntityFrameworkSection)config.GetSection("entityFramework");
                var p1 = ef.DefaultConnectionFactory.Parameters.Cast<ParameterElement>().ElementAt(0);
                Assert.Equal("System.String", p1.TypeName);
            }

            [Fact]
            public void Exception_when_connection_factory_type_name_missing()
            {
                var config = CreateConfig(
                    @"<entityFramework>
                          <defaultConnectionFactory />
                      </entityFramework>");

                Assert.True(
                    Assert.Throws<ConfigurationErrorsException>(() => config.GetSection("entityFramework")).Message.Contains(" 'type' "));
            }

            [Fact]
            public void Exception_when_parameter_value_missing()
            {
                var config = CreateConfig(
                    @"<entityFramework>
                          <defaultConnectionFactory type='FactoryType'>
                              <parameters>
                                  <parameter />
                              </parameters>
                          </defaultConnectionFactory>
                      </entityFramework>");

                Assert.True(
                    Assert.Throws<ConfigurationErrorsException>(() => config.GetSection("entityFramework")).Message.Contains(" 'value' "));
            }
        }

        public class Contexts
        {
            [Fact]
            public void Can_load_context_section()
            {
                var config = CreateConfig(
                    @"<entityFramework>
                          <contexts>
                              <context type='ContextType' />
                          </contexts>
                      </entityFramework>");

                var ef = (EntityFrameworkSection)config.GetSection("entityFramework");

                Assert.Equal(1, ef.Contexts.Count);
                var ctx = ef.Contexts.Cast<ContextElement>().Single();
                Assert.Equal("ContextType", ctx.ContextTypeName);
                Assert.False(ctx.IsDatabaseInitializationDisabled);
                Assert.False(ctx.DatabaseInitializer.ElementInformation.IsPresent);
            }

            [Fact]
            public void Can_load_context_section_with_database_initializer_disabled()
            {
                var config = CreateConfig(
                    @"<entityFramework>
                          <contexts>
                              <context type='ContextType' disableDatabaseInitialization='true' />
                          </contexts>
                      </entityFramework>");

                var ef = (EntityFrameworkSection)config.GetSection("entityFramework");

                var ctx = ef.Contexts.Cast<ContextElement>().Single();
                Assert.True(ctx.IsDatabaseInitializationDisabled);
            }

            [Fact]
            public void Can_load_context_section_with_database_initializer_specified()
            {
                var config = CreateConfig(
                    @"<entityFramework>
                          <contexts>
                              <context type='ContextType'>
                                  <databaseInitializer type='InitializerType' />
                              </context>
                          </contexts>
                      </entityFramework>");

                var ef = (EntityFrameworkSection)config.GetSection("entityFramework");

                var ctx = ef.Contexts.Cast<ContextElement>().Single();
                Assert.True(ctx.DatabaseInitializer.ElementInformation.IsPresent);
                Assert.Equal("InitializerType", ctx.DatabaseInitializer.InitializerTypeName);
                Assert.Equal(0, ctx.DatabaseInitializer.Parameters.Count);
            }

            [Fact]
            public void Can_load_context_section_with_database_initializer_specified_with_parameters()
            {
                var config = CreateConfig(
                    @"<entityFramework>
                          <contexts>
                              <context type='ContextType'>
                                  <databaseInitializer type='InitializerType'>
                                      <parameters>
                                          <parameter value='MyString' />
                                          <parameter value='2' type='System.Int32' />
                                      </parameters>
                                  </databaseInitializer>
                              </context>
                          </contexts>
                      </entityFramework>");

                var ef = (EntityFrameworkSection)config.GetSection("entityFramework");

                var ctx = ef.Contexts.Cast<ContextElement>().Single();

                Assert.Equal(2, ctx.DatabaseInitializer.Parameters.Count);
                var p1 = ctx.DatabaseInitializer.Parameters.Cast<ParameterElement>().ElementAt(0);
                var p2 = ctx.DatabaseInitializer.Parameters.Cast<ParameterElement>().ElementAt(1);
                Assert.Equal("MyString", p1.ValueString);
                Assert.Equal("System.String", p1.TypeName);
                Assert.Equal("2", p2.ValueString);
                Assert.Equal("System.Int32", p2.TypeName);
            }

            [Fact]
            public void Exception_when_context_type_name_missing()
            {
                var config = CreateConfig(
                    @"<entityFramework>
                          <contexts>
                              <context />
                          </contexts>
                      </entityFramework>");

                Assert.True(
                    Assert.Throws<ConfigurationErrorsException>(() => config.GetSection("entityFramework")).Message.Contains(" 'type' "));
            }

            [Fact]
            public void Exception_when_database_initializer_type_name_missing()
            {
                var config = CreateConfig(
                    @"<entityFramework>
                          <contexts>
                              <context type='ContextType'>
                                  <databaseInitializer />
                              </context>
                          </contexts>
                      </entityFramework>");

                Assert.True(
                    Assert.Throws<ConfigurationErrorsException>(() => config.GetSection("entityFramework")).Message.Contains(" 'type' "));
            }

            [Fact]
            public void Exception_when_same_context_configured_twice()
            {
                var config = CreateConfig(
                    @"<entityFramework>
                          <contexts>
                              <context type='MyContext' />
                              <context type='MyContext' />
                          </contexts>
                      </entityFramework>");

                Assert.True(
                    Assert.Throws<ConfigurationErrorsException>(() => config.GetSection("entityFramework")).Message.Contains(
                        Strings.ContextConfiguredMultipleTimes("MyContext")));
            }
        }

        public class Providers
        {
            [Fact]
            public void Providers_returns_the_configured_EF_providers()
            {
                var config = CreateConfig(
                    @"<entityFramework>
                        <providers>
                          <provider invariantName='My.Invariant1' type='MyProvider1'/>
                          <provider invariantName='My.Invariant2' type='MyProvider2'/>
                          <provider invariantName='My.Invariant3' type='MyProvider1'/>
                        </providers>
                      </entityFramework>");

                var ef = (EntityFrameworkSection)config.GetSection("entityFramework");

                Assert.Equal(3, ef.Providers.Count);

                Assert.Equal(
                    "MyProvider1",
                    ef.Providers.OfType<ProviderElement>().Single(p => p.InvariantName == "My.Invariant1").ProviderTypeName);
                
                Assert.Equal(
                    "MyProvider2", 
                    ef.Providers.OfType<ProviderElement>().Single(p => p.InvariantName == "My.Invariant2").ProviderTypeName);
                
                Assert.Equal(
                    "MyProvider1", 
                    ef.Providers.OfType<ProviderElement>().Single(p => p.InvariantName == "My.Invariant3").ProviderTypeName);
            }

            [Fact]
            public void Reading_config_throws_if_invariant_name_is_repeated()
            {
                var config = CreateConfig(
                    @"<entityFramework>
                        <providers>
                          <provider invariantName='My.Invariant1' type='MyProvider1'/>
                          <provider invariantName='My.Invariant2' type='MyProvider2'/>
                          <provider invariantName='My.Invariant1' type='MyProvider3'/>
                        </providers>
                      </entityFramework>");

                Assert.True(
                    Assert.Throws<ConfigurationErrorsException>(() => config.GetSection("entityFramework")).Message.Contains(
                        Strings.ProviderInvariantRepeatedInConfig("My.Invariant1")));
            }
        }

        private static Configuration CreateConfig(string efSection)
        {
            var file = Path.GetTempFileName();
            File.WriteAllText(
                file,
                @"<?xml version='1.0' encoding='utf-8'?>
                <configuration>
                    <configSections>
                        <section name='entityFramework' type='System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework' />
                    </configSections>
                " + efSection + @"
                </configuration>");

            return ConfigurationManager.OpenMappedExeConfiguration(
                new ExeConfigurationFileMap()
                    {
                        ExeConfigFilename = file
                    },
                ConfigurationUserLevel.None);
        }
    }
}
