// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

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

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ModificationFunctionsConfiguration<TEntityType> InsertFunction(
            Action<ModificationFunctionConfiguration<TEntityType>> modificationFunctionConfigurationAction)
        {
            Check.NotNull(modificationFunctionConfigurationAction, "modificationFunctionConfigurationAction");

            var modificationFunctionConfiguration
                = new ModificationFunctionConfiguration<TEntityType>();

            modificationFunctionConfigurationAction(modificationFunctionConfiguration);

            _configuration.InsertFunction(modificationFunctionConfiguration.Configuration);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ModificationFunctionsConfiguration<TEntityType> UpdateFunction(
            Action<ModificationFunctionConfiguration<TEntityType>> modificationFunctionConfigurationAction)
        {
            Check.NotNull(modificationFunctionConfigurationAction, "modificationFunctionConfigurationAction");

            var modificationFunctionConfiguration
                = new ModificationFunctionConfiguration<TEntityType>();

            modificationFunctionConfigurationAction(modificationFunctionConfiguration);

            _configuration.UpdateFunction(modificationFunctionConfiguration.Configuration);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ModificationFunctionsConfiguration<TEntityType> DeleteFunction(
            Action<ModificationFunctionConfiguration<TEntityType>> modificationFunctionConfigurationAction)
        {
            Check.NotNull(modificationFunctionConfigurationAction, "modificationFunctionConfigurationAction");

            var modificationFunctionConfiguration
                = new ModificationFunctionConfiguration<TEntityType>();

            modificationFunctionConfigurationAction(modificationFunctionConfiguration);

            _configuration.DeleteFunction(modificationFunctionConfiguration.Configuration);

            return this;
        }
    }
}
