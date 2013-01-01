// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using Xunit;

    public class EdmItemCollectionTests
    {
        [Fact]
        public void Can_initialize_from_edm_model_and_items_set_read_only()
        {
            var context = new ShopContext_v1();
            var compiledModel = context.InternalContext.CodeFirstModel;

            var builder = compiledModel.CachedModelBuilder.Clone();

            var databaseMapping
                = builder.Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping;

            var itemCollection = new EdmItemCollection(databaseMapping.Model);

            Assert.Equal(3.0, itemCollection.EdmVersion);

            foreach (var globalItem in databaseMapping.Model.GlobalItems)
            {
                Assert.True(itemCollection.Contains(globalItem));
                Assert.True(globalItem.IsReadOnly);
            }
        }

        // TODO: METADATA: Rework these...

        [Fact]
        public void Code_first_built_entities_matches_som_loaded_entities()
        {
            var context = new ShopContext_v1();
            var compiledModel = context.InternalContext.CodeFirstModel;

            var builder = compiledModel.CachedModelBuilder.Clone();

            var databaseMapping
                = builder.Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping;

            var itemCollection = new EdmItemCollection(databaseMapping.Model);

            var entities = databaseMapping.Model.EntityTypes.ToList();
            var somEntities = itemCollection.GetItems<EntityType>();

            Assert.Equal(entities.Count(), somEntities.Count());

            foreach (var entityType in entities)
            {
                var somEntityType = somEntities.Single(e => e.Name == entityType.Name);

                Assert.Equal(entityType.NamespaceName, somEntityType.NamespaceName);
                Assert.Equal(entityType.Identity, somEntityType.Identity);
                Assert.Equal(entityType.Abstract, somEntityType.Abstract);
                Assert.Equal(entityType.FullName, somEntityType.FullName);
                Assert.Equal(entityType.KeyMembers.Count, somEntityType.KeyMembers.Count);
                Assert.Equal(entityType.Members.Count, somEntityType.Members.Count);
            }
        }

        [Fact]
        public void Code_first_built_complex_types_matches_som_loaded_complex_types()
        {
            var context = new ShopContext_v1();
            var compiledModel = context.InternalContext.CodeFirstModel;

            var builder = compiledModel.CachedModelBuilder.Clone();

            var databaseMapping
                = builder.Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping;

            var itemCollection = new EdmItemCollection(databaseMapping.Model);

            var complexTypes = databaseMapping.Model.ComplexTypes.ToList();
            var somComplexTypes = itemCollection.GetItems<ComplexType>();

            Assert.Equal(complexTypes.Count(), somComplexTypes.Count());

            foreach (var complexType in complexTypes)
            {
                var somComplexType = somComplexTypes.Single(e => e.Name == complexType.Name);

                Assert.Equal(complexType.NamespaceName, somComplexType.NamespaceName);
                Assert.Equal(complexType.Identity, somComplexType.Identity);
                Assert.Equal(complexType.Abstract, somComplexType.Abstract);
                Assert.Equal(complexType.FullName, somComplexType.FullName);
                Assert.Equal(complexType.Members.Count, somComplexType.Members.Count);
            }
        }

        [Fact]
        public void Code_first_built_enum_types_matches_som_loaded_enum_types()
        {
            var context = new ShopContext_v3();
            var compiledModel = context.InternalContext.CodeFirstModel;

            var builder = compiledModel.CachedModelBuilder.Clone();

            var databaseMapping
                = builder.Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping;

            var itemCollection = new EdmItemCollection(databaseMapping.Model);

            var enumTypes = databaseMapping.Model.EnumTypes.ToList();
            var somEnumTypes = itemCollection.GetItems<EnumType>();

            Assert.Equal(enumTypes.Count(), somEnumTypes.Count());

            foreach (var enumType in enumTypes)
            {
                var somEnumType = somEnumTypes.Single(e => e.Name == enumType.Name);

                Assert.Equal(enumType.NamespaceName, somEnumType.NamespaceName);
                Assert.Equal(enumType.Identity, somEnumType.Identity);
                Assert.Equal(enumType.Abstract, somEnumType.Abstract);
                Assert.Equal(enumType.FullName, somEnumType.FullName);
                Assert.Equal(enumType.Members.Count, somEnumType.Members.Count);
            }
        }

        [Fact]
        public void Code_first_built_association_types_matches_som_loaded_association_types()
        {
            var context = new ShopContext_v3();
            var compiledModel = context.InternalContext.CodeFirstModel;

            var builder = compiledModel.CachedModelBuilder.Clone();

            var databaseMapping
                = builder.Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping;

            var itemCollection = new EdmItemCollection(databaseMapping.Model);

            var associationTypes = databaseMapping.Model.AssociationTypes.ToList();
            var somAssociationTypes = itemCollection.GetItems<AssociationType>();

            Assert.Equal(associationTypes.Count(), somAssociationTypes.Count());

            foreach (var associationType in associationTypes)
            {
                var somAssociationType = somAssociationTypes.Single(e => e.Name == associationType.Name);

                Assert.Equal(associationType.NamespaceName, somAssociationType.NamespaceName);
                Assert.Equal(associationType.Identity, somAssociationType.Identity);
                Assert.Equal(associationType.Abstract, somAssociationType.Abstract);
                Assert.Equal(associationType.FullName, somAssociationType.FullName);
                Assert.Equal(associationType.Members.Count, somAssociationType.Members.Count);
            }
        }

        [Fact]
        public void EdmItemCollection_Create_factory_method_throws_for_null_readers()
        {
            IList<EdmSchemaError> errors;
            
            Assert.Equal("xmlReaders",
                Assert.Throws<ArgumentNullException>(
                    () => EdmItemCollection.Create(null, null, out errors)).ParamName);
        }

        [Fact]
        public void EdmItemCollection_Create_factory_method_throws_for_null_reader_in_the_collection()
        {
            IList<EdmSchemaError> errors;

            Assert.Equal(Strings.CheckArgumentContainsNullFailed("xmlReaders"),
                Assert.Throws<ArgumentException>(
                    () => EdmItemCollection.Create(new XmlReader[1], null, out errors)).Message);
        }

        [Fact]
        public void EdmItemCollection_Create_factory_method_returns_null_and_errors_for_invalid_csdl()
        {
            var csdl = 
                XDocument.Parse(
                    "<Schema Namespace='Model' p1:UseStrongSpatialTypes='false' " +
                    "  xmlns:p1='http://schemas.microsoft.com/ado/2009/02/edm/annotation' " +
                    "  xmlns='http://schemas.microsoft.com/ado/2009/11/edm'>" +
                    "  <EnumType Name='Invalid Name' />" + 
                    "</Schema>");

            IList<EdmSchemaError> errors;
            var edmItemCollection = EdmItemCollection.Create(new[] { csdl.CreateReader() }, null, out errors);

            Assert.Null(edmItemCollection);
            Assert.Equal(1, errors.Count);
            Assert.Contains("'Invalid Name'", errors[0].Message);
        }

        [Fact]
        public void EdmItemCollection_Create_factory_method_returns_EdmItemCollection_instance_for_valid_csdl()
        {
            var csdl =
                XDocument.Parse(
                    "<Schema Namespace='Model' p1:UseStrongSpatialTypes='false' " +
                    "  xmlns:p1='http://schemas.microsoft.com/ado/2009/02/edm/annotation' " +
                    "  xmlns='http://schemas.microsoft.com/ado/2009/11/edm'>" +
                    "  <EnumType Name='Color' />" +
                    "</Schema>");

            IList<EdmSchemaError> errors;
            var edmItemCollection = EdmItemCollection.Create(new[] { csdl.CreateReader() }, null, out errors);

            Assert.NotNull(edmItemCollection);
            Assert.NotNull(edmItemCollection.GetItem<EnumType>("Model.Color"));
            Assert.Equal(0, errors.Count);            
        }
    }
}
