// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Convention to configure integer primary keys to be identity.
    /// </summary>
    public class StoreGeneratedIdentityKeyConvention : IConceptualModelConvention<EntityType>
    {
        private static readonly IEnumerable<PrimitiveTypeKind> _applicableTypes
            = new[] { PrimitiveTypeKind.Int16, PrimitiveTypeKind.Int32, PrimitiveTypeKind.Int64 };

        /// <inheritdoc />
        public virtual void Apply(EntityType item, DbModel model)
        {
            Check.NotNull(item, "item");
            Check.NotNull(model, "model");

            Debug.Assert(item.KeyProperties != null);

            if ((item.BaseType == null && item.KeyProperties.Count == 1)
                && !(from p in item.DeclaredProperties
                     let sgp = p.GetStoreGeneratedPattern()
                     where sgp != null && sgp == StoreGeneratedPattern.Identity
                     select sgp).Any()) // Entity already has an Identity property.
            {
                var property = item.KeyProperties.Single();

                Debug.Assert(property.TypeUsage != null);

                if ((property.GetStoreGeneratedPattern() == null)
                    && property.PrimitiveType != null
                    && _applicableTypes.Contains(property.PrimitiveType.PrimitiveTypeKind))
                {
                    if (!model.GetConceptualModel().AssociationTypes.Any(a => IsNonTableSplittingForeignKey(a, property))
                        && !ParentOfTpc(item, model.GetConceptualModel()))
                    {
                        property.SetStoreGeneratedPattern(StoreGeneratedPattern.Identity);
                    }
                }
            }
        }
        
        /// <summary>
        /// Checks for the PK property being an FK in a different table. A PK which is also an FK but
        /// in the same table is used for table splitting and can still be an identity column because
        /// the update pipeline is only inserting into one column of one table.
        /// </summary>
        private static bool IsNonTableSplittingForeignKey(AssociationType association, EdmProperty property)
        {
            if (association.Constraint != null
                && association.Constraint.ToProperties.Contains(property))
            {
                var sourceConfig = (EntityTypeConfiguration)association.SourceEnd.GetEntityType().GetConfiguration();
                var targetConfig = (EntityTypeConfiguration)association.TargetEnd.GetEntityType().GetConfiguration();

                return sourceConfig == null
                       || targetConfig == null
                       || sourceConfig.GetTableName() == null
                       || targetConfig.GetTableName() == null
                       || !sourceConfig.GetTableName().Equals(targetConfig.GetTableName());
            }
            return false;
        }

        private static bool ParentOfTpc(EntityType entityType, EdmModel model)
        {
            return (from e in model.EntityTypes.Where(et => et.GetRootType() == entityType)
                    let configuration = e.GetConfiguration() as EntityTypeConfiguration
                    where configuration != null && configuration.IsMappingAnyInheritedProperty(e)
                    select e).Any();
        }
    }
}
