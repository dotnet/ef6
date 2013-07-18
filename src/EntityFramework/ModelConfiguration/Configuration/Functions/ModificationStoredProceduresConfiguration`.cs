// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Allows configuration to be performed for a stored procedure that is used to modify entities.
    /// </summary>
    /// <typeparam name="TEntityType">The type of the entity that the stored procedure can be used to modify.</typeparam>
    public class ModificationStoredProceduresConfiguration<TEntityType>
        where TEntityType : class
    {
        private readonly ModificationStoredProceduresConfiguration _configuration
            = new ModificationStoredProceduresConfiguration();

        internal ModificationStoredProceduresConfiguration()
        {
        }

        internal ModificationStoredProceduresConfiguration Configuration
        {
            get { return _configuration; }
        }

        /// <summary>Configures stored procedure used to insert entities.</summary>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        /// <param name="modificationStoredProcedureConfigurationAction">A lambda expression that performs configuration for the stored procedure.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ModificationStoredProceduresConfiguration<TEntityType> Insert(
            Action<InsertModificationStoredProcedureConfiguration<TEntityType>> modificationStoredProcedureConfigurationAction)
        {
            Check.NotNull(modificationStoredProcedureConfigurationAction, "modificationStoredProcedureConfigurationAction");

            var modificationStoredProcedureConfiguration
                = new InsertModificationStoredProcedureConfiguration<TEntityType>();

            modificationStoredProcedureConfigurationAction(modificationStoredProcedureConfiguration);

            _configuration.Insert(modificationStoredProcedureConfiguration.Configuration);

            return this;
        }

        /// <summary>Configures stored procedure used to update entities.</summary>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        /// <param name="modificationStoredProcedureConfigurationAction">A lambda expression that performs configuration for the stored procedure.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ModificationStoredProceduresConfiguration<TEntityType> Update(
            Action<UpdateModificationStoredProcedureConfiguration<TEntityType>> modificationStoredProcedureConfigurationAction)
        {
            Check.NotNull(modificationStoredProcedureConfigurationAction, "modificationStoredProcedureConfigurationAction");

            var modificationStoredProcedureConfiguration
                = new UpdateModificationStoredProcedureConfiguration<TEntityType>();

            modificationStoredProcedureConfigurationAction(modificationStoredProcedureConfiguration);

            _configuration.Update(modificationStoredProcedureConfiguration.Configuration);

            return this;
        }

        /// <summary>Configures stored procedure used to delete entities.</summary>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        /// <param name="modificationStoredProcedureConfigurationAction">A lambda expression that performs configuration for the stored procedure.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ModificationStoredProceduresConfiguration<TEntityType> Delete(
            Action<DeleteModificationStoredProcedureConfiguration<TEntityType>> modificationStoredProcedureConfigurationAction)
        {
            Check.NotNull(modificationStoredProcedureConfigurationAction, "modificationStoredProcedureConfigurationAction");

            var modificationStoredProcedureConfiguration
                = new DeleteModificationStoredProcedureConfiguration<TEntityType>();

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

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
