// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     Convention to configure integer primary keys to be identity.
    /// </summary>
    public sealed class StoreGeneratedIdentityKeyConvention : IEdmConvention<EdmEntityType>
    {
        private static readonly IEnumerable<EdmPrimitiveTypeKind> _applicableTypes
            = new[] { EdmPrimitiveTypeKind.Int16, EdmPrimitiveTypeKind.Int32, EdmPrimitiveTypeKind.Int64 };

        internal StoreGeneratedIdentityKeyConvention()
        {
        }

        void IEdmConvention<EdmEntityType>.Apply(EdmEntityType entityType, EdmModel model)
        {
            Contract.Assert(entityType.DeclaredKeyProperties != null);

            if ((entityType.DeclaredKeyProperties.Count == 1)
                && !(from p in entityType.DeclaredProperties
                     let sgp = p.GetStoreGeneratedPattern()
                     where sgp != null && sgp == DbStoreGeneratedPattern.Identity
                     select sgp).Any()) // Entity already has an Identity property.
            {
                var property = entityType.DeclaredKeyProperties.Single();

                Contract.Assert(property.PropertyType != null);

                if ((property.GetStoreGeneratedPattern() == null)
                    && property.PropertyType.PrimitiveType != null
                    && _applicableTypes.Contains(property.PropertyType.PrimitiveType.PrimitiveTypeKind))
                {
                    if (!model.GetAssociationTypes().Any(a => IsNonTableSplittingForeignKey(a, property))
                        && !ParentOfTpc(entityType, model))
                    {
                        property.SetStoreGeneratedPattern(DbStoreGeneratedPattern.Identity);
                    }
                }
            }
        }

        /// <summary>
        ///     Checks for the PK property being an FK in a different table. A PK which is also an FK but
        ///     in the same table is used for table splitting and can still be an identity column because
        ///     the update pipeline is only inserting into one column of one table.
        /// </summary>
        private static bool IsNonTableSplittingForeignKey(EdmAssociationType association, EdmProperty property)
        {
            if (association.Constraint != null
                && association.Constraint.DependentProperties.Contains(property))
            {
                var sourceConfig = (EntityTypeConfiguration)association.SourceEnd.EntityType.GetConfiguration();
                var targetConfig = (EntityTypeConfiguration)association.TargetEnd.EntityType.GetConfiguration();

                return sourceConfig == null
                       || targetConfig == null
                       || sourceConfig.GetTableName() == null
                       || targetConfig.GetTableName() == null
                       || !sourceConfig.GetTableName().Equals(targetConfig.GetTableName());
            }
            return false;
        }

        private static bool ParentOfTpc(EdmEntityType entityType, EdmModel model)
        {
            return (from e in model.GetEntityTypes().Where(et => et.GetRootType() == entityType)
                    let configuration = e.GetConfiguration() as EntityTypeConfiguration
                    where configuration != null && configuration.IsMappingAnyInheritedProperty(e)
                    select e).Any();
        }
    }
}
