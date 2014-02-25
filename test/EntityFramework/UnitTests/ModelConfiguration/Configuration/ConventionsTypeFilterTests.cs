// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using Xunit;

    public class ConventionsTypeFilterTests
    {
        [Fact]
        public void IsConvention_return_true_for_default_convention()
        {
            Assert.True(new ConventionsTypeFilter().IsConvention(typeof(RegularConvention)));
        }

        [Fact]
        public void IsConvention_return_true_for_store_model_convention()
        {
            Assert.True(new ConventionsTypeFilter().IsConvention(typeof(RegularStoreModelConvention)));
        }

        [Fact]
        public void IsConvention_return_true_for_conceptual_model_convention()
        {
            Assert.True(new ConventionsTypeFilter().IsConvention(typeof(RegularConceptualModelConvention)));
        }

        [Fact]
        public void IsConvention_return_true_for_db_mapping_convention()
        {
            Assert.True(new ConventionsTypeFilter().IsConvention(typeof(RegularDbMappingConvention)));
        }

        class RegularConvention
            :Convention
        {

        }

        class RegularStoreModelConvention
            :IStoreModelConvention<EdmProperty>
        {
            public void Apply(EdmProperty item, Infrastructure.DbModel model) { }
        }

        class RegularConceptualModelConvention
            :IConceptualModelConvention<EdmProperty>
        {
            public void Apply(EdmProperty item, Infrastructure.DbModel model) { }
        }

        class RegularDbMappingConvention
            :IDbMappingConvention
        {
            public void Apply(DbDatabaseMapping databaseMapping) { }
        }
    }
}
