namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Linq;

    /// <summary>
    ///     Convention to add a cascade delete to the join table from both tables involved in a many to many relationship.
    /// </summary>
    public sealed class ManyToManyCascadeDeleteConvention : IDbMappingConvention
    {
        internal ManyToManyCascadeDeleteConvention()
        {
        }

        void IDbMappingConvention.Apply(DbDatabaseMapping databaseMapping)
        {
            databaseMapping.EntityContainerMappings
                .SelectMany(ecm => ecm.AssociationSetMappings)
                .Where(
                    asm => asm.AssociationSet.ElementType.IsManyToMany()
                           && !asm.AssociationSet.ElementType.IsSelfReferencing())
                .SelectMany(asm => asm.Table.ForeignKeyConstraints)
                .Each(fk => fk.DeleteAction = DbOperationAction.Cascade);
        }
    }
}
