// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Used to configure an independent constraint on a navigation property.
    /// </summary>
    public class IndependentConstraintConfiguration : ConstraintConfiguration
    {
        private static readonly ConstraintConfiguration _instance = new IndependentConstraintConfiguration();

        private IndependentConstraintConfiguration()
        {
        }

        /// <summary>
        ///     Gets the Singleton instance of the IndependentConstraintConfiguration class.
        /// </summary>
        public static ConstraintConfiguration Instance
        {
            get { return _instance; }
        }

        internal override ConstraintConfiguration Clone()
        {
            return _instance;
        }

        internal override void Configure(
            AssociationType associationType, AssociationEndMember dependentEnd,
            EntityTypeConfiguration entityTypeConfiguration)
        {
            DebugCheck.NotNull(associationType);
            DebugCheck.NotNull(dependentEnd);
            DebugCheck.NotNull(entityTypeConfiguration);

            associationType.MarkIndependent();
        }
    }
}
