// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    public class ManyToManyModificationFunctionsConfiguration<TEntityType, TTargetEntityType>
        where TEntityType : class
        where TTargetEntityType : class
    {
        private readonly ModificationFunctionsConfiguration _configuration
            = new ModificationFunctionsConfiguration();

        internal ManyToManyModificationFunctionsConfiguration()
        {
        }

        internal ModificationFunctionsConfiguration Configuration
        {
            get { return _configuration; }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationFunctionsConfiguration<TEntityType, TTargetEntityType> Insert(
            Action<ManyToManyModificationFunctionConfiguration<TEntityType, TTargetEntityType>> modificationFunctionConfigurationAction)
        {
            Check.NotNull(modificationFunctionConfigurationAction, "modificationFunctionConfigurationAction");

            var modificationFunctionConfiguration
                = new ManyToManyModificationFunctionConfiguration<TEntityType, TTargetEntityType>();

            modificationFunctionConfigurationAction(modificationFunctionConfiguration);

            _configuration.Insert(modificationFunctionConfiguration.Configuration);

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ManyToManyModificationFunctionsConfiguration<TEntityType, TTargetEntityType> Delete(
            Action<ManyToManyModificationFunctionConfiguration<TEntityType, TTargetEntityType>> modificationFunctionConfigurationAction)
        {
            Check.NotNull(modificationFunctionConfigurationAction, "modificationFunctionConfigurationAction");

            var modificationFunctionConfiguration
                = new ManyToManyModificationFunctionConfiguration<TEntityType, TTargetEntityType>();

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
