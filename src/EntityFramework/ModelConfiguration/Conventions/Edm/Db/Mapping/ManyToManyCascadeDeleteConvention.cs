// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Convention to add a cascade delete to the join table from both tables involved in a many to many relationship.
    /// </summary>
    public class ManyToManyCascadeDeleteConvention : IDbMappingConvention
    {
        void IDbMappingConvention.Apply(DbDatabaseMapping databaseMapping)
        {
            Check.NotNull(databaseMapping, "databaseMapping");

            databaseMapping.EntityContainerMappings
                           .SelectMany(ecm => ecm.AssociationSetMappings)
                           .Where(
                               asm => asm.AssociationSet.ElementType.IsManyToMany()
                                      && !asm.AssociationSet.ElementType.IsSelfReferencing())
                           .SelectMany(asm => asm.Table.ForeignKeyBuilders)
                           .Each(fk => fk.DeleteAction = OperationAction.Cascade);
        }
    }
}
