// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    /// <summary>
    ///     Base class for performing configuration of a relationship.
    ///     This configuration functionality is available via the Code First Fluent API, see <see cref="DbModelBuilder" />.
    /// </summary>
    [ContractClass(typeof(AssociationMappingConfigurationContracts))]
    public abstract class AssociationMappingConfiguration
    {
        internal abstract void Configure(
            DbAssociationSetMapping associationSetMapping,
            DbDatabaseMetadata database,
            PropertyInfo navigationProperty);

        internal abstract AssociationMappingConfiguration Clone();

        #region Base Member Contracts

        [ContractClassFor(typeof(AssociationMappingConfiguration))]
        private abstract class AssociationMappingConfigurationContracts : AssociationMappingConfiguration
        {
            internal override void Configure(
                DbAssociationSetMapping associationSetMapping,
                DbDatabaseMetadata database,
                PropertyInfo navigationProperty)
            {
                Contract.Requires(associationSetMapping != null);
                Contract.Requires(database != null);
                Contract.Requires(navigationProperty != null);
            }
        }

        #endregion
    }
}
