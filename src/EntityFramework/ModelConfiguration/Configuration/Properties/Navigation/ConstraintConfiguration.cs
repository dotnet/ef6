// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;

    /// <summary>
    ///     Used to configure a constraint on a navigation property.
    /// </summary>
    public abstract class ConstraintConfiguration
    {
        internal abstract ConstraintConfiguration Clone();

        internal abstract void Configure(
            AssociationType associationType, AssociationEndMember dependentEnd,
            EntityTypeConfiguration entityTypeConfiguration);

        /// <summary>
        ///     Gets a value indicating whether the constraint has been fully specified
        ///     using the Code First Fluent API.
        /// </summary>
        public virtual bool IsFullySpecified
        {
            get { return true; }
        }
    }
}
