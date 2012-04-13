namespace UnitTests.ProductivityAPI
{
    using System;
    using System.Configuration;
    using System.Data.Entity;
    using System.Data.Entity.Internal.ConfigFile;
    using System.Data.Entity.Resources;
    using System.IO;
    using System.Linq;
    using Xunit;

    public class EntityFrameworkSectionTests : UnitTestBase
    {
        #region Positive loading from .config file tests

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

        #endregion

        #region Negative loading from .config file tests

        [Fact]
        public void Exception_when_connection_factory_type_name_missing()
        {
            var config = CreateConfig(
@"<entityFramework>
    <defaultConnectionFactory />
</entityFramework>");

            AssertException<ConfigurationErrorsException>(() => config.GetSection("entityFramework"), " 'type' ");
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

            AssertException<ConfigurationErrorsException>(() => config.GetSection("entityFramework"), " 'value' ");
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

            AssertException<ConfigurationErrorsException>(() => config.GetSection("entityFramework"), " 'type' ");
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

            AssertException<ConfigurationErrorsException>(() => config.GetSection("entityFramework"), " 'type' ");
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

            AssertException<ConfigurationErrorsException>(() => config.GetSection("entityFramework"), Strings.ContextConfiguredMultipleTimes("MyContext"));
        }

        #endregion

        #region Helper method tests

        [Fact]
        public void ParameterElement_converts_to_valid_type()
        {
            var param = new ParameterElement(0) { ValueString="2", TypeName = "System.Int32" };
            Assert.Equal(2, param.GetTypedParameterValue());
        }

        [Fact]
        public void ParameterElement_throws_converting_to_invalid_type()
        {
            var param = new ParameterElement(0) { ValueString="MyValue", TypeName = "Not.A.Type" };
            AssertException<TypeLoadException>(() => param.GetTypedParameterValue(), " 'Not.A.Type' ");
        }

        [Fact]
        public void ParameterElement_throws_converting_to_incompatible_type()
        {
            var param = new ParameterElement(0) { ValueString = "MyValue", TypeName = "System.Int32" };
            AssertException<FormatException>(() => param.GetTypedParameterValue());
        }

        [Fact]
        public void ParameterCollection_converts_valid_types()
        {
            var coll = new ParameterCollection();
            var p1 = coll.NewElement();
            p1.ValueString = "Test";
            p1.TypeName = "System.String";
            var p2 = coll.NewElement();
            p2.ValueString = "true";
            p2.TypeName = "System.Boolean";

            var parameters = coll.GetTypedParameterValues();
            Assert.Equal(2, parameters.Count());
            Assert.Equal("Test", parameters.ElementAt(0));
            Assert.Equal(true, parameters.ElementAt(1));
        }

        [Fact]
        public void ParameterCollection_throws_converting_to_invalid_type()
        {
            var coll = new ParameterCollection();
            var p1 = coll.NewElement();
            p1.ValueString = "MyValue";
            p1.TypeName = "Not.A.Type";

            AssertException<TypeLoadException>(() => coll.GetTypedParameterValues(), " 'Not.A.Type' ");
        }

        [Fact]
        public void ParameterCollection_throws_converting_to_incompatible_type()
        {
            var coll = new ParameterCollection();
            var p1 = coll.NewElement();
            p1.ValueString = "MyValue";
            p1.TypeName = "System.Int32";

            AssertException<FormatException>(() => coll.GetTypedParameterValues());
        }

        [Fact]
        public void DatabaseInitializerElement_converts_valid_type()
        {
            var param = new DatabaseInitializerElement { InitializerTypeName = "System.Int32" };
            Assert.Equal(typeof(System.Int32), param.GetInitializerType());
        }

        [Fact]
        public void DatabaseInitializerElement_throws_converting_invalid_type()
        {
            var param = new DatabaseInitializerElement { InitializerTypeName = "Not.A.Type" };
            AssertException<TypeLoadException>(() => param.GetInitializerType(), " 'Not.A.Type' ");
        }

        [Fact]
        public void ContextElement_converts_valid_type()
        {
            var param = new ContextElement { ContextTypeName = "System.Int32" };
            Assert.Equal(typeof(System.Int32), param.GetContextType());
        }

        [Fact]
        public void ContextElement_throws_converting_invalid_type()
        {
            var param = new ContextElement { ContextTypeName = "Not.A.Type" };
            AssertException<TypeLoadException>(() => param.GetContextType(), " 'Not.A.Type' ");
        }

        #endregion

        private Configuration CreateConfig(string efSection)
        {
            var file = Path.GetTempFileName();
            File.WriteAllText(file, 
@"<?xml version='1.0' encoding='utf-8'?>
<configuration>
    <configSections>
        <section name='entityFramework' type='System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework' />
    </configSections>
" + efSection + @"
</configuration>");

            return ConfigurationManager.OpenMappedExeConfiguration(
                    new ExeConfigurationFileMap() { ExeConfigFilename = file },
                    ConfigurationUserLevel.None);
        }

        private void AssertException<TException>(Action test, string exceptionMessageSearchText = "")
            where TException : Exception
        {
            try
            {
                test();
                Assert.True(false, "Test did not throw!");
            }
            catch (TException exception)
            {
                Assert.True(exception.Message.Contains(exceptionMessageSearchText));
            }
        }
    }
}
