// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using SimpleModel;
    using Xunit;

    /// <summary>
    /// Functional tests for WriteEdmx methods.
    /// </summary>
    public class WriteEdmxTests : FunctionalTestBase
    {
        #region Infrastructure/setup

        public WriteEdmxTests()
        {
            CreateMetadataFilesForSimpleModel();
        }

        #endregion

        #region Tests for creating EDMX files from a Code First DbContext.

        [Fact]
        public void EDMX_can_be_written_before_context_is_initialized()
        {
            EDMX_can_be_written(initializeContext: false);
        }

        [Fact]
        public void EDMX_can_be_written_after_context_is_initialized()
        {
            EDMX_can_be_written(initializeContext: true);
        }

        private void EDMX_can_be_written(bool initializeContext)
        {
            var edmxBuilder = new StringBuilder();
            using (var context = new SimpleModelContext())
            {
                if (initializeContext)
                {
                    context.Database.Initialize(force: false);
                }

                EdmxWriter.WriteEdmx(context, XmlWriter.Create(edmxBuilder));

                // Quick sanity check that the context is still usable.
                Assert.NotNull(context.Products.Find(1));
            }

            SanityCheckEdmx(edmxBuilder);
        }

        [Fact]
        public void EDMX_can_be_written_multiple_times()
        {
            var edmxBuilders = new[] { new StringBuilder(), new StringBuilder(), new StringBuilder() };
            using (var context = new SimpleModelContext())
            {
                EdmxWriter.WriteEdmx(context, XmlWriter.Create(edmxBuilders[0]));

                context.Database.Initialize(force: false);

                EdmxWriter.WriteEdmx(context, XmlWriter.Create(edmxBuilders[1]));
                EdmxWriter.WriteEdmx(context, XmlWriter.Create(edmxBuilders[2]));
            }

            foreach (var builder in edmxBuilders.ToList())
            {
                SanityCheckEdmx(builder);
            }
        }

        [Fact]
        public void Edmx_can_be_written_from_a_context_created_using_DbCompiledModel()
        {
            var edmxBuilder = new StringBuilder();
            var model = SimpleModelContext.CreateBuilder().Build(ProviderRegistry.Sql2008_ProviderInfo).Compile();
            using (var context = new SimpleModelContext(model))
            {
                EdmxWriter.WriteEdmx(context, XmlWriter.Create(edmxBuilder));

                // Quick sanity check that the context is still usable.
                Assert.NotNull(context.Products.Find(1));
            }

            SanityCheckEdmx(edmxBuilder);
        }

        /// <summary>
        /// Not really testing that the EDMX is correct, just that a sanity check
        /// that the string has something in it. Testing that the EDMX is correct
        /// should be done at the EdmLib/EdmxSerializer level.
        /// </summary>
        private void SanityCheckEdmx(StringBuilder edmxBuilder)
        {
            var edmx = edmxBuilder.ToString();

            Assert.True(edmx.Contains("EntitySet Name=\"Products\""));
            Assert.True(edmx.Contains("EntitySet Name=\"Categories\""));
        }

        #endregion

        #region Tests for creating EDMX files from a DbModel.

        [Fact]
        public void EDMX_can_be_written_from_DbModel()
        {
            var edm = SimpleModelContext.CreateBuilder().Build(ProviderRegistry.Sql2008_ProviderInfo);
            var edmxBuilder = new StringBuilder();

            EdmxWriter.WriteEdmx(edm, XmlWriter.Create(edmxBuilder));

            SanityCheckEdmx(edmxBuilder);
        }

        #endregion

        #region Negative tests for cases where EDMX writing is not supported.

        [Fact]
        public void Context_based_WriteEdmx_throws_when_used_with_DbContext_created_from_existing_ObjectContext()
        {
            using (var outerContext = new SimpleModelContext())
            {
                using (var context = new SimpleModelContext(GetObjectContext(outerContext)))
                {
                    Assert.Throws<NotSupportedException>(
                        () => EdmxWriter.WriteEdmx(context, XmlWriter.Create(Stream.Null))).ValidateMessage(
                            "EdmxWriter_EdmxFromObjectContextNotSupported");
                }
            }
        }

        [Fact]
        public void Context_based_WriteEdmx_throws_when_used_with_Model_First_DbContext()
        {
            using (
                var context = new DbContext(
                    new EntityConnection(SimpleModelEntityConnectionString),
                    contextOwnsConnection: true))
            {
                Assert.Throws<NotSupportedException>(() => EdmxWriter.WriteEdmx(context, XmlWriter.Create(Stream.Null)))
                    .ValidateMessage("EdmxWriter_EdmxFromModelFirstNotSupported");
            }
        }

        #endregion

        #region Using an invalid model

        // See Dev11 Bug 151633
        [Fact]
        public void WriteEdmx_throws_when_using_bad_mapping_that_is_not_caught_by_first_pass_CSDL_validation()
        {
            using (var context = new InvalidMappingContext())
            {
                Assert.Throws<ModelValidationException>(
                    () => EdmxWriter.WriteEdmx(context, XmlWriter.Create(Stream.Null)));
            }
        }

        // See Dev11 Bug 151633
        [Fact]
        public void Compiling_the_model_throws_when_using_bad_mapping_that_is_not_caught_by_first_pass_CSDL_validation()
        {
            using (var context = new InvalidMappingContext())
            {
                Assert.Throws<ModelValidationException>(() => context.Database.Initialize(force: false));
            }
        }

        #endregion

        #region Using EdmxWriter with DbContextInfo (Dev11 324747)

        [Fact]
        public void EdmxWriter_can_write_an_EDMX_when_DbContextInfo_is_used_to_specify_the_provider_info()
        {
            var contextInfo = new DbContextInfo(
                typeof(SimpleModelContext),
                new DbProviderInfo("System.Data.SqlClient", "2008"));

            using (var context = contextInfo.CreateInstance())
            {
                var edmxBuilder = new StringBuilder();

                EdmxWriter.WriteEdmx(context, XmlWriter.Create(edmxBuilder));

                SanityCheckEdmx(edmxBuilder);
            }
        }

        #endregion
    }
}
