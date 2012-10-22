// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Linq;
    using Xunit;

    public class ForeignKeyBuilderTests
    {
        [Fact]
        public void Name_should_return_association_type_name()
        {
            var foreignKeyBuilder = new ForeignKeyBuilder(
                new EdmModel
                    {
                        Version = 3.0
                    },
                "FK");

            Assert.Equal("FK", foreignKeyBuilder.Name);
        }

        [Fact]
        public void Can_get_and_set_principal_table()
        {
            var foreignKeyBuilder = new ForeignKeyBuilder(
                new EdmModel
                    {
                        Version = 3.0
                    },
                "FK");

            var principalTable = new EntityType("P", XmlConstants.TargetNamespace_3, DataSpace.SSpace);

            foreignKeyBuilder.PrincipalTable = principalTable;

            Assert.Same(principalTable, foreignKeyBuilder.PrincipalTable);
        }

        [Fact]
        public void Can_set_owner_and_corresponding_association_added_to_model()
        {
            var database
                = new EdmModel
                      {
                          Version = 3.0
                      }.DbInitialize();
            var foreignKeyBuilder = new ForeignKeyBuilder(database, "FK");

            var principalTable = new EntityType("P", XmlConstants.TargetNamespace_3, DataSpace.SSpace);

            foreignKeyBuilder.PrincipalTable = principalTable;

            var dependentTable = new EntityType("D", XmlConstants.TargetNamespace_3, DataSpace.SSpace);

            foreignKeyBuilder.SetOwner(dependentTable);

            var associationType = database.GetAssociationType("FK");

            Assert.NotNull(associationType);
            Assert.NotNull(associationType.SourceEnd);
            Assert.NotNull(associationType.TargetEnd);
            Assert.Same(principalTable, associationType.SourceEnd.GetEntityType());
            Assert.Equal("P", associationType.SourceEnd.Name);
            Assert.Same(dependentTable, associationType.TargetEnd.GetEntityType());
            Assert.Equal("D", associationType.TargetEnd.Name);

            var associationSet = database.GetAssociationSet(associationType);

            Assert.NotNull(associationSet);
        }

        [Fact]
        public void SetOwner_when_null_should_remove_association_type_from_model()
        {
            var database
                = new EdmModel
                      {
                          Version = 3.0
                      }.DbInitialize();
            var foreignKeyBuilder = new ForeignKeyBuilder(database, "FK");

            var principalTable = new EntityType("P", XmlConstants.TargetNamespace_3, DataSpace.SSpace);

            foreignKeyBuilder.PrincipalTable = principalTable;

            var dependentTable = new EntityType("D", XmlConstants.TargetNamespace_3, DataSpace.SSpace);

            foreignKeyBuilder.SetOwner(dependentTable);

            Assert.NotNull(database.GetAssociationType("FK"));

            foreignKeyBuilder.SetOwner(null);

            Assert.Null(database.GetAssociationType("FK"));
        }

        [Fact]
        public void SetOwner_when_self_ref_should_differentiate_target_end_name()
        {
            var database
                = new EdmModel
                      {
                          Version = 3.0
                      }.DbInitialize();
            var foreignKeyBuilder = new ForeignKeyBuilder(database, "FK");

            var table = new EntityType("T", XmlConstants.TargetNamespace_3, DataSpace.SSpace);

            foreignKeyBuilder.PrincipalTable = table;
            foreignKeyBuilder.SetOwner(table);

            var associationType = database.GetAssociationType("FK");

            Assert.Same(table, associationType.SourceEnd.GetEntityType());
            Assert.Equal("T", associationType.SourceEnd.Name);
            Assert.Same(table, associationType.TargetEnd.GetEntityType());
            Assert.Equal("TSelf", associationType.TargetEnd.Name);
        }

        [Fact]
        public void Set_principal_table_when_self_ref_should_differentiate_target_end_name()
        {
            var database
                = new EdmModel
                      {
                          Version = 3.0
                      }.DbInitialize();
            var foreignKeyBuilder = new ForeignKeyBuilder(database, "FK");

            var table = new EntityType("T", XmlConstants.TargetNamespace_3, DataSpace.SSpace);

            foreignKeyBuilder.PrincipalTable = new EntityType("P", XmlConstants.TargetNamespace_3, DataSpace.SSpace);
            foreignKeyBuilder.SetOwner(table);

            var associationType = database.GetAssociationType("FK");

            Assert.Equal("P", associationType.SourceEnd.Name);
            Assert.Equal("T", associationType.TargetEnd.Name);

            foreignKeyBuilder.PrincipalTable = table;

            Assert.Same(table, associationType.SourceEnd.GetEntityType());
            Assert.Equal("T", associationType.SourceEnd.Name);
            Assert.Same(table, associationType.TargetEnd.GetEntityType());
            Assert.Equal("TSelf", associationType.TargetEnd.Name);
        }

        [Fact]
        public void Can_get_and_set_dependent_columns_and_multiplicities_assigned()
        {
            var database
                = new EdmModel
                      {
                          Version = 3.0
                      }.DbInitialize();
            var foreignKeyBuilder = new ForeignKeyBuilder(database, "FK");

            var table = new EntityType("T", XmlConstants.TargetNamespace_3, DataSpace.SSpace);

            foreignKeyBuilder.PrincipalTable = table;
            foreignKeyBuilder.SetOwner(table);

            var property = EdmProperty.Primitive("K", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            foreignKeyBuilder.DependentColumns = new[] { property };

            Assert.Equal(1, foreignKeyBuilder.DependentColumns.Count());

            var associationType = database.GetAssociationType("FK");

            Assert.NotNull(associationType.Constraint);
            Assert.Same(property, associationType.Constraint.ToProperties.Single());
            Assert.Same(associationType.SourceEnd, associationType.Constraint.FromRole);
            Assert.Same(associationType.TargetEnd, associationType.Constraint.ToRole);
            Assert.Equal(RelationshipMultiplicity.Many, associationType.TargetEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, associationType.SourceEnd.RelationshipMultiplicity);

            property.Nullable = false;

            foreignKeyBuilder.DependentColumns = new[] { property };

            Assert.Equal(RelationshipMultiplicity.One, associationType.SourceEnd.RelationshipMultiplicity);
        }

        [Fact]
        public void Can_get_and_set_delete_action()
        {
            var database
                = new EdmModel
                      {
                          Version = 3.0
                      }.DbInitialize();
            var foreignKeyBuilder = new ForeignKeyBuilder(database, "FK");

            var table = new EntityType("T", XmlConstants.TargetNamespace_3, DataSpace.SSpace);

            foreignKeyBuilder.PrincipalTable = table;
            foreignKeyBuilder.SetOwner(table);
            foreignKeyBuilder.DeleteAction = OperationAction.Cascade;

            var associationType = database.GetAssociationType("FK");

            Assert.Equal(OperationAction.Cascade, associationType.SourceEnd.DeleteBehavior);
        }
    }
}
