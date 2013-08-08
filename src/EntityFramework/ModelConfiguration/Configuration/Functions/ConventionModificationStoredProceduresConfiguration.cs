// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Creates a convention that configures stored procedures to be used to modify entities in the database.
    /// </summary>
    public class ConventionModificationStoredProceduresConfiguration
    {
        private readonly Type _type;

        private readonly ModificationStoredProceduresConfiguration _configuration
            = new ModificationStoredProceduresConfiguration();

        internal ConventionModificationStoredProceduresConfiguration(Type type)
        {
            DebugCheck.NotNull(type);

            _type = type;
        }

        internal ModificationStoredProceduresConfiguration Configuration
        {
            get { return _configuration; }
        }

        /// <summary>Configures stored procedure used to insert entities.</summary>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        /// <param name="modificationStoredProcedureConfigurationAction">A lambda expression that performs configuration for the stored procedure.</param>
        public ConventionModificationStoredProceduresConfiguration Insert(
            Action<ConventionInsertModificationStoredProcedureConfiguration> modificationStoredProcedureConfigurationAction)
        {
            Check.NotNull(modificationStoredProcedureConfigurationAction, "modificationStoredProcedureConfigurationAction");

            var modificationStoredProcedureConfiguration
                = new ConventionInsertModificationStoredProcedureConfiguration(_type);

            modificationStoredProcedureConfigurationAction(modificationStoredProcedureConfiguration);

            _configuration.Insert(modificationStoredProcedureConfiguration.Configuration);

            return this;
        }

        /// <summary>Configures stored procedure used to update entities.</summary>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        /// <param name="modificationStoredProcedureConfigurationAction">A lambda expression that performs configuration for the stored procedure.</param>
        public ConventionModificationStoredProceduresConfiguration Update(
            Action<ConventionUpdateModificationStoredProcedureConfiguration> modificationStoredProcedureConfigurationAction)
        {
            Check.NotNull(modificationStoredProcedureConfigurationAction, "modificationStoredProcedureConfigurationAction");

            var modificationStoredProcedureConfiguration
                = new ConventionUpdateModificationStoredProcedureConfiguration(_type);

            modificationStoredProcedureConfigurationAction(modificationStoredProcedureConfiguration);

            _configuration.Update(modificationStoredProcedureConfiguration.Configuration);

            return this;
        }

        /// <summary>Configures stored procedure used to delete entities.</summary>
        /// <returns> The same configuration instance so that multiple calls can be chained. </returns>
        /// <param name="modificationStoredProcedureConfigurationAction">A lambda expression that performs configuration for the stored procedure.</param>
        public ConventionModificationStoredProceduresConfiguration Delete(
            Action<ConventionDeleteModificationStoredProcedureConfiguration> modificationStoredProcedureConfigurationAction)
        {
            Check.NotNull(modificationStoredProcedureConfigurationAction, "modificationStoredProcedureConfigurationAction");

            var modificationStoredProcedureConfiguration
                = new ConventionDeleteModificationStoredProcedureConfiguration(_type);

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
