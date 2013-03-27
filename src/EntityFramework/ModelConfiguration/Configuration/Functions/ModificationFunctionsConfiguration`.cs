// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
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
