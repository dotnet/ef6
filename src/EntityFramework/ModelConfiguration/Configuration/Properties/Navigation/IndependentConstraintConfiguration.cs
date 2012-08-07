// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;

    internal class IndependentConstraintConfiguration : ConstraintConfiguration
    {
        private static readonly ConstraintConfiguration _instance = new IndependentConstraintConfiguration();

        private IndependentConstraintConfiguration()
        {
        }

        public static ConstraintConfiguration Instance
        {
            get { return _instance; }
        }

        internal override ConstraintConfiguration Clone()
        {
            return _instance;
        }

        internal override void Configure(
            EdmAssociationType associationType, EdmAssociationEnd dependentEnd,
            EntityTypeConfiguration entityTypeConfiguration)
        {
            associationType.MarkIndependent();
        }
    }
}
