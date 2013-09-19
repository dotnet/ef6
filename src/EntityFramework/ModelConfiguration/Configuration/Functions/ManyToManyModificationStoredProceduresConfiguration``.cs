// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Allows configuration to be performed for a stored procedure that is used to modify a many to many relationship.
    /// </summary>
    /// <typeparam name="TEntityType">The type of the entity that the relationship is being configured from.</typeparam>
    /// <typeparam name="TTargetEntityType">The type of the entity that the other end of the relationship targets.</typeparam>
    public class ManyToManyModificationStoredProceduresConfiguration<TEntityType, TTargetEntityType>
        where TEntityType : class
        where TTargetEntityType : class
    {
        private readonly ModificationStoredProceduresConfiguration _configuration
            = new ModificationStoredProceduresConfiguration();

        internal ManyToManyModificationStoredProceduresConfiguration()
        {
        }

        internal ModificationStoredProceduresConfiguration Configuration
        {
            get { return _configuration; }
        }

        /// <summary>Configures stored procedure used to insert relationships.</summary>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        /// <param name="modificationStoredProcedureConfigurationAction">A lambda expression that performs configuration for the stored procedure.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationStoredProceduresConfiguration<TEntityType, TTargetEntityType> Insert(
            Action<ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType>> modificationStoredProcedureConfigurationAction)
        {
            Check.NotNull(modificationStoredProcedureConfigurationAction, "modificationStoredProcedureConfigurationAction");

            var modificationStoredProcedureConfiguration
                = new ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType>();

            modificationStoredProcedureConfigurationAction(modificationStoredProcedureConfiguration);

            _configuration.Insert(modificationStoredProcedureConfiguration.Configuration);

            return this;
        }

        /// <summary>Configures stored procedure used to delete relationships.</summary>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        /// <param name="modificationStoredProcedureConfigurationAction">A lambda expression that performs configuration for the stored procedure.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationStoredProceduresConfiguration<TEntityType, TTargetEntityType> Delete(
            Action<ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType>> modificationStoredProcedureConfigurationAction)
        {
            Check.NotNull(modificationStoredProcedureConfigurationAction, "modificationStoredProcedureConfigurationAction");

            var modificationStoredProcedureConfiguration
                = new ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType>();

            modificationStoredProcedureConfigurationAction(modificationStoredProcedureConfiguration);

            _configuration.Delete(modificationStoredProcedureConfiguration.Configuration);

            return this;
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Gets the <see cref="Type" /> of the current instance.
        /// </summary>
        /// <returns>The exact runtime type of the current instance.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
