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
    public class ModificationFunctionsConfiguration<TEntityType>
        where TEntityType : class
    {
        private readonly ModificationFunctionsConfiguration _configuration
            = new ModificationFunctionsConfiguration();

        internal ModificationFunctionsConfiguration()
        {
        }

        internal ModificationFunctionsConfiguration Configuration
        {
            get { return _configuration; }
        }

        /// <summary>Configures stored procedure used to insert entities.</summary>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        /// <param name="modificationFunctionConfigurationAction">A lambda expression that performs configuration for the stored procedure.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ModificationFunctionsConfiguration<TEntityType> Insert(
            Action<InsertModificationFunctionConfiguration<TEntityType>> modificationFunctionConfigurationAction)
        {
            Check.NotNull(modificationFunctionConfigurationAction, "modificationFunctionConfigurationAction");

            var modificationFunctionConfiguration
                = new InsertModificationFunctionConfiguration<TEntityType>();

            modificationFunctionConfigurationAction(modificationFunctionConfiguration);

            _configuration.Insert(modificationFunctionConfiguration.Configuration);

            return this;
        }

        /// <summary>Configures stored procedure used to update entities.</summary>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        /// <param name="modificationFunctionConfigurationAction">A lambda expression that performs configuration for the stored procedure.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ModificationFunctionsConfiguration<TEntityType> Update(
            Action<UpdateModificationFunctionConfiguration<TEntityType>> modificationFunctionConfigurationAction)
        {
            Check.NotNull(modificationFunctionConfigurationAction, "modificationFunctionConfigurationAction");

            var modificationFunctionConfiguration
                = new UpdateModificationFunctionConfiguration<TEntityType>();

            modificationFunctionConfigurationAction(modificationFunctionConfiguration);

            _configuration.Update(modificationFunctionConfiguration.Configuration);

            return this;
        }

        /// <summary>Configures stored procedure used to delete entities.</summary>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        /// <param name="modificationFunctionConfigurationAction">A lambda expression that performs configuration for the stored procedure.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ModificationFunctionsConfiguration<TEntityType> Delete(
            Action<DeleteModificationFunctionConfiguration<TEntityType>> modificationFunctionConfigurationAction)
        {
            Check.NotNull(modificationFunctionConfigurationAction, "modificationFunctionConfigurationAction");

            var modificationFunctionConfiguration
                = new DeleteModificationFunctionConfiguration<TEntityType>();

            modificationFunctionConfigurationAction(modificationFunctionConfiguration);

            _configuration.Delete(modificationFunctionConfiguration.Configuration);

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
