namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using Xunit;

    public sealed class ManyToManyCascadeDeleteConventionTests
    {
        [Fact]
        public void Apply_should_introduce_cascade_delete_on_constraints()
        {
            var databaseMapping
                = new DbDatabaseMapping()
                    .Initialize(new EdmModel().Initialize(), new DbDatabaseMetadata().Initialize());

            var foreignKeyConstraint = new DbForeignKeyConstraintMetadata();

            Assert.Equal(DbOperationAction.None, foreignKeyConstraint.DeleteAction);

            var table = new DbTableMetadata();
            table.ForeignKeyConstraints.Add(foreignKeyConstraint);

            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EndKind = EdmAssociationEndKind.Many;
            associationType.SourceEnd.EntityType = new EdmEntityType();
            associationType.TargetEnd.EndKind = EdmAssociationEndKind.Many;
            associationType.TargetEnd.EntityType = new EdmEntityType();

            var associationSetMapping = databaseMapping.AddAssociationSetMapping(new EdmAssociationSet { ElementType = associationType });
            associationSetMapping.Table = table;

            ((IDbMappingConvention)new ManyToManyCascadeDeleteConvention()).Apply(databaseMapping);

            Assert.Equal(DbOperationAction.Cascade, foreignKeyConstraint.DeleteAction);
        }
    }
}