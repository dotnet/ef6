// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using Xunit;

    public sealed class ManyToManyCascadeDeleteConventionTests
    {
        [Fact]
        public void Apply_should_introduce_cascade_delete_on_constraints()
        {
            var databaseMapping
                = new DbDatabaseMapping()
                    .Initialize(new EdmModel(DataSpace.CSpace), new EdmModel(DataSpace.SSpace));

            var foreignKeyConstraint
                = new ForeignKeyBuilder(databaseMapping.Database, "FK")
                      {
                          PrincipalTable = databaseMapping.Database.AddTable("P")
                      };

            Assert.Equal(OperationAction.None, foreignKeyConstraint.DeleteAction);

            var table = databaseMapping.Database.AddTable("T");

            table.AddForeignKey(foreignKeyConstraint);

            var associationType
                = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)
                      {
                          SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace)),
                          TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace))
                      };

            associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.Many;
            associationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.Many;

            var associationSetMapping = databaseMapping.AddAssociationSetMapping(
                new AssociationSet("AS", associationType), new EntitySet());

            associationSetMapping.StoreEntitySet = databaseMapping.Database.GetEntitySet(table);

            ((IDbMappingConvention)new ManyToManyCascadeDeleteConvention()).Apply(databaseMapping);

            Assert.Equal(OperationAction.Cascade, foreignKeyConstraint.DeleteAction);
        }
    }
}
