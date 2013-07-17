// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Creates a convention that configures stored procedures to be used to modify entities in the database.
    /// </summary>
    public class ConventionModificationFunctionsConfiguration
    {
        private readonly Type _type;

        private readonly ModificationFunctionsConfiguration _configuration
            = new ModificationFunctionsConfiguration();

        internal ConventionModificationFunctionsConfiguration(Type type)
        {
            DebugCheck.NotNull(type);

            _type = type;
        }

        internal ModificationFunctionsConfiguration Configuration
        {
            get { return _configuration; }
        }

        /// <summary>Configures stored procedure used to insert entities.</summary>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        /// <param name="modificationFunctionConfigurationAction">A lambda expression that performs configuration for the stored procedure.</param>
        public ConventionModificationFunctionsConfiguration Insert(
            Action<ConventionInsertModificationFunctionConfiguration> modificationFunctionConfigurationAction)
        {
            Check.NotNull(modificationFunctionConfigurationAction, "modificationFunctionConfigurationAction");

            var modificationFunctionConfiguration
                = new ConventionInsertModificationFunctionConfiguration(_type);

            modificationFunctionConfigurationAction(modificationFunctionConfiguration);

            _configuration.Insert(modificationFunctionConfiguration.Configuration);

            return this;
        }

        /// <summary>Configures stored procedure used to update entities.</summary>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        /// <param name="modificationFunctionConfigurationAction">A lambda expression that performs configuration for the stored procedure.</param>
        public ConventionModificationFunctionsConfiguration Update(
            Action<ConventionUpdateModificationFunctionConfiguration> modificationFunctionConfigurationAction)
        {
            Check.NotNull(modificationFunctionConfigurationAction, "modificationFunctionConfigurationAction");

            var modificationFunctionConfiguration
                = new ConventionUpdateModificationFunctionConfiguration(_type);

            modificationFunctionConfigurationAction(modificationFunctionConfiguration);

            _configuration.Update(modificationFunctionConfiguration.Configuration);

            return this;
        }

        /// <summary>Configures stored procedure used to delete entities.</summary>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        /// <param name="modificationFunctionConfigurationAction">A lambda expression that performs configuration for the stored procedure.</param>
        public ConventionModificationFunctionsConfiguration Delete(
            Action<ConventionDeleteModificationFunctionConfiguration> modificationFunctionConfigurationAction)
        {
            Check.NotNull(modificationFunctionConfigurationAction, "modificationFunctionConfigurationAction");

            var modificationFunctionConfiguration
                = new ConventionDeleteModificationFunctionConfiguration(_type);

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
