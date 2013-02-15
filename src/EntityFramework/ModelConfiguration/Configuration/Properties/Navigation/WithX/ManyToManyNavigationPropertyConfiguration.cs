// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Configures a many:many relationship.
    ///     This configuration functionality is available via the Code First Fluent API, see <see cref="DbModelBuilder" />.
    /// </summary>
    public class ManyToManyNavigationPropertyConfiguration<TEntityType, TTargetEntityType>
        where TEntityType : class
        where TTargetEntityType : class
    {
        private readonly NavigationPropertyConfiguration _navigationPropertyConfiguration;

        internal ManyToManyNavigationPropertyConfiguration(
            NavigationPropertyConfiguration navigationPropertyConfiguration)
        {
            DebugCheck.NotNull(navigationPropertyConfiguration);

            _navigationPropertyConfiguration = navigationPropertyConfiguration;
        }

        /// <summary>
        ///     Configures the foreign key column(s) and table used to store the relationship.
        /// </summary>
        /// <param name="configurationAction"> Action that configures the foreign key column(s) and table. </param>
        public ManyToManyNavigationPropertyConfiguration<TEntityType, TTargetEntityType> Map(
            Action<ManyToManyAssociationMappingConfiguration> configurationAction)
        {
            Check.NotNull(configurationAction, "configurationAction");

            var manyToManyMappingConfiguration = new ManyToManyAssociationMappingConfiguration();

            configurationAction(manyToManyMappingConfiguration);

            _navigationPropertyConfiguration.AssociationMappingConfiguration = manyToManyMappingConfiguration;

            return this;
        }

        public ManyToManyNavigationPropertyConfiguration<TEntityType, TTargetEntityType> MapToStoredProcedures()
        {
            _navigationPropertyConfiguration.ModificationFunctionsConfiguration
                = new ModificationFunctionsConfiguration();

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyNavigationPropertyConfiguration<TEntityType, TTargetEntityType> MapToStoredProcedures(
            Action<ManyToManyModificationFunctionsConfiguration<TEntityType, TTargetEntityType>>
                modificationFunctionMappingConfigurationAction)
        {
            Check.NotNull(modificationFunctionMappingConfigurationAction, "modificationFunctionMappingConfigurationAction");

            var modificationFunctionMappingConfiguration
                = new ManyToManyModificationFunctionsConfiguration<TEntityType, TTargetEntityType>();

            modificationFunctionMappingConfigurationAction(modificationFunctionMappingConfiguration);

            _navigationPropertyConfiguration.ModificationFunctionsConfiguration
                = modificationFunctionMappingConfiguration.Configuration;

            return this;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
