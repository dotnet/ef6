// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Diagnostics.Contracts;

    [ContractClass(typeof(ConstraintConfigurationContracts))]
    internal abstract class ConstraintConfiguration
    {
        internal abstract ConstraintConfiguration Clone();

        internal abstract void Configure(
            EdmAssociationType associationType, EdmAssociationEnd dependentEnd,
            EntityTypeConfiguration entityTypeConfiguration);

        internal virtual bool IsFullySpecified
        {
            get { return true; }
        }

        #region Base Member Contracts

        [ContractClassFor(typeof(ConstraintConfiguration))]
        private abstract class ConstraintConfigurationContracts : ConstraintConfiguration
        {
            internal override void Configure(
                EdmAssociationType associationType, EdmAssociationEnd dependentEnd,
                EntityTypeConfiguration entityTypeConfiguration)
            {
                Contract.Requires(associationType != null);
                Contract.Requires(dependentEnd != null);
                Contract.Requires(entityTypeConfiguration != null);
            }
        }

        #endregion
    }
}
